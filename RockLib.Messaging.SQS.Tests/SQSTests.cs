using Amazon.SQS;
using Amazon.SQS.Model;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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

        [Fact]
        public void WhenAutoAcknowledgeIsTrueMessagesAreAutomaticallyDeleted()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            SetupReceiveMessageAsync(mockSqs);
            SetupDeleteMessageAsync(mockSqs);

            var waitHandle = new AutoResetEvent(false);

            using (var receiver = new SQSQueueReceiver(mockSqs.Object, "foo", "http://url.com/foo", autoAcknowledge: true))
            {
                receiver.Start(m =>
                {
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
        public void WhenAutoAcknowledgeIsTrueMessagesAreNotDeletedIfExplicitlyHandled()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            SetupReceiveMessageAsync(mockSqs);
            SetupDeleteMessageAsync(mockSqs);

            var waitHandle = new AutoResetEvent(false);

            using (var receiver = new SQSQueueReceiver(mockSqs.Object, "foo", "http://url.com/foo", autoAcknowledge: true))
            {
                receiver.Start(m =>
                {
                    m.Rollback();
                    waitHandle.Set();
                });

                waitHandle.WaitOne();
            }

            mockSqs.Verify(m => m.DeleteMessageAsync(
                It.Is<DeleteMessageRequest>(r => r.QueueUrl == "http://url.com/foo"
                    && r.ReceiptHandle == "bar"),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public void SNSToSQSReceiverMessageCorrectlyUnpacksSNSMessage()
        {
            var snsMessage = @"{
  ""Type"" : ""Notification"",
  ""MessageId"" : ""f5129dcc-0bb0-5e18-9431-c95b6a9b32d9"",
  ""TopicArn"" : ""arn:PutARealARNHere"",
  ""Message"" : ""This is a better test message"",
  ""Timestamp"" : ""2018-12-21T21:45:15.114Z"",
  ""SignatureVersion"" : ""1"",
  ""Signature"" : ""SomeSignatureValue"",
  ""SigningCertURL"" : ""SomeUrl"",
  ""UnsubscribeURL"" : ""SomeOtherUrl"",
  ""MessageAttributes"" : {
    ""core_internal_id"" : {""Type"":""String"",""Value"":""18c4ef1f-62d1-4bd9-aba0-f008a0ac481d""},
    ""core_originating_system"" : {""Type"":""String"",""Value"":""SNS""}
  }
}";

            var message = new Message
            {
                Body = snsMessage
            };

            var sqsReceiverMessage = new SQSReceiverMessage(message, c => Task.FromResult(0), true);

            sqsReceiverMessage.StringPayload.Should().Be("This is a better test message");
            sqsReceiverMessage.Headers["TopicARN"].Should().Be("arn:PutARealARNHere");
            sqsReceiverMessage.Headers["core_internal_id"].Should().Be("18c4ef1f-62d1-4bd9-aba0-f008a0ac481d");
            sqsReceiverMessage.Headers["core_originating_system"].Should().Be("SNS");
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
