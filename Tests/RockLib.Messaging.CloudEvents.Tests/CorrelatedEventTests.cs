using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace RockLib.Messaging.CloudEvents.Tests
{
    public class CorrelatedEventTests
    {
        [Fact(DisplayName = "CorrelationId property setter and getter work as expected")]
        public void CorrelationIdPropertyHappyPath1()
        {
            var cloudEvent = new CorrelatedEvent();

            cloudEvent.CorrelationId = "123";

            cloudEvent.CorrelationId.Should().Be("123");
        }

        [Fact(DisplayName = "CorrelationId property getter returns new GUID if setter has not been called")]
        public void CorrelationIdPropertyHappyPath2()
        {
            var cloudEvent = new CorrelatedEvent();

            cloudEvent.CorrelationId.Should().NotBeNullOrEmpty();
            Guid.TryParse(cloudEvent.CorrelationId, out _).Should().BeTrue();
        }

        [Fact(DisplayName = "CorrelationId property setter throws if value is null")]
        public void CorrelationIdPropertySadPath()
        {
            var cloudEvent = new CorrelatedEvent();

            cloudEvent.Invoking(evt => evt.CorrelationId = null!).Should()
                .ThrowExactly<ArgumentNullException>()
                .WithMessage("*value*");
        }

        [Fact(DisplayName = "Validate method adds correlationid attribute if missing")]
        public void ValidateMethodHappyPath()
        {
            var cloudEvent = new CorrelatedEvent
            {
                Id = "MyId",
                Type = "MyType",
                Source = "/MySource",
                Time = DateTime.Now
            };

            cloudEvent.Attributes.Should().HaveCount(4);

            cloudEvent.Validate();

            cloudEvent.Attributes.Should().HaveCount(5);
            cloudEvent.Attributes.Should().ContainKey(CorrelatedEvent.CorrelationIdAttribute)
                .WhichValue.Should().NotBeNull();
        }

        [Fact(DisplayName = "Validate static method does not throw when given valid sender message")]
        public void ValidateStaticMethodHappyPath1()
        {
            var senderMessage = new SenderMessage("Hello, world!");
            
            senderMessage.Headers.Add(CorrelatedEvent.CorrelationIdAttribute, "MyCorrelationId");

            senderMessage.Headers.Add(CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => CorrelatedEvent.Validate(senderMessage);

            act.Should().NotThrow();
        }

        [Fact(DisplayName = "Validate static method does not throw when given valid sender message for specified protocol binding")]
        public void ValidateStaticMethodHappyPath2()
        {
            // Non-default protocol binding

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add("test-" + CorrelatedEvent.CorrelationIdAttribute, "MyCorrelationId");

            senderMessage.Headers.Add("test-" + CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add("test-" + CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add("test-" + CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add("test-" + CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add("test-" + CloudEvent.TimeAttribute, DateTime.UtcNow);

            var mockProtocolBinding = new Mock<IProtocolBinding>();
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>())).Returns<string>(header => "test-" + header);

            Action act = () => CorrelatedEvent.Validate(senderMessage, mockProtocolBinding.Object);

            act.Should().NotThrow();
        }

        [Fact(DisplayName = "Validate static method adds missing CorrelationId header")]
        public void ValidateStaticMethodSadPath()
        {
            // Missing CorrelationId

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            CorrelatedEvent.Validate(senderMessage);

            senderMessage.Headers.Should().ContainKey(CorrelatedEvent.CorrelationIdAttribute)
                .WhichValue.Should().NotBeNull();
        }
    }
}
