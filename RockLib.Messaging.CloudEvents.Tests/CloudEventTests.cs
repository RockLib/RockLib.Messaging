using FluentAssertions;
using Moq;
using RockLib.Messaging.Testing;
using System;
using System.Net.Mime;
using System.Text;
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

            cloudEvent.StringData.Should().BeNull();
            cloudEvent.BinaryData.Should().BeNull();
            cloudEvent.AdditionalAttributes.Should().BeEmpty();
            cloudEvent.DataContentType.Should().BeNull();
            cloudEvent.DataSchema.Should().BeNull();
            cloudEvent.Source.Should().BeNull();
            cloudEvent.Subject.Should().BeNull();
            cloudEvent.Type.Should().BeNull();
        }

        [Fact(DisplayName = "Constructor 2 sets expected properties")]
        public void Constructor2HappyPath()
        {
            var source = new CloudEvent
            {
                Id = "MyId",
                Time = new DateTime(2020, 7, 9, 22, 21, 37, DateTimeKind.Local),
                Source = new Uri("http://mysource/"),
                Type = "MyType",
                DataContentType = new ContentType("application/json; charset=utf-8"),
                DataSchema = new Uri("http://mydataschema/"),
                Subject = "MySubject"
            };

            var cloudEvent = new CloudEvent(source);

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
            CloudEvent source = null;

            Action act = () => new CloudEvent(source);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*source*");
        }

        [Fact(DisplayName = "Constructor 3 creates cloud event with binary data")]
        public void Constructor3HappyPath1()
        {
            var binaryData = new byte[] { 1, 2, 3, 4 };

            var receiverMessage = new FakeReceiverMessage(binaryData);

            var cloudEvent = new CloudEvent(receiverMessage);

            cloudEvent.BinaryData.Should().BeSameAs(binaryData);
            cloudEvent.StringData.Should().Be(Convert.ToBase64String(binaryData));
        }

        [Fact(DisplayName = "Constructor 3 creates cloud event with string data")]
        public void Constructor3HappyPath2()
        {
            var stringData = "Hello, world!";

            var receiverMessage = new FakeReceiverMessage(stringData);

            var cloudEvent = new CloudEvent(receiverMessage);

            cloudEvent.StringData.Should().BeSameAs(stringData);
            cloudEvent.BinaryData.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(stringData));
        }

        [Fact(DisplayName = "Constructor 3 maps cloud event attributes from receiver message headers")]
        public void Constructor3HappyPath3()
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

        [Fact(DisplayName = "Constructor 3 does not require any cloud event attributes to be mapped")]
        public void Constructor3HappyPath4()
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

        [Fact(DisplayName = "Constructor 3 maps from stringly typed receiver message headers")]
        public void Constructor3HappyPath5()
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

        [Fact(DisplayName = "Constructor 3 maps additional attributes verbatim")]
        public void Constructor3HappyPath6()
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


        [Fact(DisplayName = "Constructor 3 maps with the specified protocol binding")]
        public void Constructor3HappyPath7()
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

        [Fact(DisplayName = "Constructor 3 throws when receiverMessage parameter is null")]
        public void Constructor3SadPath1()
        {
            // Null receiverMessage

            Action act = () => new CloudEvent((IReceiverMessage)null);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*receiverMessage*");
        }

        [Fact(DisplayName = "Constructor 3 throws when specversion header is not '1.0'")]
        public void Constructor3SadPath2()
        {
            // Invalid specversion

            var receiverMessage = new FakeReceiverMessage("Hello, world!");
            receiverMessage.Headers.Add(CloudEvent.SpecVersionAttribute, "0.0");

            Action act = () => new CloudEvent(receiverMessage);

            act.Should().ThrowExactly<CloudEventValidationException>();
        }

        #endregion

        #region Properties

        [Fact(DisplayName = "Id property setter and getter work as expected")]
        public void IdPropertyHappyPath1()
        {
            var cloudEvent = new CloudEvent();

            cloudEvent.Id = "123";

            cloudEvent.Id.Should().Be("123");
        }

        [Fact(DisplayName = "Id property getter returns new GUID if setter has not been called")]
        public void IdPropertyHappyPath2()
        {
            var cloudEvent = new CloudEvent();

            cloudEvent.Id.Should().NotBeNullOrEmpty();
            Guid.TryParse(cloudEvent.Id, out _).Should().BeTrue();
        }

        [Fact(DisplayName = "Id property setter throws if value is null")]
        public void IdPropertySadPath()
        {
            var cloudEvent = new CloudEvent();

            cloudEvent.Invoking(evt => evt.Id = null).Should()
                .ThrowExactly<ArgumentNullException>()
                .WithMessage("*value*");
        }

        [Fact(DisplayName = "Time property setter and getter work as expected")]
        public void TimePropertyHappyPath1()
        {
            var cloudEvent = new CloudEvent();

            var time = new DateTime(2020, 7, 9, 22, 21, 37, DateTimeKind.Local);

            cloudEvent.Time = time;

            cloudEvent.Time.Should().Be(time);
        }

        [Fact(DisplayName = "Time property getter returns UtcNow if setter has not been called")]
        public void TimePropertyHappyPath2()
        {
            var cloudEvent = new CloudEvent();

            var before = DateTime.UtcNow;
            var time = cloudEvent.Time;
            var after = DateTime.UtcNow;

            time.Should().BeOnOrAfter(before);
            time.Should().BeOnOrBefore(after);
        }

        [Fact(DisplayName = "StringData property setter and getter work as expected")]
        public void DataPropertyHappyPath1()
        {
            var cloudEvent = new CloudEvent();

            var stringData = "Hello, world!";

            cloudEvent.StringData = stringData;

            cloudEvent.StringData.Should().Be(stringData);
            cloudEvent.BinaryData.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(stringData));
        }

        [Fact(DisplayName = "BinaryData property setter and getter work as expected")]
        public void DataPropertyHappyPath2()
        {
            var cloudEvent = new CloudEvent();

            var binaryData = new byte[] { 1, 2, 3, 4 };

            cloudEvent.BinaryData = binaryData;

            cloudEvent.BinaryData.Should().BeEquivalentTo(binaryData);
            cloudEvent.StringData.Should().Be(Convert.ToBase64String(binaryData));
        }

        #endregion

        #region ToSenderMessage

        [Fact(DisplayName = "ToSenderMessage method maps string data to StringPayload")]
        public void ToSenderMessageMethodHappyPath1()
        {
            var stringData = "Hello, world!";

            var cloudEvent = new CloudEvent
            {
                StringData = stringData,
                Id = "MyId",
                Source = new Uri("http://mysource/"),
                Type = "MyType"
            };

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.StringPayload.Should().BeSameAs(stringData);
        }

        [Fact(DisplayName = "ToSenderMessage method maps binary data to BinaryPayload")]
        public void ToSenderMessageMethodHappyPath2()
        {
            var binaryData = new byte[] { 1, 2, 3, 4 };

            var cloudEvent = new CloudEvent
            {
                BinaryData = binaryData,
                Id = "MyId",
                Source = new Uri("http://mysource/"),
                Type = "MyType"
            };

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
                AdditionalAttributes = { { "foo", "abc" } },
                ProtocolBinding = mockProtocolBinding.Object
            };

            var senderMessage = cloudEvent.ToSenderMessage();

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
            mockCloudEvent.Setup(m => m.ToSenderMessage()).CallBase();
            mockCloudEvent.Setup(m => m.Validate()).CallBase();
            mockCloudEvent.Object.StringData = "Hello, world!";
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

            mockCloudEvent.Verify(m => m.ToSenderMessage(), Times.Once());
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
