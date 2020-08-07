using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace RockLib.Messaging.CloudEvents.Tests
{
    public class PartitionedEventTests
    {
        [Fact(DisplayName = "PartitionKey property setter and getter work as expected")]
        public void CorrelationIdPropertyHappyPath1()
        {
            var cloudEvent = new PartitionedEvent();

            cloudEvent.PartitionKey = "123";

            cloudEvent.PartitionKey.Should().Be("123");
        }

        [Fact(DisplayName = "Validate method does not throw when valid")]
        public void ValidateMethodHappyPath()
        {
            var cloudEvent = new PartitionedEvent
            {
                Type = "MyType",
                Source = "/MySource",
                PartitionKey = "123"
            };

            cloudEvent.Invoking(ce => ce.Validate())
                .Should().NotThrow();
        }

        [Theory(DisplayName = "Validate method throws when PartitionKey is missing")]
        [InlineData(null)]
        [InlineData("")]
        public void ValidateMethodSadPath1(string partitionKey)
        {
            var cloudEvent = new PartitionedEvent
            {
                Type = "MyType",
                Source = "/MySource",
                PartitionKey = partitionKey
            };

            cloudEvent.Invoking(ce => ce.Validate())
                .Should().ThrowExactly<CloudEventValidationException>()
                .WithMessage("PartitionKey cannot be null or empty.");
        }

        [Fact(DisplayName = "Validate static method does not throw when given valid sender message")]
        public void ValidateStaticMethodHappyPath1()
        {
            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(PartitionedEvent.PartitionKeyAttribute, "123");

            senderMessage.Headers.Add(CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => PartitionedEvent.Validate(senderMessage);

            act.Should().NotThrow();
        }

        [Fact(DisplayName = "Validate static method does not throw when given valid sender message for specified protocol binding")]
        public void ValidateStaticMethodHappyPath2()
        {
            // Non-default protocol binding

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add("test-" + PartitionedEvent.PartitionKeyAttribute, "123");

            senderMessage.Headers.Add("test-" + CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add("test-" + CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add("test-" + CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add("test-" + CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add("test-" + CloudEvent.TimeAttribute, DateTime.UtcNow);

            var mockProtocolBinding = new Mock<IProtocolBinding>();
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>())).Returns<string>(header => "test-" + header);

            Action act = () => PartitionedEvent.Validate(senderMessage, mockProtocolBinding.Object);

            act.Should().NotThrow();
        }

        [Fact(DisplayName = "Validate static method throws given missing PartitionKey header")]
        public void ValidateStaticMethodSadPath()
        {
            // Missing PartitionKey

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => PartitionedEvent.Validate(senderMessage);

            act.Should().ThrowExactly<CloudEventValidationException>();
        }
    }
}
