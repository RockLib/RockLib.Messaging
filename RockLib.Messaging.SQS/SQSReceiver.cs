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

        private bool _stopRequested;

        /// <summary>The default value for <see cref="MaxMessages"/>.</summary>
        public const int DefaultMaxMessages = 3;

        /// <summary>The default value for <see cref="WaitTimeSeconds"/>.</summary>
        public const int DefaultWaitTimeSeconds = 0;

        private const int _maxAcknowledgeAttempts = 3;
        private const int _maxReceiveAttempts = 3;

        private readonly CancellationTokenSource _consumerToken = new();

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
            Uri queueUrl,
            string? region = null,
            int maxMessages = DefaultMaxMessages,
            bool autoAcknowledge = true,
            int waitTimeSeconds = DefaultWaitTimeSeconds,
            bool unpackSNS = false,
            bool terminateMessageVisibilityTimeoutOnRollback = false)
            : this(region is null ? new AmazonSQSClient() : new AmazonSQSClient(RegionEndpoint.GetBySystemName(region)), name, queueUrl, maxMessages, autoAcknowledge, waitTimeSeconds, unpackSNS, terminateMessageVisibilityTimeoutOnRollback)
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
            Uri queueUrl,
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
            Uri queueUrl,
            int maxMessages = DefaultMaxMessages,
            bool autoAcknowledge = true,
            int waitTimeSeconds = DefaultWaitTimeSeconds,
            bool unpackSNS = false,
            bool terminateMessageVisibilityTimeoutOnRollback = false)
            : base(name)
        {
            if (maxMessages < 1 || maxMessages > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(maxMessages), "Value must be from 1 to 10, inclusive.");
            }
            if (waitTimeSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(waitTimeSeconds), "Value cannot be negative.");
            }

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
            Uri queueUrl,
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
        public Uri? QueueUrl { get; }

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
            if (!_receiveMessagesTask.IsValueCreated && !_stopRequested)
            {
                _ = _receiveMessagesTask.Value;
            }
        }

        private async Task ReceiveMessages()
        {
            await Task.Yield();

            bool? connected = null;

            while (!_stopRequested)
            {
                var receiveMessageRequest = new ReceiveMessageRequest
                {
                    MaxNumberOfMessages = MaxMessages,
                    QueueUrl = QueueUrl?.OriginalString,
                    MessageAttributeNames = new List<string> { ".*" },
                    WaitTimeSeconds = WaitTimeSeconds,
                    AttributeNames = new List<string> { "All" }
                };

                ReceiveMessageResponse? response = null;
                Exception? exception = null;

                for (int i = 0; i < _maxReceiveAttempts; i++)
                {
                    try
                    {
                        _consumerToken.Token.ThrowIfCancellationRequested();

                        response = await SQSClient.ReceiveMessageAsync(receiveMessageRequest, _consumerToken.Token)
                            .ConfigureAwait(false);

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
                    catch (OperationCanceledException) when (_consumerToken.IsCancellationRequested)
                    {
                        return;
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        exception = ex;
                    }
                }

#pragma warning disable CA1508 // false positive regarding `exception`
                if (exception is not null || response is null || response.HttpStatusCode != HttpStatusCode.OK)
#pragma warning restore CA1508
                {
                    if (connected != false)
                    {
                        string GetErrorMessage()
                        {
                            if (exception is not null)
                            {
                                return $"{exception.GetType().Name}: {exception.Message}";
                            }
                            if (response is null)
                            {
                                return "Null response returned from IAmazonSQS.ReceiveMessageAsync method.";
                            }
                            return $"Unsuccessful response returned from IAmazonSQS.ReceiveMessageAsync method: {(int)response.HttpStatusCode} {response.HttpStatusCode}";
                        }

                        connected = false;
                        OnDisconnected(GetErrorMessage());
                    }

                    OnError($"Unable to receive SQS messages from AWS. Additional Information - {GetAdditionalInformation(response, null)}", exception!);
                    continue;
                }

                await ProcessMessagesAsync(response.Messages).ConfigureAwait(false);
            }

            OnDoneReceiving();
        }

        /// <summary>
        /// Processes the group of messages received from SQS.
        /// </summary>
        /// <param name="messages">The messages to process.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected async virtual Task ProcessMessagesAsync(IEnumerable<Message> messages)
        {
            var tasks = messages.Select(HandleAsync);
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Indicates that the receiver should stop trying to receive messages.
        /// </summary>
        protected void OnStopRequested()
        {
            _stopRequested = true;
        }

        /// <summary>
        /// Indicates that the receiver has stopped trying to receive messages.
        /// </summary>
        protected virtual void OnDoneReceiving() {}

        /// <summary>
        /// Handles a single message received from SQS.
        /// </summary>
        /// <param name="message"></param>
        protected async Task HandleAsync(Message message)
        {
            ArgumentNullException.ThrowIfNull(message);

            if (_stopRequested)
            {
                return;
            }

            var receiptHandle = message.ReceiptHandle;

            Task DeleteMessageAsync(CancellationToken cancellationToken = default(CancellationToken)) =>
                DeleteAsync(receiptHandle, cancellationToken);

            Task RollbackMessageAsync(CancellationToken cancellationToken = default(CancellationToken)) =>
                RollbackAsync(receiptHandle, cancellationToken);

            using var receiverMessage = new SQSReceiverMessage(message, DeleteMessageAsync, RollbackMessageAsync, UnpackSNS);

            try
            {
                await MessageHandler!.OnMessageReceivedAsync(this, receiverMessage).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                OnError("Error in MessageHandler.OnMessageReceivedAsync.", ex);
            }
            finally
            {
                if (AutoAcknwoledge && !receiverMessage.Handled)
                {
                    try
                    {
                        await DeleteMessageAsync(_consumerToken.Token).ConfigureAwait(false);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        OnError("Error in AutoAcknowledge.", ex);
                    }
                }
            }
        }

        private Task DeleteAsync(string receiptHandle, CancellationToken cancellationToken) =>
            ExecuteWithRetry(
                async () =>
                    await SQSClient.DeleteMessageAsync(
                        new DeleteMessageRequest
                        {
                            QueueUrl = QueueUrl?.OriginalString,
                            ReceiptHandle = receiptHandle
                        },
                        cancellationToken)
                    .ConfigureAwait(false),
                _consumerToken.Token);

        private Task RollbackAsync(string receiptHandle, CancellationToken cancellationToken)
        {
            if (!TerminateMessageVisibilityTimeoutOnRollback)
            {
                return Task.CompletedTask;
            }

            return ExecuteWithRetry(
                async () =>
                    await SQSClient.ChangeMessageVisibilityAsync(
                        new ChangeMessageVisibilityRequest
                        {
                            VisibilityTimeout = 0,
                            QueueUrl = QueueUrl?.OriginalString,
                            ReceiptHandle = receiptHandle
                        },
                        cancellationToken)
                    .ConfigureAwait(false),
                _consumerToken.Token);
        }

        private static async Task ExecuteWithRetry(Func<Task<AmazonWebServiceResponse>> funcToExecute,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int i = 0;

            while (true)
            {
                try
                {
                    var response = await funcToExecute().ConfigureAwait(false);
                    if (response.HttpStatusCode == HttpStatusCode.OK)
                    {
                        return;
                    }
                }
                catch
                {
                    if (i++ >= _maxAcknowledgeAttempts)
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Signals the polling background thread to exit then waits for it to finish.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            OnStopRequested();
            _consumerToken.Cancel();

            if (_receiveMessagesTask.IsValueCreated)
            {
                if (!_receiveMessagesTask.Value.IsCompleted)
                {
                    try { _receiveMessagesTask.Value.Wait(); }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch { }
#pragma warning restore CA1031 // Do not catch general exception types
                }

                _receiveMessagesTask.Value.Dispose();
            }

            SQSClient.Dispose();
            _consumerToken.Dispose();
            base.Dispose(disposing);
        }

        private static string? GetAdditionalInformation(AmazonWebServiceResponse? response, string? receiptHandle)
        {
            if (response is null && receiptHandle is null)
            {
                return null;
            }

            if (response is null)
            {
                return $@"{{
   ""receiptHandle"": ""{receiptHandle}""
}}";
            }

            if (receiptHandle is not null)
            {
                receiptHandle = $@",
   ""receiptHandle"": ""{receiptHandle}""";
            }

            var responseMetadata = "null";

            if (response.ResponseMetadata is not null)
            {
                var metadata = "null";

                if (response.ResponseMetadata.Metadata is not null)
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

                if (response.ResponseMetadata.RequestId is not null)
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
