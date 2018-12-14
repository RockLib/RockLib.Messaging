using System.Linq;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RockLib.Messaging.SQS
{
    /// <summary>
    /// An implementation of <see cref="IReceiver"/> that receives messages from SQS.
    /// </summary>
    public class SQSQueueReceiver : Receiver
    {
        private readonly IAmazonSQS _sqs;
        private readonly Thread _worker;

        private bool _stopped;

        private const int _defaultMaxMessages = 3;
        private const int _maxAcknowledgeAttempts = 3;
        private const int _maxReceiveAttempts = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSQueueReceiver"/> class.
        /// Uses a default implementation of the <see cref="AmazonSQSClient"/> to
        /// communicate with SQS.
        /// </summary>
        /// <param name="name">The name of the receiver.</param>
        /// <param name="queueUrl">The url of the SQS queue.</param>
        /// <param name="maxMessages">
        /// The maximum number of messages to return with each call to the SQS endpoint.
        /// Amazon SQS never returns more messages than this value (however, fewer messages
        /// might be returned). Valid values are 1 to 10.
        /// </param>
        /// <param name="autoAcknowledge">
        /// Whether messages will be automatically acknowledged after any event handlers execute.
        /// </param>
        /// <param name="parallelHandling">
        /// Whether, in the case of when multiple messages are received from an SQS request,
        /// messages are handled in parallel or sequentially.
        /// </param>
        public SQSQueueReceiver(string name,
            string queueUrl,
            int maxMessages = _defaultMaxMessages,
            bool autoAcknowledge = true,
            bool parallelHandling = false)
            : this(new AmazonSQSClient(), name, queueUrl, maxMessages, autoAcknowledge, parallelHandling)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSQueueReceiver"/> class.
        /// </summary>
        /// <param name="sqs">An object that communicates with SQS.</param>
        /// <param name="name">The name of the receiver.</param>
        /// <param name="queueUrl">The url of the SQS queue.</param>
        /// <param name="maxMessages">
        /// The maximum number of messages to return with each call to the SQS endpoint.
        /// Amazon SQS never returns more messages than this value (however, fewer messages
        /// might be returned). Valid values are 1 to 10.
        /// </param>
        /// <param name="autoAcknowledge">
        /// Whether messages will be automatically acknowledged after any event handlers execute.
        /// </param>
        /// <param name="parallelHandling">
        /// Whether, in the case of when multiple messages are received from an SQS request,
        /// messages are handled in parallel or sequentially.
        /// </param>
        public SQSQueueReceiver(IAmazonSQS sqs,
            string name,
            string queueUrl,
            int maxMessages = _defaultMaxMessages,
            bool autoAcknowledge = true,
            bool parallelHandling = false)
            : base(name)
        {
            if (maxMessages < 1 || maxMessages > 10)
                throw new ArgumentOutOfRangeException(nameof(maxMessages), "Value must be from 1 to 10, inclusive.");

            _sqs = sqs ?? throw new ArgumentNullException(nameof(sqs));
            QueueUrl = queueUrl ?? throw new ArgumentNullException(nameof(queueUrl));
            MaxMessages = maxMessages;
            AutoAcknwoledge = autoAcknowledge;
            ParallelHandling = parallelHandling;

            _worker = new Thread(DoStuff);
        }

        /// <summary>
        /// Gets the url of the SQS queue.
        /// </summary>
        public string QueueUrl { get; private set; }

        /// <summary>
        /// Gets the maximum number of messages to return with each call to the SQS endpoint.
        /// Amazon SQS never returns more messages than this value (however, fewer messages
        /// might be returned). Valid values are 1 to 10.
        /// </summary>
        public int MaxMessages { get; private set; }

        /// <summary>
        /// Gets a value indicating whether messages will be automatically acknowledged after
        /// any event handlers execute.
        /// </summary>
        public bool AutoAcknwoledge { get; private set; }

        /// <summary>
        /// Gets a value indicating whether, in the case of when multiple messages are received
        /// from an SQS request, messages are handled in parallel or sequentially.
        /// </summary>
        public bool ParallelHandling { get; private set; }

        /// <summary>
        /// Starts the polling background thread that listens for messages.
        /// </summary>
        protected override void Start()
        {
            if (!_worker.IsAlive && !_stopped)
            {
                _worker.Start();
            }
        }

        private void DoStuff()
        {
            bool? connected = null;

            while (!_stopped)
            {
                var receiveMessageRequest = new ReceiveMessageRequest
                {
                    MaxNumberOfMessages = MaxMessages,
                    QueueUrl = QueueUrl,
                    MessageAttributeNames = new List<string> { "*" }
                };

                ReceiveMessageResponse response = null;
                Exception exception = null;

                for (int i = 0; i < _maxReceiveAttempts; i++)
                {
                    try
                    {
                        response = Sync.OverAsync(() => _sqs.ReceiveMessageAsync(receiveMessageRequest));

                        if (response.HttpStatusCode == HttpStatusCode.OK)
                        {
                            if (connected != true)
                            {
                                connected = true;
                                OnConnected();
                            }

                            exception = null;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                }

                if (exception != null || response == null || response.HttpStatusCode != HttpStatusCode.OK)
                {
                    if (connected != false)
                    {
                        string GetErrorMessage()
                        {
                            if (exception != null)
                                return $"{exception.GetType().Name}: {exception.Message}";
                            if (response == null)
                                return "Null response returned from IAmazonSQS.ReceiveMessageAsync method.";
                            return $"Unsuccessful response returned from IAmazonSQS.ReceiveMessageAsync method: {(int)response.HttpStatusCode} {response.HttpStatusCode}";
                        }

                        connected = false;
                        OnDisconnected(GetErrorMessage());
                    }

                    Trace.TraceError($"Unable to receive SQS messages from AWS. Additional Information - {GetAdditionalInformation(response, null)}");
                    continue;
                }

                if (ParallelHandling)
                {
                    Parallel.ForEach(response.Messages, Handle);
                }
                else
                {
                    foreach (var message in response.Messages)
                    {
                        Handle(message);
                    }
                }
            }
        }

        private void Handle(Message message)
        {
            if (_stopped)
            {
                return;
            }

            var receiptHandle = message.ReceiptHandle;
            void DeleteMessage() => Delete(receiptHandle);

            var receiverMessage = new SQSReceiverMessage(message, DeleteMessage);

            try
            {                
                MessageHandler.OnMessageReceived(this, receiverMessage);
            }
            finally
            {
                if (AutoAcknwoledge)
                    DeleteMessage();
            }
        }

        private void Delete(string receiptHandle)
        {
            DeleteMessageResponse deleteResponse = null;

            for (int i = 0; i < _maxAcknowledgeAttempts; i++)
            {
                try
                {
                    deleteResponse = null;

                    deleteResponse = Sync.OverAsync(() => _sqs.DeleteMessageAsync(new DeleteMessageRequest
                    {
                        QueueUrl = QueueUrl,
                        ReceiptHandle = receiptHandle
                    }));

                    if (deleteResponse.HttpStatusCode == HttpStatusCode.OK)
                    {
                        return;
                    }
                }
                catch
                {
                }
            }

            Trace.TraceError($"Unable to delete SQS message. Additional Information - {GetAdditionalInformation(deleteResponse, receiptHandle)}");
        }

        /// <summary>
        /// Signals the polling background thread to exit then waits for it to finish.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            _stopped = true;
            _worker.Join();
            _sqs.Dispose();
            base.Dispose(disposing);
        }

        private static string GetAdditionalInformation(AmazonWebServiceResponse response, string receiptHandle)
        {
            if (response == null && receiptHandle == null)
            {
                return null;
            }

            if (response == null)
            {
                return $@"{{
   ""receiptHandle"": ""{receiptHandle}""
}}";
            }

            if (receiptHandle != null)
            {
                receiptHandle = $@",
   ""receiptHandle"": ""{receiptHandle}""";
            }

            var responseMetadata = "null";

            if (response.ResponseMetadata != null)
            {
                var metadata = "null";

                if (response.ResponseMetadata.Metadata != null)
                {
                    if (response.ResponseMetadata.Metadata.Count == 0)
                    {
                        metadata = "{}";
                    }
                    else
                    {
                        metadata = $@"{{
            {string.Join(",\r\n            ",
                            response.ResponseMetadata.Metadata.Select(x => $@"""{x.Key}"": ""{x.Value}"""))}
         }}";
                    }
                }

                var requestId = "null";

                if (response.ResponseMetadata.RequestId != null)
                {
                    requestId = $@"""{response.ResponseMetadata.RequestId}""";
                }

                responseMetadata = $@"{{
         ""RequestId"": {requestId},
         ""Metadata"": {metadata}
      }}";
            }

            return $@"{{
   ""response"": {{
      ""HttpStatusCode"": ""{response.HttpStatusCode}"",
      ""ContentLength"": {response.ContentLength},
      ""ResponseMetadata"": {responseMetadata}
   }}{receiptHandle}
}}";
        }
    }
}
