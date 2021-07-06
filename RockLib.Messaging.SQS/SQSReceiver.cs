using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace RockLib.Messaging.SQS
{
    /// <summary>
    /// An implementation of <see cref="IReceiver"/> that receives messages from SQS.
    /// </summary>
    public class SQSReceiver : Receiver
    {
        private readonly Lazy<Task> _receiveMessagesTask;

        private bool _stopped;

        /// <summary>The default value for <see cref="MaxMessages"/>.</summary>
        public const int DefaultMaxMessages = 3;

        /// <summary>The default value for <see cref="WaitTimeSeconds"/>.</summary>
        public const int DefaultWaitTimeSeconds = 0;

        private const int _maxAcknowledgeAttempts = 3;
        private const int _maxReceiveAttempts = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSReceiver"/> class.
        /// Uses a default implementation of the <see cref="AmazonSQSClient"/> to
        /// communicate with SQS.
        /// </summary>
        /// <param name="name">The name of the receiver.</param>
        /// <param name="queueUrl">The url of the SQS queue.</param>
        /// <param name="region">The region of the SQS queue.</param>
        /// <param name="maxMessages">
        /// The maximum number of messages to return with each call to the SQS endpoint.
        /// Amazon SQS never returns more messages than this value (however, fewer messages
        /// might be returned). Valid values are 1 to 10.
        /// </param>
        /// <param name="autoAcknowledge">
        /// Whether messages will be automatically acknowledged after the message handler executes.
        /// </param>
        /// <param name="waitTimeSeconds">
        /// The duration (in seconds) for which calls to ReceiveMessage wait for a message
        /// to arrive in the queue before returning. If a message is available, the call returns
        /// sooner than WaitTimeSeconds. If no messages are available and the wait time expires,
        /// the call returns successfully with an empty list of messages.
        /// </param>
        /// <param name="unpackSNS">Whether to attempt to unpack the message body as an SNS message.</param>
        /// <param name="terminateMessageVisibilityTimeoutOnRollback">Whether to terminate the
        /// message visibility timeout when <see cref="SQSReceiverMessage.RollbackMessageAsync(CancellationToken)"/>
        /// is called. Terminating the message visibility timeout allows the message to immediately
        /// become available for queue consumers to process.</param>
        public SQSReceiver(string name,
            string queueUrl,
            string region = null,
            int maxMessages = DefaultMaxMessages,
            bool autoAcknowledge = true,
            int waitTimeSeconds = DefaultWaitTimeSeconds,
            bool unpackSNS = false,
            bool terminateMessageVisibilityTimeoutOnRollback = false)
            : this(region == null ? new AmazonSQSClient() : new AmazonSQSClient(RegionEndpoint.GetBySystemName(region)), name, queueUrl, maxMessages, autoAcknowledge, waitTimeSeconds, unpackSNS, terminateMessageVisibilityTimeoutOnRollback)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSReceiver"/> class.
        /// Uses a default implementation of the <see cref="AmazonSQSClient"/> to
        /// communicate with SQS.
        /// </summary>
        /// <param name="name">The name of the receiver.</param>
        /// <param name="queueUrl">The url of the SQS queue.</param>
        /// <param name="region">The region of the SQS queue.</param>
        /// <param name="maxMessages">
        /// The maximum number of messages to return with each call to the SQS endpoint.
        /// Amazon SQS never returns more messages than this value (however, fewer messages
        /// might be returned). Valid values are 1 to 10.
        /// </param>
        /// <param name="autoAcknowledge">
        /// Whether messages will be automatically acknowledged after the message handler executes.
        /// </param>
        /// <param name="waitTimeSeconds">
        /// The duration (in seconds) for which calls to ReceiveMessage wait for a message
        /// to arrive in the queue before returning. If a message is available, the call returns
        /// sooner than WaitTimeSeconds. If no messages are available and the wait time expires,
        /// the call returns successfully with an empty list of messages.
        /// </param>
        /// <param name="unpackSNS">Whether to attempt to unpack the message body as an SNS message.</param>
        public SQSReceiver(string name,
            string queueUrl,
            string region,
            int maxMessages,
            bool autoAcknowledge,
            int waitTimeSeconds,
            bool unpackSNS)
            : this(name, queueUrl, region, maxMessages, autoAcknowledge, waitTimeSeconds, unpackSNS, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSReceiver"/> class.
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
        /// Whether messages will be automatically acknowledged after the message handler executes.
        /// </param>
        /// <param name="waitTimeSeconds">
        /// The duration (in seconds) for which calls to ReceiveMessage wait for a message
        /// to arrive in the queue before returning. If a message is available, the call returns
        /// sooner than WaitTimeSeconds. If no messages are available and the wait time expires,
        /// the call returns successfully with an empty list of messages.
        /// </param>
        /// <param name="unpackSNS">Whether to attempt to unpack the message body as an SNS message.</param>
        /// <param name="terminateMessageVisibilityTimeoutOnRollback">Whether to terminate the
        /// message visibility timeout when <see cref="SQSReceiverMessage.RollbackMessageAsync(CancellationToken)"/>
        /// is called. Terminating the message visibility timeout allows the message to immediately
        /// become available for queue consumers to process.</param>
        public SQSReceiver(IAmazonSQS sqs,
            string name,
            string queueUrl,
            int maxMessages = DefaultMaxMessages,
            bool autoAcknowledge = true,
            int waitTimeSeconds = DefaultWaitTimeSeconds,
            bool unpackSNS = false,
            bool terminateMessageVisibilityTimeoutOnRollback = false)
            : base(name)
        {
            if (maxMessages < 1 || maxMessages > 10)
                throw new ArgumentOutOfRangeException(nameof(maxMessages), "Value must be from 1 to 10, inclusive.");
            if (waitTimeSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(waitTimeSeconds), "Value cannot be negative.");

            SQSClient = sqs ?? throw new ArgumentNullException(nameof(sqs));
            QueueUrl = queueUrl ?? throw new ArgumentNullException(nameof(queueUrl));
            MaxMessages = maxMessages;
            AutoAcknwoledge = autoAcknowledge;
            WaitTimeSeconds = waitTimeSeconds;
            UnpackSNS = unpackSNS;
            TerminateMessageVisibilityTimeoutOnRollback = terminateMessageVisibilityTimeoutOnRollback;

            _receiveMessagesTask = new Lazy<Task>(ReceiveMessages);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSReceiver"/> class.
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
        /// Whether messages will be automatically acknowledged after the message handler executes.
        /// </param>
        /// <param name="waitTimeSeconds">
        /// The duration (in seconds) for which calls to ReceiveMessage wait for a message
        /// to arrive in the queue before returning. If a message is available, the call returns
        /// sooner than WaitTimeSeconds. If no messages are available and the wait time expires,
        /// the call returns successfully with an empty list of messages.
        /// </param>
        /// <param name="unpackSNS">Whether to attempt to unpack the message body as an SNS message.</param>
        public SQSReceiver(IAmazonSQS sqs,
            string name,
            string queueUrl,
            int maxMessages,
            bool autoAcknowledge,
            int waitTimeSeconds,
            bool unpackSNS)
            : this(sqs, name, queueUrl, maxMessages, autoAcknowledge, waitTimeSeconds, unpackSNS, false)
        {
        }

        /// <summary>
        /// Gets the url of the SQS queue.
        /// </summary>
        public string QueueUrl { get; }

        /// <summary>
        /// Gets the maximum number of messages to return with each call to the SQS endpoint.
        /// Amazon SQS never returns more messages than this value (however, fewer messages
        /// might be returned). Valid values are 1 to 10.
        /// </summary>
        public int MaxMessages { get; }

        /// <summary>
        /// Gets a value indicating whether messages will be automatically acknowledged after
        /// the message handler executes.
        /// </summary>
        public bool AutoAcknwoledge { get; }

        /// <summary>
        /// Gets the duration (in seconds) for which calls to ReceiveMessage wait for a message
        /// to arrive in the queue before returning. If a message is available, the call returns
        /// sooner than WaitTimeSeconds. If no messages are available and the wait time expires,
        /// the call returns successfully with an empty list of messages.
        /// </summary>
        public int WaitTimeSeconds { get; }

        /// <summary>
        /// Gets a value indicating whether to attempt to unpack the message body as an SNS message.
        /// </summary>
        public bool UnpackSNS { get; }

        /// <summary>
        /// Gets a value indicating whether to terminate the message visibility timeout when
        /// <see cref="SQSReceiverMessage.RollbackMessageAsync(CancellationToken)"/> is called.
        /// Terminating the message visibility timeout allows the message to immediately become
        /// available for queue consumers
        /// to process.
        /// </summary>
        public bool TerminateMessageVisibilityTimeoutOnRollback { get; }

        /// <summary>
        /// Gets the object that communicates with SQS.
        /// </summary>
        public IAmazonSQS SQSClient { get; }

        /// <summary>
        /// Starts the polling background thread that listens for messages.
        /// </summary>
        protected override void Start()
        {
            if (!_receiveMessagesTask.IsValueCreated && !_stopped)
            {
                var dummy = _receiveMessagesTask.Value;
            }
        }

        private async Task ReceiveMessages()
        {
            await Task.Yield();

            bool? connected = null;

            while (!_stopped)
            {
                var receiveMessageRequest = new ReceiveMessageRequest
                {
                    MaxNumberOfMessages = MaxMessages,
                    QueueUrl = QueueUrl,
                    MessageAttributeNames = new List<string> { "*" },
                    WaitTimeSeconds = WaitTimeSeconds,
                    AttributeNames = new List<string> { "All" }
                };

                ReceiveMessageResponse response = null;
                Exception exception = null;

                for (int i = 0; i < _maxReceiveAttempts; i++)
                {
                    try
                    {
                        response = await SQSClient.ReceiveMessageAsync(receiveMessageRequest).ConfigureAwait(false);

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

                    OnError($"Unable to receive SQS messages from AWS. Additional Information - {GetAdditionalInformation(response, null)}", exception);
                    continue;
                }

                var tasks = response.Messages.Select(HandleAsync);
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private async Task HandleAsync(Message message)
        {
            if (_stopped)
                return;

            var receiptHandle = message.ReceiptHandle;

            Task DeleteMessageAsync(CancellationToken cancellationToken = default(CancellationToken)) =>
                DeleteAsync(receiptHandle, cancellationToken);

            Task RollbackMessageAsync(CancellationToken cancellationToken = default(CancellationToken)) =>
                RollbackAsync(receiptHandle, cancellationToken);

            var receiverMessage = new SQSReceiverMessage(message, DeleteMessageAsync, RollbackMessageAsync, UnpackSNS);

            try
            {
                await MessageHandler.OnMessageReceivedAsync(this, receiverMessage).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                OnError("Error in MessageHandler.OnMessageReceivedAsync.", ex);
            }
            finally
            {
                if (AutoAcknwoledge && !receiverMessage.Handled)
                {
                    try
                    {
                        await DeleteMessageAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        OnError("Error in AutoAcknowledge.", ex);
                    }
                }
            }
        }

        private Task DeleteAsync(string receiptHandle, CancellationToken cancellationToken)
        {
            return ExecuteWithRetry(async () =>
            {
                return await SQSClient.DeleteMessageAsync(new DeleteMessageRequest
                {
                    QueueUrl = QueueUrl,
                    ReceiptHandle = receiptHandle
                }, cancellationToken).ConfigureAwait(false);
            });
        }

        private Task RollbackAsync(string receiptHandle, CancellationToken cancellationToken)
        {
            if (!TerminateMessageVisibilityTimeoutOnRollback)
                return Task.FromResult(0);

            return ExecuteWithRetry(async () =>
            {
                return await SQSClient.ChangeMessageVisibilityAsync(new ChangeMessageVisibilityRequest
                {
                    VisibilityTimeout = 0,
                    QueueUrl = QueueUrl,
                    ReceiptHandle = receiptHandle
                }, cancellationToken).ConfigureAwait(false);
            });
        }

        private async Task ExecuteWithRetry(Func<Task<AmazonWebServiceResponse>> funcToExecute)
        {
            int i = 0;

            while (true)
            {
                try
                {
                    var response = await funcToExecute();
                    if (response.HttpStatusCode == HttpStatusCode.OK)
                        return;
                }
                catch
                {
                    if (i++ >= _maxAcknowledgeAttempts)
                        throw;
                }
            }
        }

        /// <summary>
        /// Signals the polling background thread to exit then waits for it to finish.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            _stopped = true;
            if (_receiveMessagesTask.IsValueCreated)
                try { _receiveMessagesTask.Value.Wait(); }
                catch { }
            SQSClient.Dispose();
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
