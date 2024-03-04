using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace RockLib.Messaging.CloudEvents.Tests
{
    public class SequentialEventTests
    {
        [Fact(DisplayName = "Constructor 2 sets expected properties")]
        public void Constructor2HappyPath()
        {
            var source = new SequentialEvent
            {
                Sequence = "1",
                SequenceType = SequenceTypes.Integer,
                Id = "MyId",
                Time = new DateTime(2020, 7, 9, 22, 21, 37, DateTimeKind.Local),
                Source = "http://mysource/",
                Type = "MyType",
                DataContentType = "application/json; charset=utf-8",
                DataSchema = "http://mydataschema/",
                Subject = "MySubject"
            };

            var cloudEvent = new SequentialEvent(source);

            cloudEvent.Sequence.Should().Be("2");
            cloudEvent.SequenceType.Should().Be(SequenceTypes.Integer);

            cloudEvent.Source.Should().BeSameAs(source.Source);
            cloudEvent.Type.Should().BeSameAs(source.Type);
            cloudEvent.DataContentType.Should().BeSameAs(source.DataContentType);
            cloudEvent.DataSchema.Should().BeSameAs(source.DataSchema);
            cloudEvent.Subject.Should().BeSameAs(source.Subject);

            cloudEvent.Id.Should().NotBe(source.Id);
            cloudEvent.Time.Should().NotBe(source.Time);
        }

        [Fact(DisplayName = "Sequence property setter and getter work as expected")]
        public void SequencePropertyHappyPath()
        {
            var cloudEvent = new SequentialEvent();

            cloudEvent.Sequence = "123";

            cloudEvent.Sequence.Should().Be("123");
        }

        [Fact(DisplayName = "SequenceType property setter and getter work as expected")]
        public void SequenceTypePropertyHappyPath()
        {
            var cloudEvent = new SequentialEvent();

            cloudEvent.SequenceType = "abc";

            cloudEvent.SequenceType.Should().Be("abc");
        }

        [Fact(DisplayName = "Validate method does not throw when valid")]
        public void ValidateMethodHappyPath()
        {
            var cloudEvent = new SequentialEvent
            {
                Type = "MyType",
                Source = "/MySource",
                Sequence = "1",
                SequenceType = SequenceTypes.Integer
            };

            cloudEvent.Invoking(ce => ce.Validate())
                .Should().NotThrow();
        }

        [Theory(DisplayName = "Validate method throws when Sequence is missing")]
        [InlineData(null)]
        [InlineData("")]
        public void ValidateMethodSadPath1(string? sequence)
        {
            var cloudEvent = new SequentialEvent
            {
                Type = "MyType",
                Source = "/MySource",
                Sequence = sequence
            };

            cloudEvent.Invoking(ce => ce.Validate())
                .Should().ThrowExactly<CloudEventValidationException>()
                .WithMessage("Sequence cannot be null or empty.");
        }

        [Fact(DisplayName = "Validate method throws when SequenceType is 'Integer' and Sequence is not an integer")]
        public void ValidateMethodSadPath2()
        {
            var cloudEvent = new SequentialEvent
            {
                Type = "MyType",
                Source = "/MySource",
                Sequence = "abc",
                SequenceType = SequenceTypes.Integer
            };

            cloudEvent.Invoking(ce => ce.Validate())
                .Should().ThrowExactly<CloudEventValidationException>()
                .WithMessage("Invalid valid for Sequence: 'abc'.*");
        }

        [Fact(DisplayName = "Validate static method does not throw when given valid sender message")]
        public void ValidateStaticMethodHappyPath1()
        {
            var senderMessage = new SenderMessage("Hello, world!");
            
            senderMessage.Headers.Add(SequentialEvent.SequenceAttribute, "1");

            senderMessage.Headers.Add(CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => SequentialEvent.Validate(senderMessage);

            act.Should().NotThrow();
        }

        [Fact(DisplayName = "Validate static method does not throw when given valid sender message for specified protocol binding")]
        public void ValidateStaticMethodHappyPath2()
        {
            // Non-default protocol binding

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add("test-" + SequentialEvent.SequenceAttribute, "1");

            senderMessage.Headers.Add("test-" + CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add("test-" + CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add("test-" + CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add("test-" + CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add("test-" + CloudEvent.TimeAttribute, DateTime.UtcNow);

            var mockProtocolBinding = new Mock<IProtocolBinding>();
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>())).Returns<string>(header => "test-" + header);

            Action act = () => SequentialEvent.Validate(senderMessage, mockProtocolBinding.Object);

            act.Should().NotThrow();
        }

        [Fact(DisplayName = "Validate static method throws given missing Sequence header")]
        public void ValidateStaticMethodSadPath1()
        {
            // Missing Sequence

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => SequentialEvent.Validate(senderMessage);

            act.Should().ThrowExactly<CloudEventValidationException>();
        }

        [Fact(DisplayName = "Validate static method throws when SequenceType is 'Integer' and Sequence header is not an integer")]
        public void ValidateStaticMethodSadPath2()
        {
            // Missing Sequence

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add("test-" + SequentialEvent.SequenceAttribute, "abc");
            senderMessage.Headers.Add("test-" + SequentialEvent.SequenceTypeAttribute, SequenceTypes.Integer);

            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => SequentialEvent.Validate(senderMessage);

            act.Should().ThrowExactly<CloudEventValidationException>();
        }
    }
}
