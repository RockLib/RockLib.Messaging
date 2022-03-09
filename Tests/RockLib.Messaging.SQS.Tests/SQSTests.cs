using Amazon.SQS;
using Amazon.SQS.Model;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace RockLib.Messaging.SQS.Tests
{
    public static class SQSTests
    {
        [Fact]
        public static async Task SQSSenderSendsMessagesToItsIAmazonSQS()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            using (var sender = new SQSSender(mockSqs.Object, "foo", new Uri("http://url.com/foo")))
            {
                await sender.SendAsync(
                    new SenderMessage("Hello, world!") { Headers = { { "bar", "abc" } } })
                    .ConfigureAwait(false);
            }

            mockSqs.Verify(m => m.SendMessageAsync(
                It.Is<SendMessageRequest>(r => r.MessageBody == "Hello, world!"
                    && r.MessageAttributes[HeaderNames.OriginatingSystem].StringValue == "SQS"
                    && r.MessageAttributes["bar"].StringValue == "abc"),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public static async Task SQSSenderSetsMessageGroupIdWhenSpecifiedInHeader()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            using (var sender = new SQSSender(mockSqs.Object, "foo", new Uri("http://url.com/foo")))
            {
                await sender.SendAsync(
                    new SenderMessage("") { Headers = { { "SQS.MessageGroupId", "abc" } } })
                    .ConfigureAwait(false);
            }

            mockSqs.Verify(m => m.SendMessageAsync(
                It.Is<SendMessageRequest>(r =>
                    !r.MessageAttributes.ContainsKey("SQS.MessageGroupId")
                    && r.MessageGroupId == "abc"),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public static async Task SQSSenderSetsMessageGroupIdWhenSpecifiedInConstructor()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            using (var sender = new SQSSender(mockSqs.Object, "foo", new Uri("http://url.com/foo"), "abc"))
            {
                await sender.SendAsync(
                    new SenderMessage(""))
                    .ConfigureAwait(false);
            }

            mockSqs.Verify(m => m.SendMessageAsync(
                It.Is<SendMessageRequest>(r => r.MessageGroupId == "abc"),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public static async Task SQSSenderSetsMessageGroupIdToHeaderValueWhenSpecifiedInBothHeaderAndConstructor()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            using (var sender = new SQSSender(mockSqs.Object, "foo", new Uri("http://url.com/foo"), "abc"))
            {
                await sender.SendAsync(
                    new SenderMessage("") { Headers = { { "SQS.MessageGroupId", "xyz" } } })
                    .ConfigureAwait(false);
            }

                mockSqs.Verify(m => m.SendMessageAsync(
                It.Is<SendMessageRequest>(r =>
                    !r.MessageAttributes.ContainsKey("SQS.MessageGroupId")
                    && r.MessageGroupId == "xyz"),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public static async Task SQSSenderSetsMessageDeduplicationIdWhenSpecifiedInHeader()
        {
            var mockSqs = new Mock<IAmazonSQS>();

                using (var sender = new SQSSender(mockSqs.Object, "foo", new Uri("http://url.com/foo")))
                {
                    await sender.SendAsync(
                        new SenderMessage("") { Headers = { { "SQS.MessageDeduplicationId", "abc" } } })
                        .ConfigureAwait(false);
                }

            mockSqs.Verify(m => m.SendMessageAsync(
                It.Is<SendMessageRequest>(r =>
                    !r.MessageAttributes.ContainsKey("SQS.MessageDeduplicationId")
                    && r.MessageDeduplicationId == "abc"),
                It.IsAny<CancellationToken>()));
        }


        [Fact]
        public static async Task SQSSenderSetsMessageDelayWhenSpecifiedInHeader()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            using (var sender = new SQSSender(mockSqs.Object, "foo", new Uri("http://url.com/foo")))
            {
                await sender.SendAsync(
                    new SenderMessage("") { Headers = { { "SQS.DelaySeconds", 123 } } })
                    .ConfigureAwait(false);
            }

            mockSqs.Verify(m => m.SendMessageAsync(
                It.Is<SendMessageRequest>(r =>
                    !r.MessageAttributes.ContainsKey("SQS.DelaySeconds")
                    && r.DelaySeconds == 123),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public static void SQSReceiverReceivesMessagesFromItsIAmazonSQS()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            SetupReceiveMessageAsync(mockSqs);

            using var waitHandle = new AutoResetEvent(false);

            string? receivedMessage = null;
            string? quxHeader = null;

            using (var receiver = new SQSReceiver(mockSqs.Object, "foo", new Uri("http://url.com/foo"), autoAcknowledge: false))
            {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
                receiver.Start(async m =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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
        public static void AcknowledgeDeletesTheMessageByReceiptHandleWithItsIAmazonSQS()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            SetupReceiveMessageAsync(mockSqs);
            SetupDeleteMessageAsync(mockSqs);

            using var waitHandle = new AutoResetEvent(false);

            using (var receiver = new SQSReceiver(mockSqs.Object, "foo", new Uri("http://url.com/foo"), autoAcknowledge: false))
            {
                receiver.Start(async m =>
                {
                    await m.AcknowledgeAsync().ConfigureAwait(false);
                    waitHandle.Set();
                });

                waitHandle.WaitOne();
            }

            mockSqs.Verify(m => m.DeleteMessageAsync(
                It.Is<DeleteMessageRequest>(r => r.QueueUrl == "http://url.com/foo"
                    && r.ReceiptHandle == "bar"),
                It.IsAny<CancellationToken>()));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public static void RollbackDoesNotDeleteTheMessageByReceiptHandleWithItsIAmazonSQS(bool terminateMessageVisibilityTimeoutOnRollback)
        {
            var mockSqs = new Mock<IAmazonSQS>();

            SetupReceiveMessageAsync(mockSqs);
            SetupChangeMessageVisibilityAsync(mockSqs);

            using var waitHandle = new AutoResetEvent(false);

            using (var receiver = new SQSReceiver(mockSqs.Object, "foo", new Uri("http://url.com/foo"), autoAcknowledge: false, terminateMessageVisibilityTimeoutOnRollback: terminateMessageVisibilityTimeoutOnRollback))
            {
                receiver.Start(async m =>
                {
                    await m.RollbackAsync().ConfigureAwait(false);
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
        public static void WhenTerminateMessageVisibilityTimeoutOnRollbackIsFalseRollbackDoesNotChangeMessageVisibilityByReceiptHandleWithItsIAmazonSQS()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            SetupReceiveMessageAsync(mockSqs);
            SetupChangeMessageVisibilityAsync(mockSqs);

            using var waitHandle = new AutoResetEvent(false);

            using (var receiver = new SQSReceiver(mockSqs.Object, "foo", new Uri("http://url.com/foo"), autoAcknowledge: false, terminateMessageVisibilityTimeoutOnRollback: false))
            {
                receiver.Start(async m =>
                {
                    await m.RollbackAsync().ConfigureAwait(false);
                    waitHandle.Set();
                });

                waitHandle.WaitOne();
            }

            mockSqs.Verify(m => m.ChangeMessageVisibilityAsync(
                It.IsAny<ChangeMessageVisibilityRequest>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public static void WhenTerminateMessageVisibilityTimeoutOnRollbackIsTrueRollbackChangesMessageVisibilityToZeroByReceiptHandleWithItsIAmazonSQS()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            SetupReceiveMessageAsync(mockSqs);
            SetupChangeMessageVisibilityAsync(mockSqs);

            using var waitHandle = new AutoResetEvent(false);

            using (var receiver = new SQSReceiver(mockSqs.Object, "foo", new Uri("http://url.com/foo"), autoAcknowledge: false, terminateMessageVisibilityTimeoutOnRollback: true))
            {
                receiver.Start(async m =>
                {
                    await m.RollbackAsync().ConfigureAwait(false);
                    waitHandle.Set();
                });

                waitHandle.WaitOne();
            }

            mockSqs.Verify(m => m.ChangeMessageVisibilityAsync(
                It.Is<ChangeMessageVisibilityRequest>(r => r.VisibilityTimeout == 0),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public static void RejectDeletesTheMessageByReceiptHandleWithItsIAmazonSQS()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            SetupReceiveMessageAsync(mockSqs);
            SetupDeleteMessageAsync(mockSqs);

            using var waitHandle = new AutoResetEvent(false);

            using (var receiver = new SQSReceiver(mockSqs.Object, "foo", new Uri("http://url.com/foo"), autoAcknowledge: false))
            {
                receiver.Start(async m =>
                {
                    await m.RejectAsync().ConfigureAwait(false);
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
        public static void WhenAutoAcknowledgeIsTrueMessagesAreAutomaticallyDeleted()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            SetupReceiveMessageAsync(mockSqs);
            SetupDeleteMessageAsync(mockSqs);

            using var waitHandle = new AutoResetEvent(false);

            using (var receiver = new SQSReceiver(mockSqs.Object, "foo", new Uri("http://url.com/foo"), autoAcknowledge: false))
            {
                receiver.Start(async m =>
                {
                    await m.AcknowledgeAsync().ConfigureAwait(false);
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
        public static void WhenAutoAcknowledgeIsTrueMessagesAreNotDeletedIfExplicitlyHandled()
        {
            var mockSqs = new Mock<IAmazonSQS>();

            SetupReceiveMessageAsync(mockSqs);
            SetupDeleteMessageAsync(mockSqs);

            using var waitHandle = new AutoResetEvent(false);

            using (var receiver = new SQSReceiver(mockSqs.Object, "foo", new Uri("http://url.com/foo"), autoAcknowledge: false))
            {
                receiver.Start(async m =>
                {
                    await m.RollbackAsync().ConfigureAwait(false);
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
        public static void SQSReceiverMessageWithUnpackSNSTrueCorrectlyUnpacksSNSMessage()
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

            using var sqsReceiverMessage = new SQSReceiverMessage(message, c => Task.FromResult(0), c => Task.FromResult(0), unpackSNS: true);

            sqsReceiverMessage.StringPayload.Should().Be("This is a better test message");
            sqsReceiverMessage.Headers["TopicARN"].Should().Be("arn:PutARealARNHere");
            sqsReceiverMessage.Headers["core_internal_id"].Should().Be("18c4ef1f-62d1-4bd9-aba0-f008a0ac481d");
            sqsReceiverMessage.Headers["core_originating_system"].Should().Be("SNS");
        }

        [Fact]
        public static void SQSReceiverMessageWithUnpackSNSTrueCorrectlyUnpacksSNSMessageWithNoMessageAttributes()
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
  ""UnsubscribeURL"" : ""SomeOtherUrl""
}";

            var message = new Message
            {
                Body = snsMessage
            };

            using var sqsReceiverMessage = new SQSReceiverMessage(message, c => Task.FromResult(0), c => Task.FromResult(0), unpackSNS: true);

            sqsReceiverMessage.StringPayload.Should().Be("This is a better test message");
            sqsReceiverMessage.Headers["TopicARN"].Should().Be("arn:PutARealARNHere");
        }

        [Fact]
        public static void SQSReceiverMessageHasSQSMessageIDSetCorrectly()
        {
            var message = new Message
            {
                MessageId = (new Guid()).ToString(),
                Body = @"{""TestMessageItem"":""ThisIsAFakeItem""}"
            };

            using (var sqsReceiverMessage = new SQSReceiverMessage(message, c => Task.FromResult(0), c => Task.FromResult(0), unpackSNS: false))
            {
                var messageId = sqsReceiverMessage.Headers.GetValue<string>("SQS.MessageID");

                messageId.Should().NotBeNull();
            }
        }

        [Fact]
        public static void SQSReceiverMessageDoesNotHaveSQSMessageIDHeaderWhenSNS()
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
  ""UnsubscribeURL"" : ""SomeOtherUrl""
}";

            var message = new Message
            {
                MessageId = (new Guid()).ToString(),
                Body = snsMessage
            };

            using (var sqsReceiverMessage = new SQSReceiverMessage(message, c => Task.FromResult(0), c => Task.FromResult(0), unpackSNS: true))
            {
                Assert.Throws<KeyNotFoundException>(() => sqsReceiverMessage.Headers.GetValue<string>("SQS.MessageID"));
            }
 
        }

        private static void SetupDeleteMessageAsync(Mock<IAmazonSQS> mockSqs, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            mockSqs.Setup(m => m.DeleteMessageAsync(It.IsAny<DeleteMessageRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new DeleteMessageResponse
                {
                    HttpStatusCode = httpStatusCode
                }));
        }

        private static void SetupChangeMessageVisibilityAsync(Mock<IAmazonSQS> mockSqs, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            mockSqs.Setup(m => m.ChangeMessageVisibilityAsync(It.IsAny<ChangeMessageVisibilityRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ChangeMessageVisibilityResponse
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
