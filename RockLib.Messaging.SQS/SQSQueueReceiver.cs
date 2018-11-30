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
        private readonly string _queueUrl;
        private readonly int _maxMessages;
        private readonly bool _autoAcknwoledge;
        private readonly bool _parallelHandling;
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
        /// <param name="name">The configuration name.</param>
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
        /// <param name="name">The configuration name.</param>
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
            _queueUrl = queueUrl ?? throw new ArgumentNullException(nameof(queueUrl));
            _maxMessages = maxMessages;
            _autoAcknwoledge = autoAcknowledge;
            _parallelHandling = parallelHandling;

            _worker = new Thread(DoStuff);
        }

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
                    MaxNumberOfMessages = _maxMessages,
                    QueueUrl = _queueUrl,
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

                if (_parallelHandling)
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

            try
            {
                MessageHandler.OnMessageReceived(this, new SQSReceiverMessage(message, DeleteMessage));
            }
            finally
            {
                if (_autoAcknwoledge)
                    DeleteMessage();
            }
        }

        private void Delete(string receiptHandle)
        {
            Exception deleteException = null;
            DeleteMessageResponse deleteResponse = null;

            for (int i = 0; i < _maxAcknowledgeAttempts; i++)
            {
                try
                {
                    deleteException = null;
                    deleteResponse = null;

                    deleteResponse = Sync.OverAsync(() => _sqs.DeleteMessageAsync(new DeleteMessageRequest
                    {
                        QueueUrl = _queueUrl,
                        ReceiptHandle = receiptHandle
                    }));

                    if (deleteResponse.HttpStatusCode == HttpStatusCode.OK)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    deleteException = ex;
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
                return string.Format(@"{{
   ""receiptHandle"": ""{0}""
}}", receiptHandle);
            }

            if (receiptHandle != null)
            {
                receiptHandle = string.Format(@",
   ""receiptHandle"": ""{0}""", receiptHandle);
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
                        metadata = string.Format(@"{{
            {0}
         }}", string.Join(",\r\n            ",
                            response.ResponseMetadata.Metadata.Select(x => string.Format(@"""{0}"": ""{1}""", x.Key, x.Value))));
                    }
                }

                var requestId = "null";

                if (response.ResponseMetadata.RequestId != null)
                {
                    requestId = string.Format(@"""{0}""", response.ResponseMetadata.RequestId);
                }

                responseMetadata = string.Format(@"{{
         ""RequestId"": {0},
         ""Metadata"": {1}
      }}", requestId, metadata);
            }

            return string.Format(@"{{
   ""response"": {{
      ""HttpStatusCode"": ""{0}"",
      ""ContentLength"": {1},
      ""ResponseMetadata"": {2}
   }}{3}
}}", response.HttpStatusCode, response.ContentLength, responseMetadata, receiptHandle);
        }
    }
}
