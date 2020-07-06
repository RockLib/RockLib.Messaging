using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace RockLib.Messaging.CloudEvents.Tests
{
    public class DefaultCloudEventTests
    {
        [Fact]
        public void Constructor1HappyPath()
        {
            var cloudEvent = new DefaultCloudEvent();

            cloudEvent.Data.Should().BeNull();
        }

        [Fact]
        public void Constructor2HappyPath()
        {
            var cloudEvent = new DefaultCloudEvent("Hello, world!");

            cloudEvent.Data.Should().Be("Hello, world!");
        }

        [Fact]
        public void Constructor3HappyPath()
        {
            var data = new byte[] { 1, 2, 3, 4 };

            var cloudEvent = new DefaultCloudEvent(data);

            cloudEvent.Data.Should().BeSameAs(data);
        }

        [Fact]
        public void ImplicitOperatorHappyPath1()
        {
            var cloudEvent = new DefaultCloudEvent("Hello, world!")
            {
                Id = "MyId",
                Source = new Uri("http://mysource/"),
                Type = "test"
            };

            SenderMessage senderMessage = cloudEvent;

            senderMessage.StringPayload.Should().Be("Hello, world!");
            senderMessage.Headers.Should().ContainKey(CloudEvent.IdAttribute).WhichValue.Should().Be("MyId");
            senderMessage.Headers.Should().ContainKey(CloudEvent.SourceAttribute).WhichValue.ToString().Should().Be("http://mysource/");
            senderMessage.Headers.Should().ContainKey(CloudEvent.TypeAttribute).WhichValue.Should().Be("test");
        }

        [Fact]
        public void ImplicitOperatorHappyPath2()
        {
            DefaultCloudEvent cloudEvent = null;

            SenderMessage senderMessage = cloudEvent;

            senderMessage.Should().BeNull();
        }
    }
}
