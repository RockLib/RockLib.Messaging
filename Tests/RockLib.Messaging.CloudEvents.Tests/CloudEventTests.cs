using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
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

            cloudEvent.StringData.Should().BeNull();
            cloudEvent.BinaryData.Should().BeNull();
            cloudEvent.Attributes.Should().BeEmpty();
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
                Source = "http://mysource/",
                Type = "MyType",
                DataContentType = "application/json; charset=utf-8",
                DataSchema = "http://mydataschema/",
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
            CloudEvent source = null!;

            Action act = () =>
            {
                var _ = new CloudEvent(source);
            };

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*source*");
        }

        [Fact(DisplayName = "Constructor 3 creates cloud event with binary data")]
        public void Constructor3HappyPath1()
        {
            var binaryData = new byte[] { 1, 2, 3, 4 };

            using var receiverMessage = new FakeReceiverMessage(binaryData);

            var cloudEvent = new CloudEvent(receiverMessage);

            cloudEvent.BinaryData.Should().BeSameAs(binaryData);
            cloudEvent.StringData.Should().BeNull();
        }

        [Fact(DisplayName = "Constructor 3 creates cloud event with string data")]
        public void Constructor3HappyPath2()
        {
            var stringData = "Hello, world!";

            using var receiverMessage = new FakeReceiverMessage(stringData);

            var cloudEvent = new CloudEvent(receiverMessage);

            cloudEvent.StringData.Should().BeSameAs(stringData);
            cloudEvent.BinaryData.Should().BeNull();
        }

        [Fact(DisplayName = "Constructor 3 maps cloud event attributes from receiver message headers")]
        public void Constructor3HappyPath3()
        {
            // All attributes provided

            using var receiverMessage = new FakeReceiverMessage("Hello, world!");

            var source = "http://MySource";
            var dataContentType = "application/mycontenttype";
            var dataSchema = "http://MySource";
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

            CloudEvent.SpecVersion.Should().Be("1.0");
            cloudEvent.Id.Should().Be("MyId");
            cloudEvent.Source.Should().BeSameAs(source);
            cloudEvent.Type.Should().Be("MyType");
            cloudEvent.DataContentType.Should().BeSameAs(dataContentType);
            cloudEvent.DataSchema.Should().BeSameAs(dataSchema);
            cloudEvent.Subject.Should().Be("MySubject");
            cloudEvent.Time.Should().Be(time);
        }

        [Fact(DisplayName = "Constructor 3 does not require any cloud event attributes to be mapped")]
        public void Constructor3HappyPath4()
        {
            // No attributes provided

            using var receiverMessage = new FakeReceiverMessage("Hello, world!");

            var cloudEvent = new CloudEvent(receiverMessage);

            CloudEvent.SpecVersion.Should().Be("1.0");
            cloudEvent.Source.Should().BeNull();
            cloudEvent.Type.Should().BeNull();
            cloudEvent.DataContentType.Should().BeNull();
            cloudEvent.DataSchema.Should().BeNull();
            cloudEvent.Subject.Should().BeNull();
            cloudEvent.Attributes.Should().BeEmpty();
        }

        [Fact(DisplayName = "Constructor 3 maps from stringly typed receiver message headers")]
        public void Constructor3HappyPath5()
        {
            // Alternate property types provided

            using var receiverMessage = new FakeReceiverMessage("Hello, world!");

            var source = new Uri("http://MySource").ToString();
            var dataContentType = new ContentType("application/mycontenttype").ToString();
            var dataSchema = new Uri("http://MySource").ToString();
            var time = DateTime.UtcNow.ToString("O");

            receiverMessage.Headers.Add(CloudEvent.SourceAttribute, source);
            receiverMessage.Headers.Add(CloudEvent.DataContentTypeAttribute, dataContentType);
            receiverMessage.Headers.Add(CloudEvent.DataSchemaAttribute, dataSchema);
            receiverMessage.Headers.Add(CloudEvent.TimeAttribute, time);

            var cloudEvent = new CloudEvent(receiverMessage);

            cloudEvent.Should().NotBeNull();
            cloudEvent.Source!.ToString().Should().Be(source);
            cloudEvent.DataContentType!.ToString().Should().Be(dataContentType);
            cloudEvent.DataSchema!.ToString().Should().Be(dataSchema);
            cloudEvent.Time.ToString("O").Should().Be(time);
        }

        [Fact(DisplayName = "Constructor 3 maps additional attributes")]
        public void Constructor3HappyPath6()
        {
            // Additional attributes provided

            using var receiverMessage = new FakeReceiverMessage("Hello, world!");
            receiverMessage.Headers.Add("test-foo", "abc");
            receiverMessage.Headers.Add("test-bar", 123);

            var mockProtocolBinding = new Mock<IProtocolBinding>().SetupTestProtocolBinding();

            var cloudEvent = new CloudEvent(receiverMessage, mockProtocolBinding.Object);

            cloudEvent.Attributes.Should().HaveCount(2);
            cloudEvent.Attributes.Should().ContainKey("foo").WhoseValue.Should().Be("abc");
            cloudEvent.Attributes.Should().ContainKey("bar").WhoseValue.Should().Be(123);
        }

        [Fact(DisplayName = "Constructor 3 maps with the specified protocol binding")]
        public void Constructor3HappyPath7()
        {
            // Non-default protocol binding

            using var receiverMessage = new FakeReceiverMessage("Hello, world!");
            receiverMessage.Headers.Add("foo", "abc");
            receiverMessage.Headers.Add("test-" + CloudEvent.IdAttribute, "MyId");

            var mockProtocolBinding = new Mock<IProtocolBinding>().SetupTestProtocolBinding();

            var cloudEvent = new CloudEvent(receiverMessage, mockProtocolBinding.Object);

            cloudEvent.Id.Should().Be("MyId");
            cloudEvent.Headers.Should().ContainKey("foo").WhoseValue.Should().Be("abc");
        }

        [Fact(DisplayName = "Constructor 3 throws when receiverMessage parameter is null")]
        public void Constructor3SadPath1()
        {
            // Null receiverMessage

            Action act = () =>
            {
                var _ = new CloudEvent((IReceiverMessage)null!);
            };

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*receiverMessage*");
        }

        [Fact(DisplayName = "Constructor 3 throws when specversion header is not '1.0'")]
        public void Constructor3SadPath2()
        {
            // Invalid specversion

            using var receiverMessage = new FakeReceiverMessage("Hello, world!");
            receiverMessage.Headers.Add(CloudEvent.SpecVersionAttribute, "0.0");

            Action act = () =>
            {
                var _ = new CloudEvent(receiverMessage);
            };

            act.Should().ThrowExactly<CloudEventValidationException>();
        }

        [Fact(DisplayName = "Constructor 4 maps 'data_base64' attribute to BinaryData")]
        public void Constructor4HappyPath1()
        {
            var data = new byte[] { 1, 2, 3, 4 };
            var json = $"{{\"data_base64\":\"{Convert.ToBase64String(data)}\"}}";

            var cloudEvent = new CloudEvent(json);

            cloudEvent.BinaryData.Should().BeEquivalentTo(data);
        }

        [Fact(DisplayName = "Constructor 4 handles null 'data_base64' attribute")]
        public void Constructor4HappyPath2()
        {
            var json = "{\"data_base64\":null}";

            var cloudEvent = new CloudEvent(json);

            cloudEvent.BinaryData.Should().BeNull();
        }

        [Fact(DisplayName = "Constructor 4 maps unformatted 'data' attribute to StringData")]
        public void Constructor4HappyPath3()
        {
            var data = "Hello, world!";
            var json = $"{{\"data\":\"{data}\"}}";

            var cloudEvent = new CloudEvent(json);

            cloudEvent.StringData.Should().Be(data);
        }

        [Fact(DisplayName = "Constructor 4 maps JSON 'data' attribute to StringData")]
        public void Constructor4HappyPath4()
        {
            var data = "{\"foo\":\"abc\",\"bar\":123.45,\"baz\":true}";
            var json = $"{{\"data\":{data}}}";

            var cloudEvent = new CloudEvent(json);

            cloudEvent.StringData.Should().Be(data);
        }

        [Fact(DisplayName = "Constructor 4 maps DateTime 'data' attribute to StringData")]
        public void Constructor4HappyPath5()
        {
            var data = DateTime.UtcNow.ToString("O");
            var json = $"{{\"data\":\"{data}\"}}";

            var cloudEvent = new CloudEvent(json);

            cloudEvent.StringData.Should().Be(data);
        }

        [Fact(DisplayName = "Constructor 4 maps bool 'data' attribute to StringData")]
        public void Constructor4HappyPath6()
        {
            var data = "true";
            var json = $"{{\"data\":{data}}}";

            var cloudEvent = new CloudEvent(json);

            cloudEvent.StringData.Should().Be(data);
        }

        [Fact(DisplayName = "Constructor 4 maps numeric 'data' attribute to StringData")]
        public void Constructor4HappyPath7()
        {
            var data = "123.45";
            var json = $"{{\"data\":{data}}}";

            var cloudEvent = new CloudEvent(json);

            cloudEvent.StringData.Should().Be(data);
        }

        [Fact(DisplayName = "Constructor 4 handles null 'data' attribute")]
        public void Constructor4HappyPath8()
        {
            var json = "{\"data\":null}";

            var cloudEvent = new CloudEvent(json);

            cloudEvent.StringData.Should().BeNull();
        }

        [Fact(DisplayName = "Constructor 4 maps attributes")]
        public void Constructor4HappyPath9()
        {
            var json = "{\"type\":\"MyType\",\"source\":\"/MySource\"}";

            var cloudEvent = new CloudEvent(json);

            cloudEvent.Type.Should().Be("MyType");
            cloudEvent.Source.Should().Be("/MySource");
        }

        [Fact(DisplayName = "Constructor 4 throws when jsonFormattedCloudEvent parameter is null")]
        public void Constructor4SadPath1()
        {
            // Null jsonFormattedCloudEvent

            Action act = () =>
            {
                var _ = new CloudEvent((string)null!);
            };

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*jsonFormattedCloudEvent*");
        }

        [Fact(DisplayName = "Constructor 4 throws when jsonFormattedCloudEvent parameter is empty")]
        public void Constructor4SadPath2()
        {
            // Empty jsonFormattedCloudEvent

            Action act = () =>
            {
                var _ = new CloudEvent("");
            };

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*jsonFormattedCloudEvent*");
        }

        [Fact(DisplayName = "Constructor 4 throws when specversion header is not '1.0'")]
        public void Constructor4SadPath3()
        {
            // Invalid specversion

            var json = "{\"specversion\":\"0.0\",\"data\":\"abc\"}";

            Action act = () =>
            {
                var _ = new CloudEvent(json);
            };

            act.Should().ThrowExactly<CloudEventValidationException>().WithMessage("Invalid 'specversion' attribute*");
        }

        [Theory(DisplayName = "Constructor 4 throws when data_base64 value is not a string")]
        [InlineData("123.45")]
        [InlineData("{\"foo\":123}")]
        [InlineData("[\"abc\"]")]
        public void Constructor4SadPath4(string dataBase64)
        {
            // Invalid data_base64

            var json = $"{{\"data_base64\":{dataBase64}}}";

            Action act = () =>
            {
                var _ = new CloudEvent(json);
            };

            act.Should().ThrowExactly<CloudEventValidationException>().WithMessage("'data_base64' must have a string value.");
        }

        [Fact(DisplayName = "Constructor 4 throws when data_base64 value is not a valid base-64 encoded binary value")]
        public void Constructor4SadPath5()
        {
            // Invalid base-64 string for data_base64

            var json = "{\"data_base64\":\"Hello, world!\"}";

            Action act = () =>
            {
                var _ = new CloudEvent(json);
            };

            act.Should().ThrowExactly<CloudEventValidationException>().WithMessage("'data_base64' must have a valid base-64 encoded binary value.");
        }

        [Theory(DisplayName = "Constructor 4 throws when data and data_base64 are both provided")]
        [InlineData("{\"foo\":123}")]
        [InlineData("[123,null,{}]")]
        [InlineData("\"Hello, world!\"")]
        [InlineData("123.45")]
        public void Constructor4SadPath6(string data)
        {
            // data and data_base64 both provided

            var binaryData = new byte[] { 1, 2, 3, 4 };
            var json = $"{{\"data_base64\":\"{Convert.ToBase64String(binaryData)}\",\"data\":{data}}}";

            Action act = () =>
            {
                var _ = new CloudEvent(json);
            };

            act.Should().ThrowExactly<CloudEventValidationException>().WithMessage("'data_base64' and 'data' cannot both have values.");
        }

        [Theory(DisplayName = "Constructor 4 throws when attribute value is not a string")]
        [InlineData("{\"foo\":123}")]
        [InlineData("[\"abc\"]")]
        public void Constructor4SadPath7(string attributeValue)
        {
            // Attribute value is not a string.

            var json = $"{{\"customattribute\":{attributeValue}}}";

            Action act = () =>
            {
                var _ = new CloudEvent(json);
            };

            act.Should().ThrowExactly<CloudEventValidationException>().WithMessage("Invalid value for 'customattribute' member*");
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

            cloudEvent.Invoking(evt => evt.Id = null!).Should()
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

        [Fact(DisplayName = "StringData returns the string data passed to SetData")]
        public void StringDataPropertyHappyPath1()
        {
            var cloudEvent = new CloudEvent();

            var stringData = "Hello, world!";

            cloudEvent.SetData(stringData);

            cloudEvent.StringData.Should().Be(stringData);
        }

        [Fact(DisplayName = "StringData returns null if data is binary")]
        public void StringDataPropertyHappyPath2()
        {
            var cloudEvent = new CloudEvent();

            var binaryData = new byte[] { 1, 2, 3, 4 };

            cloudEvent.SetData(binaryData);

            cloudEvent.StringData.Should().BeNull();
        }

        [Fact(DisplayName = "StringData returns null if uninitialized")]
        public void StringDataPropertyHappyPath3()
        {
            var cloudEvent = new CloudEvent();

            cloudEvent.StringData.Should().BeNull();
        }

        [Fact(DisplayName = "BinaryData returns the binary data passed to SetData")]
        public void BinaryDataPropertyHappyPath1()
        {
            var cloudEvent = new CloudEvent();

            var binaryData = new byte[] { 1, 2, 3, 4 };

            cloudEvent.SetData(binaryData);

            cloudEvent.BinaryData.Should().BeEquivalentTo(binaryData);
        }

        [Fact(DisplayName = "BinaryData returns null if the data is string")]
        public void BinaryDataPropertyHappyPath2()
        {
            var cloudEvent = new CloudEvent();

            var stringData = "Hello, world!";

            cloudEvent.SetData(stringData);

            cloudEvent.BinaryData.Should().BeNull();
        }

        [Fact(DisplayName = "BinaryData returns null if uninitialized")]
        public void BinaryDataPropertyHappyPath3()
        {
            var cloudEvent = new CloudEvent();

            cloudEvent.BinaryData.Should().BeNull();
        }

        #endregion

        #region ToJson

        [Fact(DisplayName = "ToJson method maps Attributes to JSON fields")]
        public void ToJsonMethodHappyPath1()
        {
            var cloudEvent = new CloudEvent
            {
                Id = "MyId",
                Source = "/MySource",
                Attributes = { ["CustomAttribute"] = "MyCustomAttribute" }
            };
        }

        [Fact(DisplayName = "ToJson method does not map Headers to JSON fields")]
        public void ToJsonMethodHappyPath2()
        {
            var cloudEvent = new CloudEvent
            {
                Type = "MyType",
                Source = "/MySource",
                Attributes = { ["customattribute"] = "MyCustomAttribute" },
                Headers = { ["customheader"] = "MyCustomHeader" }
            };

            var jsonString = cloudEvent.ToJson();

            dynamic json = JObject.Parse(jsonString);

            string specversion = json.specversion;
            string id = json.id;
            string type = json.type;
            string source = json.source;
            string time = json.time;
            string customAttribute = json.customattribute;

            specversion.Should().Be("1.0");
            id.Should().NotBeNullOrEmpty();
            type.Should().Be("MyType");
            source.Should().Be("/MySource");
            customAttribute.Should().Be("MyCustomAttribute");
            time.Should().NotBeNullOrEmpty();

            ((JObject)json).Should().NotContainKey("customheader");
        }

        [Fact(DisplayName = "ToJson method maps binary data to JSON 'data_base64' field")]
        public void ToJsonMethodHappyPath3()
        {
            var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            var cloudEvent = new CloudEvent
            {
                Type = "MyType",
                Source = "/MySource"
            }.SetData(data);

            var jsonString = cloudEvent.ToJson();

            dynamic json = JObject.Parse(jsonString);

            string data_base64 = json.data_base64;

            data_base64.Should().Be(Convert.ToBase64String(data));
        }

        [Fact(DisplayName = "ToJson method maps json string data to JSON 'data' field")]
        public void ToJsonMethodHappyPath4()
        {
            var jsonData = "{\"foo\":\"abc\",\"bar\":123.45,\"baz\":true}";

            var cloudEvent = new CloudEvent
            {
                Type = "MyType",
                Source = "/MySource"
            }.SetData(jsonData);

            var jsonString = cloudEvent.ToJson();

            dynamic json = JObject.Parse(jsonString);

            string foo = json.data.foo;
            double bar = json.data.bar;
            bool baz = json.data.baz;

            foo.Should().Be("abc");
            bar.Should().Be(123.45);
            baz.Should().BeTrue();
        }

        [Fact(DisplayName = "ToJson method maps unformatted string data to JSON 'data' field")]
        public void ToJsonMethodHappyPath5()
        {
            var unformattedData = "Hello, world!";

            var cloudEvent = new CloudEvent
            {
                Type = "MyType",
                Source = "/MySource"
            }.SetData(unformattedData);

            var jsonString = cloudEvent.ToJson();

            dynamic json = JObject.Parse(jsonString);

            string data = json.data;

            data.Should().Be(unformattedData);
        }

        [Fact(DisplayName = "ToJson method does not map null data")]
        public void ToJsonMethodHappyPath6()
        {
            var cloudEvent = new CloudEvent
            {
                Type = "MyType",
                Source = "/MySource"
            };

            var jsonString = cloudEvent.ToJson();

            var json = JObject.Parse(jsonString);

            json.Should().NotContainKey("data");
        }

        [Theory(DisplayName = "ToJson method indent parameter works as expected")]
        [InlineData(true)]
        [InlineData(false)]
        public void ToJsonMethodHappyPath7(bool indent)
        {
            var cloudEvent = new CloudEvent
            {
                Type = "MyType",
                Source = "/MySource"
            };

            var jsonString = cloudEvent.ToJson(indent);

            if (indent)
            {
                jsonString.Should().Contain("\n");
            }
            else
            {
                jsonString.Should().NotContain("\n");
            }

        }

        #endregion

        #region ToSenderMessage

        [Fact(DisplayName = "ToSenderMessage method Binary Mode maps string data to StringPayload")]
        public void ToSenderMessageMethodBinaryModeHappyPath1()
        {
            var stringData = "Hello, world!";

            var cloudEvent = new CloudEvent
            {
                Id = "MyId",
                Source = "http://mysource/",
                Type = "MyType"
            }.SetData(stringData);

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.StringPayload.Should().BeSameAs(stringData);
        }

        [Fact(DisplayName = "ToSenderMessage method Binary Mode maps binary data to BinaryPayload")]
        public void ToSenderMessageMethodBinaryModeHappyPath2()
        {
            var binaryData = new byte[] { 1, 2, 3, 4 };

            var cloudEvent = new CloudEvent
            {
                Id = "MyId",
                Source = "http://mysource/",
                Type = "MyType"
            }.SetData(binaryData);

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.BinaryPayload.Should().BeSameAs(binaryData);
        }

        [Fact(DisplayName = "ToSenderMessage method Binary Mode maps null data to empty StringPayload")]
        public void ToSenderMessageMethodBinaryModeHappyPath3()
        {
            // null Data

            var cloudEvent = new CloudEvent
            {
                Id = "MyId",
                Source = "http://mysource/",
                Type = "MyType"
            };

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.StringPayload.Should().Be("");
        }

        [Fact(DisplayName = "ToSenderMessage method Binary Mode maps cloud event attributes to sender message headers")]
        public void ToSenderMessageMethoBinaryModedHappyPath4()
        {
            // All attributes provided

            var dataContentType = "application/xml";
            var dataSchema = "http://dataschema";
            var id = "MyId";
            var source = "http://source";
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

        [Fact(DisplayName = "ToSenderMessage method Binary Mode does not map null cloud event attributes to sender message headers")]
        public void ToSenderMessageMethodBinaryModeHappyPath5()
        {
            // No optional attributes provided

            var cloudEvent = new CloudEvent
            {
                Id = "MyId",
                Source = "http://MySource",
                Type = "MyType"
            };

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.Headers.Should().NotContainKey(CloudEvent.DataContentTypeAttribute);
            senderMessage.Headers.Should().NotContainKey(CloudEvent.DataSchemaAttribute);
            senderMessage.Headers[CloudEvent.SpecVersionAttribute].Should().Be("1.0");
            senderMessage.Headers.Should().NotContainKey(CloudEvent.SubjectAttribute);
        }

        [Fact(DisplayName = "ToSenderMessage method Binary Mode adds additional attributes to sender message headers")]
        public void ToSenderMessageMethodBinaryModeHappyPath6()
        {
            // Additional attributes provided

            var cloudEvent = new CloudEvent();
            cloudEvent.Id = "MyId";
            cloudEvent.Source = "http://mysource/";
            cloudEvent.Type = "MyType";
            cloudEvent.Attributes.Add("foo", "abc");
            cloudEvent.Attributes.Add("bar", 123);

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.Headers.Should().ContainKey("foo").WhoseValue.Should().Be("abc");
            senderMessage.Headers.Should().ContainKey("bar").WhoseValue.Should().Be(123);
        }

        [Fact(DisplayName = "ToSenderMessage method Binary Mode applies specified protocol binding to each attribute")]
        public void ToSenderMessageMethodBinaryModeHappyPath7()
        {
            // Non-default protocol binding

            var mockProtocolBinding = new Mock<IProtocolBinding>();
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>()))
                .Returns<string>(header => "test-" + header);

            var id = "MyId";

            var cloudEvent = new CloudEvent
            {
                Id = id,
                Source = "http://mysource/",
                Type = "MyType",
                Attributes = { { "foo", "abc" } },
                ProtocolBinding = mockProtocolBinding.Object
            };

            var senderMessage = cloudEvent.ToSenderMessage();

            senderMessage.Headers.Should().ContainKey("test-" + CloudEvent.IdAttribute).WhoseValue.Should().BeSameAs(id);
            senderMessage.Headers.Should().ContainKey("test-" + CloudEvent.SpecVersionAttribute).WhoseValue.Should().Be("1.0");
            senderMessage.Headers.Should().ContainKey("test-foo").WhoseValue.Should().Be("abc");
        }

        [Fact(DisplayName = "ToSenderMessage method Structured Mode renders correctly")]
        public void ToSenderMessageMethodStructuredModeHappyPath()
        {
            var stringData = "Hello, world!";

            var cloudEvent = new CloudEvent
            {
                Id = "MyId",
                Source = "http://mysource/",
                Type = "MyType",
                DataContentType = "test/text",
                Headers = { [HeaderNames.MessageId] = "MyCoreInternalId" }
            }.SetData(stringData);

            var senderMessage = cloudEvent.ToSenderMessage(structuredMode: true);

            senderMessage.Headers.Should().ContainKey(CloudEvent.StructuredModeContentTypeHeader)
                .WhoseValue.Should().Be(CloudEvent.StructuredModeJsonMediaType);
            senderMessage.Headers.Should().ContainKey(HeaderNames.MessageId)
                .WhoseValue.Should().Be("MyCoreInternalId");

            dynamic json = JObject.Parse(senderMessage.StringPayload);

            string id = json.id;
            string source = json.source;
            string type = json.type;
            string dataContentType = json.datacontenttype;
            string data = json.data;

            id.Should().Be(cloudEvent.Id);
            source.Should().Be(cloudEvent.Source);
            type.Should().Be(cloudEvent.Type);
            dataContentType.Should().Be(cloudEvent.DataContentType);
            data.Should().Be(stringData);
        }

        #endregion

        #region Validate

        [Fact(DisplayName = "Validate method does not throw when given valid sender message")]
        public void ValidateMethodHappyPath1()
        {
            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => CloudEvent.Validate(senderMessage);

            act.Should().NotThrow();
        }

        [Fact(DisplayName = "Validate method does not throw when given valid sender message with stringly typed attributes")]
        public void ValidateMethodHappyPath2()
        {
            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, "http://MySource");
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow.ToString("O"));

            Action act = () => CloudEvent.Validate(senderMessage);

            act.Should().NotThrow();
        }

        [Fact(DisplayName = "Validate method does not throw when given valid sender message for specified protocol binding")]
        public void ValidateMethodHappyPath3()
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

            Action act = () => CloudEvent.Validate(senderMessage, mockProtocolBinding.Object);

            act.Should().NotThrow();
        }

        [Fact(DisplayName = "Validate method throws given null senderMessage parameter")]
        public void ValidateMethodSadPath1()
        {
            // Null senderMessage

            Action act = () => CloudEvent.Validate(null!);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*senderMessage*");
        }

        [Fact(DisplayName = "Validate method throws given missing SpecVersion header")]
        public void ValidateMethodSadPath2()
        {
            // Invalid SpecVersion

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.SpecVersionAttribute, "0.0");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => CloudEvent.Validate(senderMessage);

            act.Should().ThrowExactly<CloudEventValidationException>();
        }

        [Fact(DisplayName = "Validate method adds Id header if missing")]
        public void ValidateMethodSadPath3()
        {
            // Missing Id

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add("test-" + CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add("test-" + CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add("test-" + CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add("test-" + CloudEvent.TimeAttribute, DateTime.UtcNow);

            var mockProtocolBinding = new Mock<IProtocolBinding>();
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>())).Returns<string>(header => "test-" + header);

            Action act = () => CloudEvent.Validate(senderMessage, mockProtocolBinding.Object);

            act.Should().NotThrow();

            senderMessage.Headers.Should().ContainKey("test-" + CloudEvent.IdAttribute).WhoseValue.Should().NotBeNull();
        }

        [Fact(DisplayName = "Validate method throws given missing Source header")]
        public void ValidateMethodSadPath4()
        {
            // Missing Source

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.TypeAttribute, "MyType");
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => CloudEvent.Validate(senderMessage);

            act.Should().ThrowExactly<CloudEventValidationException>();
        }

        [Fact(DisplayName = "Validate method throws given missing Type header")]
        public void ValidateMethodSadPath5()
        {
            // Missing Type

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add(CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add(CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add(CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add(CloudEvent.TimeAttribute, DateTime.UtcNow);

            Action act = () => CloudEvent.Validate(senderMessage);

            act.Should().ThrowExactly<CloudEventValidationException>();
        }

        [Fact(DisplayName = "Validate method adds Time header if missing")]
        public void ValidateMethodSadPath6()
        {
            // Missing Time

            var senderMessage = new SenderMessage("Hello, world!");

            senderMessage.Headers.Add("test-" + CloudEvent.SpecVersionAttribute, "1.0");
            senderMessage.Headers.Add("test-" + CloudEvent.IdAttribute, "MyId");
            senderMessage.Headers.Add("test-" + CloudEvent.SourceAttribute, new Uri("http://MySource"));
            senderMessage.Headers.Add("test-" + CloudEvent.TypeAttribute, "MyType");

            var mockProtocolBinding = new Mock<IProtocolBinding>();
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>())).Returns<string>(header => "test-" + header);

            Action act = () => CloudEvent.Validate(senderMessage, mockProtocolBinding.Object);

            act.Should().NotThrow();

            senderMessage.Headers.Should().ContainKey("test-" + CloudEvent.TimeAttribute).WhoseValue.Should().NotBeNull();
        }

        #endregion

        #region Implicit conversion operator

        [Fact(DisplayName = "Implicit conversion operator works by calling ToSenderMessage")]
        public void ImplicitConversionOperatorHappyPath1()
        {
            var mockCloudEvent = new Mock<CloudEvent>();
            mockCloudEvent.Setup(m => m.ToSenderMessage(It.IsAny<bool>())).CallBase();
            mockCloudEvent.Setup(m => m.Validate()).CallBase();
            mockCloudEvent.Object.SetData("Hello, world!");
            mockCloudEvent.Object.Id = "MyId";
            mockCloudEvent.Object.Source = "http://mysource/";
            mockCloudEvent.Object.Type = "test";
            mockCloudEvent.Object.Attributes.Add("foo", "abc");

            SenderMessage senderMessage = mockCloudEvent.Object;

            senderMessage.StringPayload.Should().Be("Hello, world!");
            senderMessage.Headers.Should().ContainKey(CloudEvent.IdAttribute).WhoseValue.Should().Be("MyId");
            senderMessage.Headers.Should().ContainKey(CloudEvent.SourceAttribute).WhoseValue.ToString().Should().Be("http://mysource/");
            senderMessage.Headers.Should().ContainKey(CloudEvent.TypeAttribute).WhoseValue.Should().Be("test");
            senderMessage.Headers.Should().ContainKey("foo").WhoseValue.Should().Be("abc");

            mockCloudEvent.Verify(m => m.ToSenderMessage(It.IsAny<bool>()), Times.Once());
        }

        [Fact(DisplayName = "Implicit conversion operator returns null given null cloud event")]
        public void ImplicitConversionOperatorHappyPath2()
        {
            CloudEvent cloudEvent = null!;

            SenderMessage senderMessage = cloudEvent;

            senderMessage.Should().BeNull();
        }

        #endregion
    }
}
