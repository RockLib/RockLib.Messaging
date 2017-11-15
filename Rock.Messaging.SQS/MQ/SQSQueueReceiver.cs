using System.Linq;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

#if ROCKLIB
using System.Diagnostics;
#else
using Rock.BackgroundErrorLogging;
#endif

#if ROCKLIB
namespace RockLib.Messaging.SQS
#else
namespace Rock.Messaging.SQS
#endif
{
    public class SQSQueueReceiver : IReceiver
    {
        private readonly string _name;
        private readonly string _queueUrl;
        private readonly int _maxMessages;
        private readonly bool _autoAcknwoledge;
        private readonly bool _parallelHandling;
        private readonly IAmazonSQS _sqs;
        private readonly Thread _worker;

        private bool _stopped;

        private const int _maxAcknowledgeAttempts = 3;
        private const int _maxReceiveAttempts = 3;

        public SQSQueueReceiver(ISQSConfiguration configuration, IAmazonSQS sqs)
        {
            _name = configuration.Name;
            _queueUrl = configuration.QueueUrl;
            _maxMessages = configuration.MaxMessages;
            _autoAcknwoledge = configuration.AutoAcknowledge;
            _parallelHandling = configuration.ParallelHandling;

            _sqs = sqs;

            _worker = new Thread(DoStuff);
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public string Name
        {
            get { return _name; }
        }

        public void Start(string selector)
        {
            if (!_worker.IsAlive && !_stopped)
            {
                _worker.Start();
            }
        }

        private void DoStuff()
        {
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

                for (int i = 0; i < _maxAcknowledgeAttempts; i++)
                {
                    try
                    {
                        response = Sync.OverAsync(() => _sqs.ReceiveMessageAsync(receiveMessageRequest));

                        if (response.HttpStatusCode == HttpStatusCode.OK)
                        {
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
#if ROCKLIB
                    Trace.TraceError($"Unable to receive SQS messages from AWS. Additional Information - {GetAdditionalInformation(response, null)}");
#else
                    BackgroundErrorLogger.Log(
                        exception,
                        "Unable to receive SQS messages from AWS.",
                        "Rock.Messaging.SQS",
                        GetAdditionalInformation(response, null));
#endif
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

            var handler = MessageReceived;

            if (handler != null)
            {
                var receiptHandle = message.ReceiptHandle;

                Action acknowledge =
                    () =>
                    {
                        Exception deleteException = null;
                        DeleteMessageResponse deleteResponse = null;

                        for (int i = 0; i < 3; i++)
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

#if ROCKLIB
                        Trace.TraceError($"Unable to delete SQS message.. Additional Information - {GetAdditionalInformation(deleteResponse, receiptHandle)}");
#else
                                BackgroundErrorLogger.Log(
                                    deleteException,
                                    "Unable to delete SQS message.",
                                    "Rock.Messaging.SQS",
                                    GetAdditionalInformation(deleteResponse, receiptHandle));
#endif
                    };

                try
                {
                    handler(this, new MessageReceivedEventArgs(new SQSMessage(message, acknowledge)));
                }
                finally
                {
                    if (_autoAcknwoledge)
                    {
                        acknowledge();
                    }
                }
            }
        }

        public void Dispose()
        {
            _stopped = true;
            _worker.Join();
            _sqs.Dispose();
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
