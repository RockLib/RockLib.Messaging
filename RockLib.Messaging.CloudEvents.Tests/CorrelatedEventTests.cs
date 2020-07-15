using FluentAssertions;
using Moq;
using RockLib.Messaging.Testing;
using System;
using System.Net.Mime;
using Xunit;

namespace RockLib.Messaging.CloudEvents.Tests
{
    public class CorrelatedEventTests
    {
        [Fact(DisplayName = "Constructor 1 does not initialize anything")]
        public void Constructor1HappyPath()
        {
            var cloudEvent = new CorrelatedEvent();

            cloudEvent.StringData.Should().BeNull();
            cloudEvent.BinaryData.Should().BeNull();
            cloudEvent.AdditionalAttributes.Should().BeEmpty();
            cloudEvent.CorrelationId.Should().NotBeNull();
            cloudEvent.DataContentType.Should().BeNull();
            cloudEvent.DataSchema.Should().BeNull();
            cloudEvent.Source.Should().BeNull();
            cloudEvent.SpecVersion.Should().Be("1.0");
            cloudEvent.Subject.Should().BeNull();
            cloudEvent.Type.Should().BeNull();
        }

        [Fact(DisplayName = "Constructor 2 sets expected properties")]
        public void Constructor2HappyPath()
        {
            var source = new CorrelatedEvent
            {
                CorrelationId = "MyCorrelationId",
                Id = "MyId",
                Time = new DateTime(2020, 7, 9, 22, 21, 37, DateTimeKind.Local),
                Source = new Uri("http://mysource/"),
                Type = "MyType",
                DataContentType = new ContentType("application/json") { CharSet = "utf-8" },
                DataSchema = new Uri("http://mydataschema/"),
                Subject = "MySubject"
            };

            var cloudEvent = new CorrelatedEvent(source);

            cloudEvent.CorrelationId.Should().Be(source.CorrelationId);

            cloudEvent.Source.Should().BeSameAs(source.Source);
            cloudEvent.Type.Should().BeSameAs(source.Type);
            cloudEvent.DataContentType.Should().BeSameAs(source.DataContentType);
            cloudEvent.DataSchema.Should().BeSameAs(source.DataSchema);
            cloudEvent.Subject.Should().BeSameAs(source.Subject);

            cloudEvent.Id.Should().NotBe(source.Id);
            cloudEvent.Time.Should().NotBe(source.Time);
        }

        [Fact(DisplayName = "Constructor 2 throws is source parameter is null")]
        public void Constructor2SadPath()
        {
            CorrelatedEvent source = null;

            Action act = () => new CorrelatedEvent(source);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*source*");
        }

        [Fact(DisplayName = "Constructor 3 maps correlated event attributes from receiver message headers")]
        public void Constructor3HappyPath1()
        {
            // All attributes provided

            var receiverMessage = new FakeReceiverMessage("Hello, world!");

            var source = new Uri("http://MySource");
            var dataContentType = new ContentType("application/mycontenttype");
            var dataSchema = new Uri("http://MySource");
            var time = DateTime.UtcNow;

            receiverMessage.Headers.Add(CorrelatedEvent.CorrelationIdAttribute, "MyCorrelationId");

            receiverMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            receiverMessage.Headers.Add(CloudEvent.SourceAttribute, source);
            receiverMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            receiverMessage.Headers.Add(CloudEvent.DataContentTypeAttribute, dataContentType);
            receiverMessage.Headers.Add(CloudEvent.DataSchemaAttribute, dataSchema);
            receiverMessage.Headers.Add(CloudEvent.SubjectAttribute, "MySubject");
            receiverMessage.Headers.Add(CloudEvent.TimeAttribute, time);

            var correlatedEvent = new CorrelatedEvent(receiverMessage);

            correlatedEvent.CorrelationId.Should().Be("MyCorrelationId");

            correlatedEvent.Id.Should().Be("MyId");
            correlatedEvent.Source.Should().BeSameAs(source);
            correlatedEvent.Type.Should().Be("MyType");
            correlatedEvent.DataContentType.Should().BeSameAs(dataContentType);
            correlatedEvent.DataSchema.Should().BeSameAs(dataSchema);
            correlatedEvent.Subject.Should().Be("MySubject");
            correlatedEvent.Time.Should().Be(time);
            correlatedEvent.AdditionalAttributes.Should().BeEmpty();
        }

        [Fact(DisplayName = "Constructor 3 does not require any correlated event attributes to be mapped")]
        public void Constructor3HappyPath2()
        {
            // No attributes provided

            var receiverMessage = new FakeReceiverMessage("Hello, world!");

            var correlatedEvent = new CorrelatedEvent(receiverMessage);

            correlatedEvent.Source.Should().BeNull();
            correlatedEvent.Type.Should().BeNull();
            correlatedEvent.DataContentType.Should().BeNull();
            correlatedEvent.DataSchema.Should().BeNull();
            correlatedEvent.Subject.Should().BeNull();
            correlatedEvent.AdditionalAttributes.Should().BeEmpty();
        }

        [Fact(DisplayName = "Constructor 3 maps with the specified protocol binding")]
        public void Constructor3HappyPath3()
        {
            // Non-default protocol binding

            var receiverMessage = new FakeReceiverMessage("Hello, world!");

            var source = new Uri("http://MySource");
            var dataContentType = new ContentType("application/mycontenttype");
            var dataSchema = new Uri("http://MySource");
            var time = DateTime.UtcNow;

            receiverMessage.Headers.Add("test-" + CorrelatedEvent.CorrelationIdAttribute, "MyCorrelationId");

            receiverMessage.Headers.Add("test-" + CloudEvent.IdAttribute, "MyId");
            receiverMessage.Headers.Add("test-" + CloudEvent.SourceAttribute, source);
            receiverMessage.Headers.Add("test-" + CloudEvent.TypeAttribute, "MyType");
            receiverMessage.Headers.Add("test-" + CloudEvent.DataContentTypeAttribute, dataContentType);
            receiverMessage.Headers.Add("test-" + CloudEvent.DataSchemaAttribute, dataSchema);
            receiverMessage.Headers.Add("test-" + CloudEvent.SubjectAttribute, "MySubject");
            receiverMessage.Headers.Add("test-" + CloudEvent.TimeAttribute, time);

            var mockProtocolBinding = new Mock<IProtocolBinding>();
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>())).Returns<string>(header => "test-" + header);

            var correlatedEvent = new CorrelatedEvent(receiverMessage, mockProtocolBinding.Object);

            correlatedEvent.CorrelationId.Should().Be("MyCorrelationId");

            correlatedEvent.Id.Should().Be("MyId");
            correlatedEvent.Source.Should().BeSameAs(source);
            correlatedEvent.Type.Should().Be("MyType");
            correlatedEvent.DataContentType.Should().BeSameAs(dataContentType);
            correlatedEvent.DataSchema.Should().BeSameAs(dataSchema);
            correlatedEvent.Subject.Should().Be("MySubject");
            correlatedEvent.Time.Should().Be(time);
            correlatedEvent.AdditionalAttributes.Should().BeEmpty();
        }

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

            cloudEvent.Invoking(evt => evt.CorrelationId = null).Should()
                .ThrowExactly<ArgumentNullException>()
                .WithMessage("*value*");
        }

        [Fact(DisplayName = "ToSenderMessage method maps correlated event attributes to sender message headers")]
        public void ToSenderMessageMethodHappyPath1()
        {
            // All attributes provided

            var correlationId = "MyCorrelationId";
            var dataContentType = new ContentType("application/xml");
            var dataSchema = new Uri("http://dataschema");
            var id = "MyId";
            Uri source = new Uri("http://source");
            var subject = "MySubject";
            var time = DateTime.UtcNow;
            var type = "MyType";

            var cloudEvent = new CorrelatedEvent
            {
                CorrelationId = correlationId,
                DataContentType = dataContentType,
                DataSchema = dataSchema,
                Id = id,
                Source = source,
                Subject = subject,
                Time = time,
                Type = type
            };

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.Headers[CorrelatedEvent.CorrelationIdAttribute].Should().BeSameAs(correlationId);

            senderMessage.Headers[CloudEvent.DataContentTypeAttribute].Should().Be(dataContentType.ToString());
            senderMessage.Headers[CloudEvent.DataSchemaAttribute].Should().Be(dataSchema.ToString());
            senderMessage.Headers[CloudEvent.IdAttribute].Should().Be(id);
            senderMessage.Headers[CloudEvent.SourceAttribute].Should().Be(source.ToString());
            senderMessage.Headers[CloudEvent.SpecVersionAttribute].Should().Be("1.0");
            senderMessage.Headers[CloudEvent.SubjectAttribute].Should().Be(subject);
            senderMessage.Headers[CloudEvent.TimeAttribute].Should().Be(time.ToString("O"));
            senderMessage.Headers[CloudEvent.TypeAttribute].Should().Be(type);
        }

        [Fact(DisplayName = "ToSenderMessage method does not map null correlated cloud event attributes to sender message headers")]
        public void ToSenderMessageMethodHappyPath2()
        {
            // Minimal attributes provided

            var cloudEvent = new CorrelatedEvent
            {
                Id = "MyId",
                Source = new Uri("http://MySource"),
                Type = "MyType"
            };

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.Headers.Should().ContainKey(CorrelatedEvent.CorrelationIdAttribute);

            senderMessage.Headers.Should().NotContainKey(CloudEvent.DataContentTypeAttribute);
            senderMessage.Headers.Should().NotContainKey(CloudEvent.DataSchemaAttribute);
            senderMessage.Headers[CloudEvent.SpecVersionAttribute].Should().Be("1.0");
            senderMessage.Headers.Should().NotContainKey(CloudEvent.SubjectAttribute);
        }

        [Fact(DisplayName = "ToSenderMessage method applies specified protocol binding to each attribute")]
        public void ToSenderMessageMethodHappyPath3()
        {
            // Non-default protocol binding

            var correlationId = "MyCorrelationId";
            var dataContentType = new ContentType("application/xml");
            var dataSchema = new Uri("http://dataschema");
            var id = "MyId";
            Uri source = new Uri("http://source");
            var subject = "MySubject";
            var time = DateTime.UtcNow;
            var type = "MyType";

            var mockProtocolBinding = new Mock<IProtocolBinding>();
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>())).Returns<string>(header => "test-" + header);

            var cloudEvent = new CorrelatedEvent
            {
                CorrelationId = correlationId,
                DataContentType = dataContentType,
                DataSchema = dataSchema,
                Id = id,
                Source = source,
                Subject = subject,
                Time = time,
                Type = type,
                ProtocolBinding = mockProtocolBinding.Object
            };

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.Headers["test-" + CorrelatedEvent.CorrelationIdAttribute].Should().BeSameAs(correlationId);

            senderMessage.Headers["test-" + CloudEvent.DataContentTypeAttribute].Should().Be(dataContentType.ToString());
            senderMessage.Headers["test-" + CloudEvent.DataSchemaAttribute].Should().Be(dataSchema.ToString());
            senderMessage.Headers["test-" + CloudEvent.IdAttribute].Should().Be(id);
            senderMessage.Headers["test-" + CloudEvent.SourceAttribute].Should().Be(source.ToString());
            senderMessage.Headers["test-" + CloudEvent.SpecVersionAttribute].Should().Be("1.0");
            senderMessage.Headers["test-" + CloudEvent.SubjectAttribute].Should().Be(subject);
            senderMessage.Headers["test-" + CloudEvent.TimeAttribute].Should().Be(time.ToString("O"));
            senderMessage.Headers["test-" + CloudEvent.TypeAttribute].Should().Be(type);
        }

        [Fact(DisplayName = "Validate method does not throw when given valid sender message")]
        public void ValidateMethodHappyPath1()
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

        [Fact(DisplayName = "Validate method does not throw when given valid sender message for specified protocol binding")]
        public void ValidateMethodHappyPath2()
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

        [Fact(DisplayName = "Validate method throws given missing CorrelationId header")]
        public void ValidateMethodSadPath()
        {
            // Missing CorrelationId

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => CorrelatedEvent.Validate(senderMessage);

            act.Should().ThrowExactly<CloudEventValidationException>();
        }
    }
}
