using Amazon.SQS;
using Amazon.SQS.Model;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Messaging.SQS.Tests
{
    public class SQSTests
    {
        [Fact]
        public void SQSQueueSenderSendsMessagesToItsIAmazonSQS()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            using (var sender = new SQSQueueSender(mockSqs.Object, "foo", "http://url.com/foo"))
                sender.Send(new SenderMessage("Hello, world!") { Headers = { { "bar", "abc" } } });

            mockSqs.Verify(m => m.SendMessageAsync(
                It.Is<SendMessageRequest>(r => r.MessageBody == "Hello, world!"
                    && r.MessageAttributes[HeaderNames.OriginatingSystem].StringValue == "SQS"
                    && r.MessageAttributes["bar"].StringValue == "abc"),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public void SQSQueueReceiverReceivesMessagesFromItsIAmazonSQS()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            SetupReceiveMessageAsync(mockSqs);

            var waitHandle = new AutoResetEvent(false);

            string receivedMessage = null;
            string quxHeader = null;

            using (var receiver = new SQSQueueReceiver(mockSqs.Object, "foo", "http://url.com/foo", autoAcknowledge: false))
            {
                receiver.Start(m =>
                {
                    receivedMessage = m.StringPayload;
                    quxHeader = m.Headers.GetValue<string>("qux");
                    waitHandle.Set();
                });

                waitHandle.WaitOne();
            }

            mockSqs.Verify(m => m.ReceiveMessageAsync(
                It.Is<ReceiveMessageRequest>(r => r.MaxNumberOfMessages == 3
                    && r.QueueUrl == "http://url.com/foo"
                    && r.MessageAttributeNames.Count == 1
                    && r.MessageAttributeNames[0] == "*"),
                It.IsAny<CancellationToken>()));

            Assert.Equal("Hello, world!", receivedMessage);
            Assert.Equal("xyz", quxHeader);
        }

        [Fact]
        public void AcknowledgeDeletesTheMessageByReceiptHandleWithItsIAmazonSQS()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            SetupReceiveMessageAsync(mockSqs);
            SetupDeleteMessageAsync(mockSqs);

            var waitHandle = new AutoResetEvent(false);

            using (var receiver = new SQSQueueReceiver(mockSqs.Object, "foo", "http://url.com/foo", autoAcknowledge: false))
            {
                receiver.Start(m =>
                {
                    m.Acknowledge();
                    waitHandle.Set();
                });

                waitHandle.WaitOne();
            }

            mockSqs.Verify(m => m.DeleteMessageAsync(
                It.Is<DeleteMessageRequest>(r => r.QueueUrl == "http://url.com/foo"
                    && r.ReceiptHandle == "bar"),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public void RollbackDoesNotDeleteTheMessageByReceiptHandleWithItsIAmazonSQS()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            SetupReceiveMessageAsync(mockSqs);
            SetupDeleteMessageAsync(mockSqs);

            var waitHandle = new AutoResetEvent(false);

            using (var receiver = new SQSQueueReceiver(mockSqs.Object, "foo", "http://url.com/foo", autoAcknowledge: false))
            {
                receiver.Start(m =>
                {
                    m.Rollback();
                    waitHandle.Set();
                });

                waitHandle.WaitOne();
            }

            mockSqs.Verify(m => m.DeleteMessageAsync(
                It.IsAny<DeleteMessageRequest>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public void RejectDeletesTheMessageByReceiptHandleWithItsIAmazonSQS()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            SetupReceiveMessageAsync(mockSqs);
            SetupDeleteMessageAsync(mockSqs);

            var waitHandle = new AutoResetEvent(false);

            using (var receiver = new SQSQueueReceiver(mockSqs.Object, "foo", "http://url.com/foo", autoAcknowledge: false))
            {
                receiver.Start(m =>
                {
                    m.Reject();
                    waitHandle.Set();
                });

                waitHandle.WaitOne();
            }

            mockSqs.Verify(m => m.DeleteMessageAsync(
                It.Is<DeleteMessageRequest>(r => r.QueueUrl == "http://url.com/foo"
                    && r.ReceiptHandle == "bar"),
                It.IsAny<CancellationToken>()));
        }

        private static void SetupDeleteMessageAsync(Mock<IAmazonSQS> mockSqs, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            mockSqs.Setup(m => m.DeleteMessageAsync(It.IsAny<DeleteMessageRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new DeleteMessageResponse
                {
                    HttpStatusCode = httpStatusCode
                }));
        }

        private static void SetupReceiveMessageAsync(Mock<IAmazonSQS> mockSqs,
            string receiptHandle = "bar", string body = "Hello, world!", string headerName = "qux", string headerValue = "xyz",
            HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            mockSqs.Setup(m => m.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ReceiveMessageResponse
                {
                    Messages = new List<Message> {
                        new Message
                        {
                            ReceiptHandle = receiptHandle,
                            Body = body,
                            MessageAttributes = new Dictionary<string, MessageAttributeValue>
                            {
                                { headerName, new MessageAttributeValue { StringValue = headerValue, DataType = "String" } }
                            }
                        } },
                    HttpStatusCode = httpStatusCode
                }));
        }
    }
}
