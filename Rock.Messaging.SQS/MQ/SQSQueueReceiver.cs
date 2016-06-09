using Amazon.SQS;
using Amazon.SQS.Model;
using Rock.Logging;
using Rock.Logging.Defaults;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using ILogger = Rock.Logging.ILogger;
using LoggerFactory = Rock.Logging.LoggerFactory;

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
        private readonly Lazy<ILogger> _logger;

        private bool _stopped;

        public SQSQueueReceiver(ISQSConfiguration configuration, IAmazonSQS sqs)
        {
            _name = configuration.Name;
            _queueUrl = configuration.QueueUrl;
            _maxMessages = configuration.MaxMessages;
            _autoAcknwoledge = configuration.AutoAcknowledge;

            _sqs = sqs;

            _worker = new Thread(DoStuff);

            _logger = new Lazy<ILogger>(() =>
            {
                try
                {
                    return LoggerFactory.GetInstance();
                }
                catch
                {
                    return null;
                }
            });
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
                    if (_logger.Value != null)
                    {
                        _logger.Value.Error(new LogEntry("Rock.Messaging.SQS.SQSQueueReceiver: Unable to receive messages.", exception));
                    }

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

                                for (int i = 0; i < 3; i++)
                                {
                                    try
                                    {
                                        var deleteResponse = _sqs.DeleteMessage(new DeleteMessageRequest
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

                                if (_logger.Value != null)
                                {
                                    _logger.Value.Error(new LogEntry("Rock.Messaging.SQS.SQSQueueReceiver: Unable to delete message.", new { receiptHandle }, deleteException));
                                }
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
    }
}
