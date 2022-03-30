using FluentAssertions;
using RockLib.Messaging.CloudEvents.Partitioning;
using System;
using Xunit;

namespace RockLib.Messaging.CloudEvents.Tests
{
    public class PartitioningExtensionsTests
    {
        [Fact(DisplayName = "GetPartitionKey extension method returns the 'partitionkey' attribute")]
        public void GetPartitionKeyExtensionMethodHappyPath1()
        {
            var cloudEvent = new CloudEvent
            {
                Attributes = { [PartitionedEvent.PartitionKeyAttribute] = "MyPartitionKey" }
            };

            var partitionKey = cloudEvent.GetPartitionKey();

            partitionKey.Should().Be("MyPartitionKey");
        }

        [Fact(DisplayName = "GetPartitionKey extension method returns null if 'partitionkey' attribute is missing")]
        public void GetPartitionKeyExtensionMethodHappyPath2()
        {
            var cloudEvent = new CloudEvent();

            var partitionKey = cloudEvent.GetPartitionKey();

            partitionKey.Should().BeNull();
        }

        [Fact(DisplayName = "GetPartitionKey extension method returns null if 'partitionkey' attribute is not a string")]
        public void GetPartitionKeyExtensionMethodHappyPath3()
        {
            var cloudEvent = new CloudEvent()
            {
                Attributes = { [PartitionedEvent.PartitionKeyAttribute] = DateTime.Now }
            };

            var partitionKey = cloudEvent.GetPartitionKey();

            partitionKey.Should().BeNull();
        }

        [Fact(DisplayName = "GetPartitionKey extension throws is cloudEvent is null")]
        public void GetPartitionKeyExtensionMethodSadPath()
        {
            CloudEvent cloudEvent = null!;

            Action act = () => cloudEvent.GetPartitionKey();

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*cloudEvent*");
        }

        [Fact(DisplayName = "SetPartitionKey extension method sets the 'partitionkey' attribute")]
        public void SetPartitionKeyExtensionMethodHappyPath1()
        {
            var cloudEvent = new CloudEvent();

            cloudEvent.SetPartitionKey("MyPartitionKey");

            cloudEvent.Attributes.Should().ContainKey(PartitionedEvent.PartitionKeyAttribute)
                .WhoseValue.Should().Be("MyPartitionKey");
        }

        [Fact(DisplayName = "SetPartitionKey extension method clears the 'partitionkey' attribute when value is null")]
        public void SetPartitionKeyExtensionMethodHappyPath2()
        {
            var cloudEvent = new CloudEvent
            {
                Attributes = { [PartitionedEvent.PartitionKeyAttribute] = "MyPartitionKey" }
            };

            cloudEvent.SetPartitionKey(null);

            cloudEvent.Attributes.Should().NotContainKey(PartitionedEvent.PartitionKeyAttribute);
        }

        [Fact(DisplayName = "SetPartitionKey extension throws is cloudEvent is null")]
        public void SetPartitionKeyExtensionMethodSadPath()
        {
            CloudEvent cloudEvent = null!;

            Action act = () => cloudEvent.SetPartitionKey("MyPartitionKey");

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*cloudEvent*");
        }
    }
}
