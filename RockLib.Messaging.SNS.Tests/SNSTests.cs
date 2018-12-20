using System.Threading;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Moq;
using Xunit;

namespace RockLib.Messaging.SNS.Tests
{
    public class SNSTests
    {
        [Fact]
        public void SQSQueueSenderSendsMessagesToItsIAmazonSQS()
        {
            var mockSns = new Mock<IAmazonSimpleNotificationService>();

            using (var sender = new SNSTopicSender(mockSns.Object, "foo", "http://url.com/foo"))
                sender.Send(new SenderMessage("Hello, world!") { Headers = { { "bar", "abc" } } });

            mockSns.Verify(m => m.PublishAsync(
                It.Is<PublishRequest>(r => r.Message == "Hello, world!"
                   && r.MessageAttributes[HeaderNames.OriginatingSystem].StringValue == "SNS"
                   && r.MessageAttributes["bar"].StringValue == "abc"),
                It.IsAny<CancellationToken>()));
        }
    }
}
