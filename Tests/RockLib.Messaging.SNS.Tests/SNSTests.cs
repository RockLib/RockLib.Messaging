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
        public static async Task SNSSenderSendsMessagesToItsIAmazonSQS()
        {
            var mockSns = new Mock<IAmazonSimpleNotificationService>();

            using (var sender = new SNSSender(mockSns.Object, "foo", "http://url.com/foo"))
            {
                await sender.SendAsync(
                    new SenderMessage("Hello, world!") { Headers = { { "bar", "abc" } } })
                    ;
            }

            mockSns.Verify(m => m.PublishAsync(
                It.Is<PublishRequest>(r => r.Message == "Hello, world!"
                   && r.MessageAttributes[HeaderNames.OriginatingSystem].StringValue == "SNS"
                   && r.MessageAttributes["bar"].StringValue == "abc"),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public static async Task SNSSenderSetsMessageGroupIdWhenSpecifiedInHeader()
        {
            var mockSns = new Mock<IAmazonSimpleNotificationService>();

            using (var sender = new SNSSender(mockSns.Object, "foo", "http://url.com/foo"))
            {
                await sender.SendAsync(
                    new SenderMessage("Hello, world!") { Headers = { { "bar", "abc" }, { "SNS.MessageGroupId", "123" } } })
                    .ConfigureAwait(false);
            }

            mockSns.Verify(m => m.PublishAsync(
                It.Is<PublishRequest>(r => r.Message == "Hello, world!"
                   && r.MessageAttributes[HeaderNames.OriginatingSystem].StringValue == "SNS"
                   && r.MessageAttributes["bar"].StringValue == "abc"
                   && r.MessageGroupId == "123"),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public static async Task SQSSenderSetsMessageGroupIdWhenSpecifiedInConstructor()
        {
            var mockSns = new Mock<IAmazonSimpleNotificationService>();

            using (var sender = new SNSSender(mockSns.Object, "foo", "http://url.com/foo", "abc"))
            {
                await sender.SendAsync(
                        new SenderMessage(""))
                    .ConfigureAwait(false);
            }

            mockSns.Verify(m => m.PublishAsync(
                It.Is<PublishRequest>(r => r.MessageGroupId == "abc"),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public static async Task SQSSenderSetsMessageGroupIdToHeaderValueWhenSpecifiedInBothHeaderAndConstructor()
        {
            var mockSns = new Mock<IAmazonSimpleNotificationService>();

            using (var sender = new SNSSender(mockSns.Object, "foo", "http://url.com/foo"))
            {
                await sender.SendAsync(
                        new SenderMessage("") { Headers = { { "SNS.MessageGroupId", "xyz" } } })
                    .ConfigureAwait(false);
            }

            mockSns.Verify(m => m.PublishAsync(
                It.Is<PublishRequest>(r =>
                    !r.MessageAttributes.ContainsKey("SNS.MessageGroupId")
                    && r.MessageGroupId == "xyz"),
                It.IsAny<CancellationToken>()));
        }
    }
}
