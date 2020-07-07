using FluentAssertions;
using Moq;
using RockLib.Messaging.Testing;
using System;
using System.Net.Mime;
using Xunit;

namespace RockLib.Messaging.CloudEvents.Tests
{
    public class SequentialEventTests
    {
        [Fact]
        public void Constructor1HappyPath()
        {
            var cloudEvent = new SequentialEvent();

            cloudEvent.Data.Should().BeNull();
        }

        [Fact]
        public void Constructor2HappyPath()
        {
            var cloudEvent = new SequentialEvent("Hello, world!");

            cloudEvent.Data.Should().Be("Hello, world!");
        }

        [Fact]
        public void Constructor3HappyPath()
        {
            var data = new byte[] { 1, 2, 3, 4 };

            var cloudEvent = new SequentialEvent(data);

            cloudEvent.Data.Should().BeSameAs(data);
        }

        [Fact]
        public void ToSenderMessageMethodHappyPath1()
        {
            // All attributes provided

            var sequence = "1";
            var sequenceType = SequenceTypes.Integer;
            var dataContentType = new ContentType("application/xml");
            var dataSchema = new Uri("http://dataschema");
            var id = "MyId";
            Uri source = new Uri("http://source");
            var subject = "MySubject";
            var time = DateTime.UtcNow;
            var type = "MyType";

            var cloudEvent = new SequentialEvent
            {
                Sequence = sequence,
                SequenceType = sequenceType,
                DataContentType = dataContentType,
                DataSchema = dataSchema,
                Id = id,
                Source = source,
                Subject = subject,
                Time = time,
                Type = type
            };

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.Headers[SequentialEvent.SequenceAttribute].Should().BeSameAs(sequence);
            senderMessage.Headers[SequentialEvent.SequenceTypeAttribute].Should().BeSameAs(sequenceType);

            senderMessage.Headers[CloudEvent.DataContentTypeAttribute].Should().Be(dataContentType.ToString());
            senderMessage.Headers[CloudEvent.DataSchemaAttribute].Should().Be(dataSchema.ToString());
            senderMessage.Headers[CloudEvent.IdAttribute].Should().Be(id);
            senderMessage.Headers[CloudEvent.SourceAttribute].Should().Be(source.ToString());
            senderMessage.Headers[CloudEvent.SpecVersionAttribute].Should().Be("1.0");
            senderMessage.Headers[CloudEvent.SubjectAttribute].Should().Be(subject);
            senderMessage.Headers[CloudEvent.TimeAttribute].Should().Be(time.ToString("O"));
            senderMessage.Headers[CloudEvent.TypeAttribute].Should().Be(type);
        }

        [Fact]
        public void ToSenderMessageMethodHappyPath2()
        {
            // No attributes provided

            var cloudEvent = new SequentialEvent();

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.Headers.Should().NotContainKey(SequentialEvent.SequenceAttribute);
            senderMessage.Headers.Should().NotContainKey(SequentialEvent.SequenceTypeAttribute);

            senderMessage.Headers.Should().NotContainKey(CloudEvent.DataContentTypeAttribute);
            senderMessage.Headers.Should().NotContainKey(CloudEvent.DataSchemaAttribute);
            senderMessage.Headers.Should().NotContainKey(CloudEvent.IdAttribute);
            senderMessage.Headers.Should().NotContainKey(CloudEvent.SourceAttribute);
            senderMessage.Headers[CloudEvent.SpecVersionAttribute].Should().Be("1.0");
            senderMessage.Headers.Should().NotContainKey(CloudEvent.SubjectAttribute);
            senderMessage.Headers.Should().NotContainKey(CloudEvent.TimeAttribute);
            senderMessage.Headers.Should().NotContainKey(CloudEvent.TypeAttribute);
        }

        [Fact]
        public void ToSenderMessageMethodHappyPath3()
        {
            // Non-default protocol binding
        }

        [Fact]
        public void ValidateMethodHappyPath1()
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

        [Fact]
        public void ValidateMethodHappyPath2()
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

        [Fact]
        public void ValidateMethodSadPath()
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

        [Fact]
        public void CreateMethodHappyPath1()
        {
            // All attributes provided

            var receiverMessage = new FakeReceiverMessage("Hello, world!");

            var source = new Uri("http://MySource");
            var dataContentType = new ContentType("application/mycontenttype");
            var dataSchema = new Uri("http://MySource");
            var time = DateTime.UtcNow;

            receiverMessage.Headers.Add(SequentialEvent.SequenceAttribute, "1");
            receiverMessage.Headers.Add(SequentialEvent.SequenceTypeAttribute, SequenceTypes.Integer);

            receiverMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            receiverMessage.Headers.Add(CloudEvent.SourceAttribute, source);
            receiverMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            receiverMessage.Headers.Add(CloudEvent.DataContentTypeAttribute, dataContentType);
            receiverMessage.Headers.Add(CloudEvent.DataSchemaAttribute, dataSchema);
            receiverMessage.Headers.Add(CloudEvent.SubjectAttribute, "MySubject");
            receiverMessage.Headers.Add(CloudEvent.TimeAttribute, time);

            var sequenceEvent = SequentialEvent.Create(receiverMessage);

            sequenceEvent.Sequence.Should().Be("1");
            sequenceEvent.SequenceType.Should().Be(SequenceTypes.Integer);

            sequenceEvent.Id.Should().Be("MyId");
            sequenceEvent.Source.Should().BeSameAs(source);
            sequenceEvent.Type.Should().Be("MyType");
            sequenceEvent.DataContentType.Should().BeSameAs(dataContentType);
            sequenceEvent.DataSchema.Should().BeSameAs(dataSchema);
            sequenceEvent.Subject.Should().Be("MySubject");
            sequenceEvent.Time.Should().Be(time);
            sequenceEvent.AdditionalAttributes.Should().BeEmpty();
        }

        [Fact]
        public void CreateMethodHappyPath2()
        {
            // No attributes provided

            var receiverMessage = new FakeReceiverMessage("Hello, world!");

            var sequenceEvent = SequentialEvent.Create(receiverMessage);

            sequenceEvent.Sequence.Should().BeNull();
            sequenceEvent.SequenceType.Should().BeNull();

            sequenceEvent.Id.Should().BeNull();
            sequenceEvent.Source.Should().BeNull();
            sequenceEvent.Type.Should().BeNull();
            sequenceEvent.DataContentType.Should().BeNull();
            sequenceEvent.DataSchema.Should().BeNull();
            sequenceEvent.Subject.Should().BeNull();
            sequenceEvent.Time.Should().BeNull();
            sequenceEvent.AdditionalAttributes.Should().BeEmpty();
        }

        [Fact]
        public void CreateMethodHappyPath3()
        {
            // Non-default protocol binding
        }
    }
}
