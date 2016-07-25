using System.Linq;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Rock.BackgroundErrorLogging;

namespace Rock.Messaging.SQS
{
    public class SQSQueueReceiver : IReceiver
    {
        private readonly string _name;
        private readonly string _queueUrl;
        private readonly int _maxMessages;
        private readonly bool _autoAcknwoledge;
        private readonly IAmazonSQS _sqs;
        private readonly Thread _worker;

        private bool _stopped;

        public SQSQueueReceiver(ISQSConfiguration configuration, IAmazonSQS sqs)
        {
            _name = configuration.Name;
            _queueUrl = configuration.QueueUrl;
            _maxMessages = configuration.MaxMessages;
            _autoAcknwoledge = configuration.AutoAcknowledge;

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

                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        response = _sqs.ReceiveMessage(receiveMessageRequest);

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
                    BackgroundErrorLogger.Log(
                        exception,
                        "Unable to receive SQS messages from AWS.",
                        "Rock.Messaging.SQS",
                        GetAdditionalInformation(response, null));

                    continue;
                }

                foreach (var message in response.Messages)
                {
                    if (_stopped)
                    {
                        break;
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

                                        deleteResponse = _sqs.DeleteMessage(new DeleteMessageRequest
                                        {
                                            QueueUrl = _queueUrl,
                                            ReceiptHandle = receiptHandle
                                        });

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

                                BackgroundErrorLogger.Log(
                                    deleteException,
                                    "Unable to delete SQS message.",
                                    "Rock.Messaging.SQS",
                                    GetAdditionalInformation(deleteResponse, receiptHandle));
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
