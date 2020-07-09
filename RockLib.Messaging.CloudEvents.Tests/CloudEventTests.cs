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
        #region Constructors

        [Fact(DisplayName = "Constructor 1 does not initialize anything")]
        public void Constructor1HappyPath()
        {
            var cloudEvent = new CloudEvent();

            cloudEvent.Data.Should().BeNull();
            cloudEvent.AdditionalAttributes.Should().BeEmpty();
            cloudEvent.DataContentType.Should().BeNull();
            cloudEvent.DataSchema.Should().BeNull();
            cloudEvent.Source.Should().BeNull();
            cloudEvent.SpecVersion.Should().Be("1.0");
            cloudEvent.Subject.Should().BeNull();
            cloudEvent.Type.Should().BeNull();
        }

        [Fact(DisplayName = "Constructor 2 initializes Data property")]
        public void Constructor2HappyPath()
        {
            var cloudEvent = new CloudEvent("Hello, world!");

            cloudEvent.Data.Should().Be("Hello, world!");
            cloudEvent.AdditionalAttributes.Should().BeEmpty();
            cloudEvent.DataContentType.Should().BeNull();
            cloudEvent.DataSchema.Should().BeNull();
            cloudEvent.Source.Should().BeNull();
            cloudEvent.SpecVersion.Should().Be("1.0");
            cloudEvent.Subject.Should().BeNull();
            cloudEvent.Type.Should().BeNull();
        }

        [Fact(DisplayName = "Constructor 3 initializes Data property")]
        public void Constructor3HappyPath()
        {
            var data = new byte[] { 1, 2, 3, 4 };

            var cloudEvent = new CloudEvent(data);

            cloudEvent.Data.Should().BeSameAs(data);
            cloudEvent.AdditionalAttributes.Should().BeEmpty();
            cloudEvent.DataContentType.Should().BeNull();
            cloudEvent.DataSchema.Should().BeNull();
            cloudEvent.Source.Should().BeNull();
            cloudEvent.SpecVersion.Should().Be("1.0");
            cloudEvent.Subject.Should().BeNull();
            cloudEvent.Type.Should().BeNull();
        }

        [Fact(DisplayName = "Constructor 4 creates cloud event with binary data")]
        public void Constructor4HappyPath1()
        {
            var binaryData = new byte[] { 1, 2, 3, 4 };

            var receiverMessage = new FakeReceiverMessage(binaryData);

            var cloudEvent = new CloudEvent(receiverMessage);

            cloudEvent.Data.Should().BeSameAs(binaryData);
        }

        [Fact(DisplayName = "Constructor 4 creates cloud event with binary data")]
        public void Constructor4HappyPath2()
        {
            var stringData = "Hello, world!";

            var receiverMessage = new FakeReceiverMessage(stringData);

            var cloudEvent = new CloudEvent(receiverMessage);

            cloudEvent.Data.Should().BeSameAs(stringData);
        }

        [Fact(DisplayName = "Constructor 4 maps cloud event attributes from receiver message headers")]
        public void Constructor4HappyPath3()
        {
            // All attributes provided

            var receiverMessage = new FakeReceiverMessage("Hello, world!");

            var source = new Uri("http://MySource");
            var dataContentType = new ContentType("application/mycontenttype");
            var dataSchema = new Uri("http://MySource");
            var time = DateTime.UtcNow;

            receiverMessage.Headers.Add(CloudEvent.SpecVersionAttribute, "1.0");
            receiverMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            receiverMessage.Headers.Add(CloudEvent.SourceAttribute, source);
            receiverMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            receiverMessage.Headers.Add(CloudEvent.DataContentTypeAttribute, dataContentType);
            receiverMessage.Headers.Add(CloudEvent.DataSchemaAttribute, dataSchema);
            receiverMessage.Headers.Add(CloudEvent.SubjectAttribute, "MySubject");
            receiverMessage.Headers.Add(CloudEvent.TimeAttribute, time);

            var cloudEvent = new CloudEvent(receiverMessage);

            cloudEvent.SpecVersion.Should().Be("1.0");
            cloudEvent.Id.Should().Be("MyId");
            cloudEvent.Source.Should().BeSameAs(source);
            cloudEvent.Type.Should().Be("MyType");
            cloudEvent.DataContentType.Should().BeSameAs(dataContentType);
            cloudEvent.DataSchema.Should().BeSameAs(dataSchema);
            cloudEvent.Subject.Should().Be("MySubject");
            cloudEvent.Time.Should().Be(time);
            cloudEvent.AdditionalAttributes.Should().BeEmpty();
        }

        [Fact(DisplayName = "Constructor 4 does not require any cloud event attributes to be mapped")]
        public void Constructor4HappyPath4()
        {
            // No attributes provided

            var receiverMessage = new FakeReceiverMessage("Hello, world!");

            var cloudEvent = new CloudEvent(receiverMessage);

            cloudEvent.SpecVersion.Should().Be("1.0");
            cloudEvent.Source.Should().BeNull();
            cloudEvent.Type.Should().BeNull();
            cloudEvent.DataContentType.Should().BeNull();
            cloudEvent.DataSchema.Should().BeNull();
            cloudEvent.Subject.Should().BeNull();
            cloudEvent.AdditionalAttributes.Should().BeEmpty();
        }

        [Fact(DisplayName = "Constructor 4 maps from stringly typed receiver message headers")]
        public void Constructor4HappyPath5()
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

            var cloudEvent = new CloudEvent(receiverMessage);

            cloudEvent.Source.ToString().Should().Be(source);
            cloudEvent.DataContentType.ToString().Should().Be(dataContentType);
            cloudEvent.DataSchema.ToString().Should().Be(dataSchema);
            cloudEvent.Time.ToString("O").Should().Be(time);
            cloudEvent.AdditionalAttributes.Should().BeEmpty();
        }

        [Fact(DisplayName = "Constructor 4 maps additional attributes verbatim")]
        public void Constructor4HappyPath6()
        {
            // Additional attributes provided

            var receiverMessage = new FakeReceiverMessage("Hello, world!");
            receiverMessage.Headers.Add("foo", "abc");
            receiverMessage.Headers.Add("bar", 123);

            var mockProtocolBinding = new Mock<IProtocolBinding>();
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>())).Returns<string>(header => "test-" + header);

            var cloudEvent = new CloudEvent(receiverMessage, mockProtocolBinding.Object);

            cloudEvent.AdditionalAttributes.Should().HaveCount(2);
            cloudEvent.AdditionalAttributes.Should().ContainKey("foo").WhichValue.Should().Be("abc");
            cloudEvent.AdditionalAttributes.Should().ContainKey("bar").WhichValue.Should().Be(123);
        }


        [Fact(DisplayName = "Constructor 4 maps with the specified protocol binding")]
        public void Constructor4HappyPath7()
        {
            // Non-default protocol binding

            var receiverMessage = new FakeReceiverMessage("Hello, world!");
            receiverMessage.Headers.Add("foo", "abc");
            receiverMessage.Headers.Add("test-" + CloudEvent.IdAttribute, "MyId");

            var mockProtocolBinding = new Mock<IProtocolBinding>();
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>())).Returns<string>(header => "test-" + header);

            var cloudEvent = new CloudEvent(receiverMessage, mockProtocolBinding.Object);

            cloudEvent.Id.Should().Be("MyId");
            cloudEvent.AdditionalAttributes.Should().ContainKey("foo").WhichValue.Should().Be("abc");
        }

        [Fact(DisplayName = "Constructor 4 throws when receiverMessage parameter is null")]
        public void Constructor4SadPath1()
        {
            // Null receiverMessage

            Action act = () => new CloudEvent((IReceiverMessage)null);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*receiverMessage*");
        }

        [Fact(DisplayName = "Constructor 4 throws when specversion header is not '1.0'")]
        public void Constructor4SadPath2()
        {
            // Invalid specversion

            var receiverMessage = new FakeReceiverMessage("Hello, world!");
            receiverMessage.Headers.Add(CloudEvent.SpecVersionAttribute, "0.0");

            Action act = () => new CloudEvent(receiverMessage);

            act.Should().ThrowExactly<CloudEventValidationException>();
        }

        #endregion

        #region SetData

        [Fact(DisplayName = "SetData method 1 sets the Data property")]
        public void SetDataMethod1HappyPath()
        {
            var stringData = "Hello, world!";

            var cloudEvent = new CloudEvent();

            cloudEvent.SetData(stringData);

            cloudEvent.Data.Should().BeSameAs(stringData);
        }

        [Fact(DisplayName = "SetData method 2 sets the Data property")]
        public void SetDataMethod2HappyPath()
        {
            var binaryData = new byte[] { 1, 2, 3, 4 };

            var cloudEvent = new CloudEvent();

            cloudEvent.SetData(binaryData);

            cloudEvent.Data.Should().BeSameAs(binaryData);
        }

        #endregion

        #region ToSenderMessage

        [Fact(DisplayName = "ToSenderMessage method maps string data to StringPayload")]
        public void ToSenderMessageMethodHappyPath1()
        {
            var stringData = "Hello, world!";

            var cloudEvent = new CloudEvent
            {
                Id = "MyId",
                Source = new Uri("http://mysource/"),
                Type = "MyType"
            };
            cloudEvent.SetData(stringData);

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.StringPayload.Should().BeSameAs(stringData);
        }

        [Fact(DisplayName = "ToSenderMessage method maps binary data to BinaryPayload")]
        public void ToSenderMessageMethodHappyPath2()
        {
            var binaryData = new byte[] { 1, 2, 3, 4 };

            var cloudEvent = new CloudEvent
            {
                Id = "MyId",
                Source = new Uri("http://mysource/"),
                Type = "MyType"
            };
            cloudEvent.SetData(binaryData);

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.BinaryPayload.Should().BeSameAs(binaryData);
        }

        [Fact(DisplayName = "ToSenderMessage method maps null data to empty StringPayload")]
        public void ToSenderMessageMethodHappyPath3()
        {
            // null Data

            var cloudEvent = new CloudEvent
            {
                Id = "MyId",
                Source = new Uri("http://mysource/"),
                Type = "MyType"
            };

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.StringPayload.Should().Be("");
        }

        [Fact(DisplayName = "ToSenderMessage method maps cloud event attributes to sender message headers")]
        public void ToSenderMessageMethodHappyPath4()
        {
            // All attributes provided

            var dataContentType = new ContentType("application/xml");
            var dataSchema = new Uri("http://dataschema");
            var id = "MyId";
            Uri source = new Uri("http://source");
            var subject = "MySubject";
            var time = DateTime.UtcNow;
            var type = "MyType";

            var cloudEvent = new CloudEvent
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

        [Fact(DisplayName = "ToSenderMessage method does not map null cloud event attributes to sender message headers")]
        public void ToSenderMessageMethodHappyPath5()
        {
            // No optional attributes provided

            var cloudEvent = new CloudEvent
            {
                Id = "MyId",
                Source = new Uri("http://MySource"),
                Type = "MyType"
            };

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.Headers.Should().NotContainKey(CloudEvent.DataContentTypeAttribute);
            senderMessage.Headers.Should().NotContainKey(CloudEvent.DataSchemaAttribute);
            senderMessage.Headers[CloudEvent.SpecVersionAttribute].Should().Be("1.0");
            senderMessage.Headers.Should().NotContainKey(CloudEvent.SubjectAttribute);
        }

        [Fact(DisplayName = "ToSenderMessage method adds additional attributes to sender message headers")]
        public void ToSenderMessageMethodHappyPath6()
        {
            // Additional attributes provided

            var cloudEvent = new CloudEvent();
            cloudEvent.Id = "MyId";
            cloudEvent.Source = new Uri("http://mysource/");
            cloudEvent.Type = "MyType";
            cloudEvent.AdditionalAttributes.Add("foo", "abc");
            cloudEvent.AdditionalAttributes.Add("bar", 123);

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.Headers.Should().ContainKey("foo").WhichValue.Should().Be("abc");
            senderMessage.Headers.Should().ContainKey("bar").WhichValue.Should().Be(123);
        }

        [Fact(DisplayName = "ToSenderMessage method applies specified protocol binding to each attribute")]
        public void ToSenderMessageMethodHappyPath7()
        {
            // Non-default protocol binding

            var mockProtocolBinding = new Mock<IProtocolBinding>();
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>()))
                .Returns<string>(header => "test-" + header);

            var id = "MyId";

            var cloudEvent = new CloudEvent
            {
                Id = id,
                Source = new Uri("http://mysource/"),
                Type = "MyType",
                AdditionalAttributes = { { "foo", "abc" } }
            };

            var senderMessage = cloudEvent.ToSenderMessage(mockProtocolBinding.Object);

            senderMessage.Headers.Should().ContainKey("test-" + CloudEvent.IdAttribute).WhichValue.Should().BeSameAs(id);
            senderMessage.Headers.Should().ContainKey("test-" + CloudEvent.SpecVersionAttribute).WhichValue.Should().Be("1.0");

            // Note that AdditionalAttributes are mapped verbatim (i.e. not using the protocol binding).
            senderMessage.Headers.Should().ContainKey("foo").WhichValue.Should().Be("abc");
        }

        #endregion

        #region ValidateCore

        [Fact(DisplayName = "ValidateCore method does not throw when given valid sender message")]
        public void ValidateCoreMethodHappyPath1()
        {
            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => TestCloudEvent.Validate(senderMessage);

            act.Should().NotThrow();
        }

        [Fact(DisplayName = "ValidateCore method does not throw when given valid sender message with stringly typed attributes")]
        public void ValidateCoreMethodHappyPath2()
        {
            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, "http://MySource");
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow.ToString("O"));

            Action act = () => TestCloudEvent.Validate(senderMessage);

            act.Should().NotThrow();
        }

        [Fact(DisplayName = "ValidateCore method does not throw when given valid sender message for specified protocol binding")]
        public void ValidateCoreMethodHappyPath3()
        {
            // Non-default protocol binding

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add("test-" + CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add("test-" + CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add("test-" + CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add("test-" + CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add("test-" + CloudEvent.TimeAttribute, DateTime.UtcNow);

            var mockProtocolBinding = new Mock<IProtocolBinding>();
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>())).Returns<string>(header => "test-" + header);

            Action act = () => TestCloudEvent.Validate(senderMessage, mockProtocolBinding.Object);

            act.Should().NotThrow();
        }

        [Fact(DisplayName = "ValidateCore method throws given null senderMessage parameter")]
        public void ValidateCoreMethodSadPath1()
        {
            // Null senderMessage

            Action act = () => TestCloudEvent.Validate(null);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*senderMessage*");
        }

        [Fact(DisplayName = "ValidateCore method throws given missing SpecVersion header")]
        public void ValidateCoreMethodSadPath2()
        {
            // Invalid SpecVersion

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.SpecVersionAttribute, "0.0");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => TestCloudEvent.Validate(senderMessage);

            act.Should().ThrowExactly<CloudEventValidationException>();
        }

        [Fact(DisplayName = "ValidateCore method adds Id header if missing")]
        public void ValidateCoreMethodSadPath3()
        {
            // Missing Id

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add("test-" + CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add("test-" + CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add("test-" + CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add("test-" + CloudEvent.TimeAttribute, DateTime.UtcNow);

            var mockProtocolBinding = new Mock<IProtocolBinding>();
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>())).Returns<string>(header => "test-" + header);

            Action act = () => TestCloudEvent.Validate(senderMessage, mockProtocolBinding.Object);

            act.Should().NotThrow();

            senderMessage.Headers.Should().ContainKey("test-" + CloudEvent.IdAttribute).WhichValue.Should().NotBeNull();
        }

        [Fact(DisplayName = "ValidateCore method throws given missing Source header")]
        public void ValidateCoreMethodSadPath4()
        {
            // Missing Source

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => TestCloudEvent.Validate(senderMessage);

            act.Should().ThrowExactly<CloudEventValidationException>();
        }

        [Fact(DisplayName = "ValidateCore method throws given missing Type header")]
        public void ValidateCoreMethodSadPath5()
        {
            // Missing Type

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => TestCloudEvent.Validate(senderMessage);

            act.Should().ThrowExactly<CloudEventValidationException>();
        }

        [Fact(DisplayName = "ValidateCore method adds Time header if missing")]
        public void ValidateCoreMethodSadPath6()
        {
            // Missing Time

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add("test-" + CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add("test-" + CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add("test-" + CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add("test-" + CloudEvent.TypeAttribute, "MyType");

            var mockProtocolBinding = new Mock<IProtocolBinding>();
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>())).Returns<string>(header => "test-" + header);

            Action act = () => TestCloudEvent.Validate(senderMessage, mockProtocolBinding.Object);

            act.Should().NotThrow();

            senderMessage.Headers.Should().ContainKey("test-" + CloudEvent.TimeAttribute).WhichValue.Should().NotBeNull();
        }

        #endregion

        #region Implicit conversion operator

        [Fact(DisplayName = "Implicit conversion operator works by calling ToSenderMessage")]
        public void ImplicitConversionOperatorHappyPath1()
        {
            var mockCloudEvent = new Mock<CloudEvent>();
            mockCloudEvent.Setup(m => m.ToSenderMessage(It.IsAny<IProtocolBinding>())).CallBase();
            mockCloudEvent.Setup(m => m.Validate()).CallBase();
            mockCloudEvent.Object.SetData("Hello, world!");
            mockCloudEvent.Object.Id = "MyId";
            mockCloudEvent.Object.Source = new Uri("http://mysource/");
            mockCloudEvent.Object.Type = "test";
            mockCloudEvent.Object.AdditionalAttributes.Add("foo", "abc");

            SenderMessage senderMessage = mockCloudEvent.Object;

            senderMessage.StringPayload.Should().Be("Hello, world!");
            senderMessage.Headers.Should().ContainKey(CloudEvent.IdAttribute).WhichValue.Should().Be("MyId");
            senderMessage.Headers.Should().ContainKey(CloudEvent.SourceAttribute).WhichValue.ToString().Should().Be("http://mysource/");
            senderMessage.Headers.Should().ContainKey(CloudEvent.TypeAttribute).WhichValue.Should().Be("test");
            senderMessage.Headers.Should().ContainKey("foo").WhichValue.Should().Be("abc");

            mockCloudEvent.Verify(m => m.ToSenderMessage(CloudEvent.DefaultProtocolBinding), Times.Once());
        }

        [Fact(DisplayName = "Implicit conversion operator returns null given null cloud event")]
        public void ImplicitConversionOperatorHappyPath2()
        {
            CloudEvent cloudEvent = null;

            SenderMessage senderMessage = cloudEvent;

            senderMessage.Should().BeNull();
        }

        #endregion

        private class TestCloudEvent : CloudEvent
        {
            public static void Validate(SenderMessage senderMessage, IProtocolBinding protocolBinding = null) =>
                ValidateCore(senderMessage, protocolBinding);
        }
    }
}
