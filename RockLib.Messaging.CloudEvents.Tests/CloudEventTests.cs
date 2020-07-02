using FluentAssertions;
using RockLib.Messaging.Testing;
using System;
using System.Net.Mime;
using Xunit;

namespace RockLib.Messaging.CloudEvents.Tests
{
    public class CloudEventTests
    {
        [Fact]
        public void SetDataMethod1HappyPath()
        {
            var stringData = "Hello, world!";

            var cloudEvent = new TestCloudEvent();

            cloudEvent.SetData(stringData);

            cloudEvent.Data.Should().BeSameAs(stringData);
        }

        [Fact]
        public void SetDataMethod2HappyPath()
        {
            var binaryData = new byte[] { 1, 2, 3, 4 };

            var cloudEvent = new TestCloudEvent();

            cloudEvent.SetData(binaryData);

            cloudEvent.Data.Should().BeSameAs(binaryData);
        }

        [Fact]
        public void ToSenderMessageMethodHappyPath1()
        {
            var stringData = "Hello, world!";

            var cloudEvent = new TestCloudEvent();
            cloudEvent.SetData(stringData);

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.StringPayload.Should().BeSameAs(stringData);
        }

        [Fact]
        public void ToSenderMessageMethodHappyPath2()
        {
            var binaryData = new byte[] { 1, 2, 3, 4 };

            var cloudEvent = new TestCloudEvent();
            cloudEvent.SetData(binaryData);

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.BinaryPayload.Should().BeSameAs(binaryData);
        }

        [Fact]
        public void ToSenderMessageMethodHappyPath3()
        {
            // null Data

            var cloudEvent = new TestCloudEvent();

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.StringPayload.Should().Be("");
        }

        [Fact]
        public void ToSenderMessageMethodHappyPath4()
        {
            // All properties provided

            var dataContentType = new ContentType("application/xml");
            var dataSchema = new Uri("http://dataschema");
            var id = "MyId";
            Uri source = new Uri("http://source");
            var subject = "MySubject";
            var time = DateTime.UtcNow;
            var type = "MyType";

            var cloudEvent = new TestCloudEvent
            {
                DataContentType = dataContentType,
                DataSchema = dataSchema,
                Id = id,
                Source = source,
                Subject = subject,
                Time = time,
                Type = type
            };

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.Headers[CloudEvent.DataContentTypeHeader].Should().Be(dataContentType.ToString());
            senderMessage.Headers[CloudEvent.DataSchemaHeader].Should().Be(dataSchema.ToString());
            senderMessage.Headers[CloudEvent.IdHeader].Should().Be(id);
            senderMessage.Headers[CloudEvent.SourceHeader].Should().Be(source.ToString());
            senderMessage.Headers[CloudEvent.SpecVersionHeader].Should().Be("1.0");
            senderMessage.Headers[CloudEvent.SubjectHeader].Should().Be(subject);
            senderMessage.Headers[CloudEvent.TimeHeader].Should().Be(time.ToString("O"));
            senderMessage.Headers[CloudEvent.TypeHeader].Should().Be(type);
        }

        [Fact]
        public void ToSenderMessageMethodHappyPath5()
        {
            // No properties provided

            var cloudEvent = new TestCloudEvent();

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.Headers.Should().NotContainKey(CloudEvent.DataContentTypeHeader);
            senderMessage.Headers.Should().NotContainKey(CloudEvent.DataSchemaHeader);
            senderMessage.Headers.Should().NotContainKey(CloudEvent.IdHeader);
            senderMessage.Headers.Should().NotContainKey(CloudEvent.SourceHeader);
            senderMessage.Headers[CloudEvent.SpecVersionHeader].Should().Be("1.0");
            senderMessage.Headers.Should().NotContainKey(CloudEvent.SubjectHeader);
            senderMessage.Headers.Should().NotContainKey(CloudEvent.TimeHeader);
            senderMessage.Headers.Should().NotContainKey(CloudEvent.TypeHeader);
        }

        [Fact]
        public void ValidateCoreMethodHappyPath1()
        {
            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.IdHeader, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceHeader, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TypeHeader, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeHeader, DateTime.UtcNow);

            Action act = () => TestCloudEvent.Validate(senderMessage);

            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateCoreMethodHappyPath2()
        {
            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.IdHeader, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceHeader, "http://MySource");
            senderMessage.Headers.Add(CloudEvent.TypeHeader, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeHeader, DateTime.UtcNow);

            Action act = () => TestCloudEvent.Validate(senderMessage);

            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateCoreMethodSadPath1()
        {
            // Null senderMessage

            Action act = () => TestCloudEvent.Validate(null);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*senderMessage*");
        }

        [Fact]
        public void ValidateCoreMethodSadPath2()
        {
            // Missing Id

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.SourceHeader, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TypeHeader, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeHeader, DateTime.UtcNow);

            Action act = () => TestCloudEvent.Validate(senderMessage);

            act.Should().NotThrow();

            senderMessage.Headers[CloudEvent.IdHeader].Should().NotBeNull();
        }

        [Fact]
        public void ValidateCoreMethodSadPath3()
        {
            // Missing Source

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.IdHeader, "MyId");
            senderMessage.Headers.Add(CloudEvent.TypeHeader, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeHeader, DateTime.UtcNow);

            Action act = () => TestCloudEvent.Validate(senderMessage);

            act.Should().ThrowExactly<CloudEventValidationException>();
        }

        [Fact]
        public void ValidateCoreMethodSadPath4()
        {
            // Missing Type

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.IdHeader, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceHeader, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TimeHeader, DateTime.UtcNow);

            Action act = () => TestCloudEvent.Validate(senderMessage);

            act.Should().ThrowExactly<CloudEventValidationException>();
        }

        [Fact]
        public void ValidateCoreMethodSadPath5()
        {
            // Missing Time

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.IdHeader, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceHeader, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TypeHeader, "MyType");

            Action act = () => TestCloudEvent.Validate(senderMessage);

            act.Should().NotThrow();

            senderMessage.Headers[CloudEvent.TimeHeader].Should().NotBeNull();
        }

        [Fact]
        public void CreateCoreMethodHappyPath1()
        {
            var binaryData = new byte[] { 1, 2, 3, 4 };

            var receiverMessage = new FakeReceiverMessage(binaryData);

            var cloudEvent = TestCloudEvent.Create(receiverMessage);

            cloudEvent.Data.Should().BeSameAs(binaryData);
        }

        [Fact]
        public void CreateCoreMethodHappyPath2()
        {
            var stringData = "Hello, world!";

            var receiverMessage = new FakeReceiverMessage(stringData);

            var cloudEvent = TestCloudEvent.Create(receiverMessage);

            cloudEvent.Data.Should().BeSameAs(stringData);
        }

        [Fact]
        public void CreateCoreMethodHappyPath3()
        {
            // All properties provided

            var receiverMessage = new FakeReceiverMessage("Hello, world!");

            var source = new Uri("http://MySource");
            var dataContentType = new ContentType("application/mycontenttype");
            var dataSchema = new Uri("http://MySource");
            var time = DateTime.UtcNow;

            receiverMessage.Headers.Add(CloudEvent.IdHeader, "MyId");
            receiverMessage.Headers.Add(CloudEvent.SourceHeader, source);
            receiverMessage.Headers.Add(CloudEvent.TypeHeader, "MyType");
            receiverMessage.Headers.Add(CloudEvent.DataContentTypeHeader, dataContentType);
            receiverMessage.Headers.Add(CloudEvent.DataSchemaHeader, dataSchema);
            receiverMessage.Headers.Add(CloudEvent.SubjectHeader, "MySubject");
            receiverMessage.Headers.Add(CloudEvent.TimeHeader, time);

            var cloudEvent = TestCloudEvent.Create(receiverMessage);

            cloudEvent.Id.Should().Be("MyId");
            cloudEvent.Source.Should().BeSameAs(source);
            cloudEvent.Type.Should().Be("MyType");
            cloudEvent.DataContentType.Should().BeSameAs(dataContentType);
            cloudEvent.DataSchema.Should().BeSameAs(dataSchema);
            cloudEvent.Subject.Should().Be("MySubject");
            cloudEvent.Time.Should().Be(time);
        }

        [Fact]
        public void CreateCoreMethodHappyPath4()
        {
            // No properties provided

            var receiverMessage = new FakeReceiverMessage("Hello, world!");

            var cloudEvent = TestCloudEvent.Create(receiverMessage);

            cloudEvent.Id.Should().BeNull();
            cloudEvent.Source.Should().BeNull();
            cloudEvent.Type.Should().BeNull();
            cloudEvent.DataContentType.Should().BeNull();
            cloudEvent.DataSchema.Should().BeNull();
            cloudEvent.Subject.Should().BeNull();
            cloudEvent.Time.Should().BeNull();
        }

        [Fact]
        public void CreateCoreMethodHappyPath5()
        {
            // Alternate property types provided

            var receiverMessage = new FakeReceiverMessage("Hello, world!");

            var source = new Uri("http://MySource").ToString();
            var dataContentType = new ContentType("application/mycontenttype").ToString();
            var dataSchema = new Uri("http://MySource").ToString();
            var time = DateTime.UtcNow.ToString("O");

            receiverMessage.Headers.Add(CloudEvent.SourceHeader, source);
            receiverMessage.Headers.Add(CloudEvent.DataContentTypeHeader, dataContentType);
            receiverMessage.Headers.Add(CloudEvent.DataSchemaHeader, dataSchema);
            receiverMessage.Headers.Add(CloudEvent.TimeHeader, time);

            var cloudEvent = TestCloudEvent.Create(receiverMessage);

            cloudEvent.Source.ToString().Should().Be(source);
            cloudEvent.DataContentType.ToString().Should().Be(dataContentType);
            cloudEvent.DataSchema.ToString().Should().Be(dataSchema);
            cloudEvent.Time.GetValueOrDefault().ToString("O").Should().Be(time);
        }

        [Fact]
        public void CreateCoreMethodSadPath()
        {
            // Null receiverMessage

            Action act = () => TestCloudEvent.Create(null);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*receiverMessage*");
        }

        private class TestCloudEvent : CloudEvent
        {
            public static void Validate(SenderMessage senderMessage) => ValidateCore(senderMessage);

            public static TestCloudEvent Create(IReceiverMessage receiverMessage) => CreateCore<TestCloudEvent>(receiverMessage);
        }
    }
}
