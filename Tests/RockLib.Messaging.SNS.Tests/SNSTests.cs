using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Moq;
using Xunit;

namespace RockLib.Messaging.SNS.Tests
{
    public static class SNSTests
    {
        [Fact]
        public static async Task SnsSenderSendsMessagesToItsIAmazonSqs()
        {
            var mockSns = new Mock<IAmazonSimpleNotificationService>();

            using (var sender = new SNSSender(mockSns.Object, "foo", "http://url.com/foo"))
                await sender.SendAsync(
                        new SenderMessage("Hello, world!") { Headers = { { "bar", "abc" } } })
                    ;

            mockSns.Verify(m => m.PublishAsync(
                It.Is<PublishRequest>(r => r.Message == "Hello, world!"
                   && r.MessageAttributes[HeaderNames.OriginatingSystem].StringValue == "SNS"
                   && r.MessageAttributes["bar"].StringValue == "abc"),
                It.IsAny<CancellationToken>()));
        }
        
        [Fact]
        public static async Task SnsSenderSendsMessageWithMessageGroupId()
        {
            var mockSns = new Mock<IAmazonSimpleNotificationService>();

            using (var sender = new SNSSender(mockSns.Object, "foo", "http://url.com/foo"))
                await sender.SendAsync(new SenderMessage("Hello, world!") { Headers = {
                    { "bar", "abc" },
                    { "messageGroupId", "this is my group id" }
                } });

            mockSns.Verify(m => m.PublishAsync(
                It.Is<PublishRequest>(r => r.Message == "Hello, world!"
                                           && r.MessageAttributes[HeaderNames.OriginatingSystem].StringValue == "SNS"
                                           && r.MessageAttributes["bar"].StringValue == "abc"
                                           && r.MessageGroupId == "this is my group id"),
                It.IsAny<CancellationToken>()));
        }
        
        [Fact]
        public static async Task SnsSenderSendsMessageWithMessageDeduplicationId()
        {
            var mockSns = new Mock<IAmazonSimpleNotificationService>();

            using (var sender = new SNSSender(mockSns.Object, "foo", "http://url.com/foo"))
                await sender.SendAsync(new SenderMessage("Hello, world!") { Headers = {
                    { "bar", "abc" },
                    { "messageDeduplicationId", "this is my deduplication id" }
                } });

            mockSns.Verify(m => m.PublishAsync(
                It.Is<PublishRequest>(r => r.Message == "Hello, world!"
                                           && r.MessageAttributes[HeaderNames.OriginatingSystem].StringValue == "SNS"
                                           && r.MessageAttributes["bar"].StringValue == "abc"
                                           && r.MessageDeduplicationId == "this is my deduplication id"),
                It.IsAny<CancellationToken>()));
        }
    }
}