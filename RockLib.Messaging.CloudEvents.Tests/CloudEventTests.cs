using FluentAssertions;
using Moq;
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
        public void ToSenderMessageMethodHappyPath5()
        {
            // No properties provided

            var cloudEvent = new TestCloudEvent();

            var senderMessage = cloudEvent.ToSenderMessage();

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
        public void ToSenderMessageMethodHappyPath6()
        {
            // Additional attributes provided

            var cloudEvent = new TestCloudEvent();
            cloudEvent.AdditionalAttributes.Add("foo", "abc");
            cloudEvent.AdditionalAttributes.Add("bar", 123);

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.Headers.Should().ContainKey("foo").WhichValue.Should().Be("abc");
            senderMessage.Headers.Should().ContainKey("bar").WhichValue.Should().Be(123);
        }

        [Fact]
        public void ToSenderMessageMethodHappyPath7()
        {
            // Non-default protocol binding

            var mockProtocolBinding = new Mock<IProtocolBinding>();
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>()))
                .Returns<string>(header => "test-" + header);

            var id = "MyId";

            var cloudEvent = new TestCloudEvent { Id = id };

            var senderMessage = cloudEvent.ToSenderMessage(mockProtocolBinding.Object);

            senderMessage.Headers.Should().ContainKey("test-" + CloudEvent.IdAttribute).WhichValue.Should().BeSameAs(id);
        }

        [Fact]
        public void ValidateCoreMethodHappyPath1()
        {
            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => TestCloudEvent.Validate(senderMessage);

            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateCoreMethodHappyPath2()
        {
            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, "http://MySource");
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => TestCloudEvent.Validate(senderMessage);

            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateCoreMethodHappyPath3()
        {
            // Non-default protocol binding

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add("test-" + CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add("test-" + CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add("test-" + CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add("test-" + CloudEvent.TimeAttribute, DateTime.UtcNow);

            var mockProtocolBinding = new Mock<IProtocolBinding>();
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>())).Returns<string>(header => "test-" + header);

            Action act = () => TestCloudEvent.Validate(senderMessage, mockProtocolBinding.Object);

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

            senderMessage.Headers.Add(CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => TestCloudEvent.Validate(senderMessage);

            act.Should().NotThrow();

            senderMessage.Headers[CloudEvent.IdAttribute].Should().NotBeNull();
        }

        [Fact]
        public void ValidateCoreMethodSadPath3()
        {
            // Missing Source

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => TestCloudEvent.Validate(senderMessage);

            act.Should().ThrowExactly<CloudEventValidationException>();
        }

        [Fact]
        public void ValidateCoreMethodSadPath4()
        {
            // Missing Type

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => TestCloudEvent.Validate(senderMessage);

            act.Should().ThrowExactly<CloudEventValidationException>();
        }

        [Fact]
        public void ValidateCoreMethodSadPath5()
        {
            // Missing Time

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");

            Action act = () => TestCloudEvent.Validate(senderMessage);

            act.Should().NotThrow();

            senderMessage.Headers[CloudEvent.TimeAttribute].Should().NotBeNull();
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

            receiverMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            receiverMessage.Headers.Add(CloudEvent.SourceAttribute, source);
            receiverMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            receiverMessage.Headers.Add(CloudEvent.DataContentTypeAttribute, dataContentType);
            receiverMessage.Headers.Add(CloudEvent.DataSchemaAttribute, dataSchema);
            receiverMessage.Headers.Add(CloudEvent.SubjectAttribute, "MySubject");
            receiverMessage.Headers.Add(CloudEvent.TimeAttribute, time);

            var cloudEvent = TestCloudEvent.Create(receiverMessage);

            cloudEvent.Id.Should().Be("MyId");
            cloudEvent.Source.Should().BeSameAs(source);
            cloudEvent.Type.Should().Be("MyType");
            cloudEvent.DataContentType.Should().BeSameAs(dataContentType);
            cloudEvent.DataSchema.Should().BeSameAs(dataSchema);
            cloudEvent.Subject.Should().Be("MySubject");
            cloudEvent.Time.Should().Be(time);
            cloudEvent.AdditionalAttributes.Should().BeEmpty();
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
            cloudEvent.AdditionalAttributes.Should().BeEmpty();
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

            receiverMessage.Headers.Add(CloudEvent.SourceAttribute, source);
            receiverMessage.Headers.Add(CloudEvent.DataContentTypeAttribute, dataContentType);
            receiverMessage.Headers.Add(CloudEvent.DataSchemaAttribute, dataSchema);
            receiverMessage.Headers.Add(CloudEvent.TimeAttribute, time);

            var cloudEvent = TestCloudEvent.Create(receiverMessage);

            cloudEvent.Source.ToString().Should().Be(source);
            cloudEvent.DataContentType.ToString().Should().Be(dataContentType);
            cloudEvent.DataSchema.ToString().Should().Be(dataSchema);
            cloudEvent.Time.GetValueOrDefault().ToString("O").Should().Be(time);
            cloudEvent.AdditionalAttributes.Should().BeEmpty();
        }

        [Fact]
        public void CreateCoreMethodHappyPath6()
        {
            // Additional attributes provided

            var receiverMessage = new FakeReceiverMessage("Hello, world!");
            receiverMessage.Headers.Add("foo", "abc");
            receiverMessage.Headers.Add("bar", 123);

            var cloudEvent = TestCloudEvent.Create(receiverMessage);

            cloudEvent.AdditionalAttributes.Should().HaveCount(2);
            cloudEvent.AdditionalAttributes.Should().ContainKey("foo").WhichValue.Should().Be("abc");
            cloudEvent.AdditionalAttributes.Should().ContainKey("bar").WhichValue.Should().Be(123);
        }


        [Fact]
        public void CreateCoreMethodHappyPath7()
        {
            // Non-default protocol binding

            var receiverMessage = new FakeReceiverMessage("Hello, world!");

            receiverMessage.Headers.Add("test-" + CloudEvent.IdAttribute, "MyId");

            var mockProtocolBinding = new Mock<IProtocolBinding>();
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>())).Returns<string>(header => "test-" + header);

            var cloudEvent = TestCloudEvent.Create(receiverMessage, mockProtocolBinding.Object);

            cloudEvent.Id.Should().Be("MyId");
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
            public static void Validate(SenderMessage senderMessage, IProtocolBinding protocolBinding = null) =>
                ValidateCore(senderMessage, protocolBinding);

            public static TestCloudEvent Create(IReceiverMessage receiverMessage, IProtocolBinding protocolBinding = null) =>
                CreateCore<TestCloudEvent>(receiverMessage, protocolBinding);
        }
    }
}
