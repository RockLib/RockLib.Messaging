using FluentAssertions;
using Newtonsoft.Json;
using RockLib.Dynamic;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;
using Xunit;

namespace RockLib.Messaging.CloudEvents.Tests
{
    public class CloudEventExtensionsTests
    {
        private static readonly ConditionalWeakTable<CloudEvent, object> _dataObjects = typeof(CloudEventExtensions).Unlock()._dataObjects;

        [Fact(DisplayName = "SetData method 1 sets the data field and clears the data object")]
        public void SetDataMethod1HappyPath1()
        {
            var cloudEvent = new CloudEvent();

            // In order to verify that the method cleared the data object, there needs to be one first.
            _dataObjects.Add(cloudEvent, new object());

            var stringData = "Hello, world!";

            cloudEvent.SetData(stringData);

            string data = cloudEvent.Unlock()._data;

            data.Should().Be(stringData);
            _dataObjects.TryGetValue(cloudEvent, out _).Should().BeFalse();
        }

        [Fact(DisplayName = "SetData method 1 does nothing if new value isn't different")]
        public void SetDataMethod1HappyPath2()
        {
            var cloudEvent = new CloudEvent();

            // In order to verify that _data has not changed, capture its initial value.
            cloudEvent.Unlock()._data = "Hello, world!";

            // In order to verify that the method did not clear the data object, there needs to be one first.
            _dataObjects.Add(cloudEvent, new object());

            cloudEvent.SetData($"Hello, {"WORLD".ToLowerInvariant()}!");

            string data = cloudEvent.Unlock()._data;

            data.Should().Be("Hello, world!");
            _dataObjects.TryGetValue(cloudEvent, out _).Should().BeTrue();
        }

        [Fact(DisplayName = "SetData method 1 throws if cloudEvent is null")]
        public void SetDataMethod1SadPath()
        {
            CloudEvent cloudEvent = null!;

            Action act = () => cloudEvent.SetData("Hello, world!");

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*cloudEvent*");
        }

        [Fact(DisplayName = "SetData method 2 sets the data field and clears the data object")]
        public void SetDataMethod2HappyPath1()
        {
            var cloudEvent = new CloudEvent();

            // In order to verify that the method cleared the data object, there needs to be one first.
            _dataObjects.Add(cloudEvent, new object());

            var binaryData = new byte[] { 1, 2, 3, 4 };

            cloudEvent.SetData(binaryData);

            cloudEvent.BinaryData.Should().BeSameAs(binaryData);
            _dataObjects.TryGetValue(cloudEvent, out _).Should().BeFalse();
        }

        [Fact(DisplayName = "SetData method 2 does nothing if new value is same instance")]
        public void SetDataMethod2HappyPath2()
        {
            var cloudEvent = new CloudEvent();

            var binaryData = new byte[] { 1, 2, 3, 4 };

            cloudEvent.Unlock()._data = binaryData;

            // In order to verify that the method did not clear the data object, there needs to be one first.
            _dataObjects.Add(cloudEvent, new object());

            cloudEvent.SetData(binaryData);

            byte[] data = cloudEvent.Unlock()._data;

            data.Should().BeSameAs(binaryData);
            _dataObjects.TryGetValue(cloudEvent, out _).Should().BeTrue();
        }

        [Fact(DisplayName = "SetData method 2 does nothing if new value is equivalent")]
        public void SetDataMethod2HappyPath3()
        {
            var cloudEvent = new CloudEvent();

            var binaryData = new byte[] { 1, 2, 3, 4 };

            // In order to verify that _data has not changed, capture its initial value.
            cloudEvent.Unlock()._data = binaryData;

            // In order to verify that the method did not clear the data object, there needs to be one first.
            _dataObjects.Add(cloudEvent, new object());

            cloudEvent.SetData(new byte[] { 1, 2, 3, 4 });

            byte[] data = cloudEvent.Unlock()._data;

            data.Should().BeSameAs(binaryData);
            _dataObjects.TryGetValue(cloudEvent, out _).Should().BeTrue();
        }

        [Fact(DisplayName = "SetData method 2 throws if cloudEvent is null")]
        public void SetDataMethod2SadPath()
        {
            CloudEvent cloudEvent = null!;

            Action act = () => cloudEvent.SetData(new byte[] { 1, 2, 3, 4 });

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*cloudEvent*");
        }

        [Fact(DisplayName = "SetData method 3 sets the data field with JSON and sets the data object")]
        public void SetDataMethod3HappyPath1()
        {
            var cloudEvent = new CloudEvent();
            var client = new Client { FirstName = "Brian", LastName = "Friesen" };

            cloudEvent.SetData(client);

            string data = cloudEvent.Unlock()._data;

            data.Should().Be(JsonConvert.SerializeObject(client));
            _dataObjects.TryGetValue(cloudEvent, out var value).Should().BeTrue();
            value.Should().BeSameAs(client);
        }

        [Fact(DisplayName = "SetData method 3 sets the data field with XML and sets the data object")]
        public void SetDataMethod3HappyPath2()
        {
            var cloudEvent = new CloudEvent();
            var client = new Client { FirstName = "Brian", LastName = "Friesen" };

            cloudEvent.SetData(client, DataSerialization.Xml);

            string data = cloudEvent.Unlock()._data;

            data.Should().Be(XmlSerialize(client));
            _dataObjects.TryGetValue(cloudEvent, out var value).Should().BeTrue();
            value.Should().BeSameAs(client);
        }

        [Fact(DisplayName = "SetData method 3 clears the data field and the data object if data is null")]
        public void SetDataMethod3HappyPath3()
        {
            var cloudEvent = new CloudEvent();

            // In order to verify that _data has been cleared, capture its initial value.
            cloudEvent.Unlock()._data = "Hello, world!";

            // In order to verify that the method cleared the data object, there needs to be one first.
            _dataObjects.Add(cloudEvent, new object());

            Client client = null!;

            cloudEvent.SetData(client);

            object data = cloudEvent.Unlock()._data;

            data.Should().BeNull();
            _dataObjects.TryGetValue(cloudEvent, out _).Should().BeFalse();
        }

        [Fact(DisplayName = "SetData method 3 throws if cloudEvent is null")]
        public void SetDataMethod3SadPath1()
        {
            CloudEvent cloudEvent = null!;
            var client = new Client { FirstName = "Brian", LastName = "Friesen" };

            Action act = () => cloudEvent.SetData(client);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*cloudEvent*");
        }

        [Fact(DisplayName = "SetData method 3 throws if serialization is not defined")]
        public void SetDataMethod3SadPath2()
        {
            var cloudEvent = new CloudEvent();
            var client = new Client { FirstName = "Brian", LastName = "Friesen" };
            var serialization = (DataSerialization)(-123);

            Action act = () => cloudEvent.SetData(client, serialization);

            act.Should().ThrowExactly<ArgumentOutOfRangeException>().WithMessage("*serialization*");
        }

        [Fact(DisplayName = "GetData method JSON deserializes StringData")]
        public void GetDataMethodHappyPath1()
        {
            var cloudEvent = new CloudEvent();
            var clientData = JsonConvert.SerializeObject(new Client { FirstName = "Brian", LastName = "Friesen" });

            cloudEvent.Unlock()._data = clientData;

            var client = cloudEvent.GetData<Client>();

            client.FirstName.Should().Be("Brian");
            client.LastName.Should().Be("Friesen");
        }

        [Fact(DisplayName = "GetData method XML deserializes StringData")]
        public void GetDataMethodHappyPath2()
        {
            var cloudEvent = new CloudEvent();
            var clientData = XmlSerialize(new Client { FirstName = "Brian", LastName = "Friesen" });

            cloudEvent.Unlock()._data = clientData;

            var client = cloudEvent.GetData<Client>(DataSerialization.Xml);

            client.FirstName.Should().Be("Brian");
            client.LastName.Should().Be("Friesen");
        }

        [Fact(DisplayName = "GetData method returns data object if it already exists")]
        public void GetDataMethodHappyPath3()
        {
            var cloudEvent = new CloudEvent();
            var client = new Client { FirstName = "Brian", LastName = "Friesen" };

            _dataObjects.Add(cloudEvent, client);

            cloudEvent.GetData<Client>().Should().BeSameAs(client);
        }

        [Fact(DisplayName = "GetData method throws if cloudEvent is null")]
        public void GetDataMethodSadPath1()
        {
            CloudEvent cloudEvent = null!;

            Action act = () => cloudEvent.GetData<Client>();

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*cloudEvent*");
        }

        [Fact(DisplayName = "GetData method throws if serialization is not defined")]
        public void GetDataMethodSadPath2()
        {
            var cloudEvent = new CloudEvent();
            var serialization = (DataSerialization)(-123);

            Action act = () => cloudEvent.GetData<Client>(serialization);

            act.Should().ThrowExactly<ArgumentOutOfRangeException>().WithMessage("*serialization*");
        }

        [Fact(DisplayName = "GetData method throws if StringData is invalid JSON")]
        public void GetDataMethodSadPath3()
        {
            var cloudEvent = new CloudEvent();

            cloudEvent.Unlock()._data = "Not valid JSON";

            Action act = () => cloudEvent.GetData<Client>();

            act.Should().Throw<JsonException>();
        }

        [Fact(DisplayName = "GetData method throws if StringData is invalid XML")]
        public void GetDataMethodSadPath4()
        {
            var cloudEvent = new CloudEvent();

            cloudEvent.Unlock()._data = "Not valid JSON";

            Action act = () => cloudEvent.GetData<Client>(DataSerialization.Xml);

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact(DisplayName = "GetData method throws if data object already exists and cannot be cast to T")]
        public void GetDataMethodSadPath5()
        {
            var cloudEvent = new CloudEvent();

            _dataObjects.Add(cloudEvent, new NotAClient());

            Action act = () => cloudEvent.GetData<Client>();

            act.Should().ThrowExactly<InvalidCastException>();
        }

        [Fact(DisplayName = "TryGetData method JSON deserializes StringData")]
        public void TryGetDataMethodHappyPath1()
        {
            var cloudEvent = new CloudEvent();
            var clientData = JsonConvert.SerializeObject(new Client { FirstName = "Brian", LastName = "Friesen" });

            cloudEvent.Unlock()._data = clientData;

            cloudEvent.TryGetData(out Client? client).Should().BeTrue();

            client.FirstName.Should().Be("Brian");
            client.LastName.Should().Be("Friesen");
        }

        [Fact(DisplayName = "TryGetData method XML deserializes StringData")]
        public void TryGetDataMethodHappyPath2()
        {
            var cloudEvent = new CloudEvent();
            var clientData = XmlSerialize(new Client { FirstName = "Brian", LastName = "Friesen" });

            cloudEvent.Unlock()._data = clientData;

            cloudEvent.TryGetData(out Client? client, DataSerialization.Xml).Should().BeTrue();

            client.FirstName.Should().Be("Brian");
            client.LastName.Should().Be("Friesen");
        }

        [Fact(DisplayName = "TryGetData method returns data object if it already exists")]
        public void TryGetDataMethodHappyPath3()
        {
            var cloudEvent = new CloudEvent();
            var client = new Client { FirstName = "Brian", LastName = "Friesen" };

            _dataObjects.Add(cloudEvent, client);

            cloudEvent.TryGetData(out Client? actualClient).Should().BeTrue();
            actualClient.Should().BeSameAs(client);
        }

        [Fact(DisplayName = "TryGetData method throws if cloudEvent is null")]
        public void TryGetDataMethodSadPath1()
        {
            CloudEvent cloudEvent = null!;

            Action act = () => cloudEvent.TryGetData(out Client? client);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*cloudEvent*");
        }

        [Fact(DisplayName = "TryGetData method throws if serialization is not defined")]
        public void TryGetDataMethodSadPath2()
        {
            var cloudEvent = new CloudEvent();
            var serialization = (DataSerialization)(-123);

            Action act = () => cloudEvent.TryGetData(out Client? client, serialization);

            act.Should().ThrowExactly<ArgumentOutOfRangeException>().WithMessage("*serialization*");
        }

        [Fact(DisplayName = "TryGetData method returns false if StringData is invalid JSON")]
        public void TryGetDataMethodSadPath3()
        {
            var cloudEvent = new CloudEvent();

            cloudEvent.Unlock()._data = "Not valid JSON";

            cloudEvent.TryGetData(out Client? _).Should().BeFalse();
        }

        [Fact(DisplayName = "TryGetData method returns false if StringData is invalid XML")]
        public void TryGetDataMethodSadPath4()
        {
            var cloudEvent = new CloudEvent();

            cloudEvent.Unlock()._data = "Not valid JSON";

            cloudEvent.TryGetData(out Client? _, DataSerialization.Xml).Should().BeFalse();
        }

        [Fact(DisplayName = "TryGetData method returns false if data object already exists and cannot be cast to T")]
        public void TryGetDataMethodSadPath5()
        {
            var cloudEvent = new CloudEvent();

            _dataObjects.Add(cloudEvent, new NotAClient());

            cloudEvent.TryGetData(out Client? _).Should().BeFalse();
        }

        private static string XmlSerialize(Client client)
        {
            var sb = new StringBuilder();
            var serializer = new XmlSerializer(typeof(Client));
            using (var writer = new StringWriter(sb))
                serializer.Serialize(writer, client);
            return sb.ToString();
        }

        public class Client
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public class NotAClient { }
    }
}
