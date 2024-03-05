using FluentAssertions;
using RockLib.Messaging.CloudEvents.Correlating;
using System;
using Xunit;

namespace RockLib.Messaging.CloudEvents.Tests
{
    public class CorrelatingExtensionsTests
    {
        [Fact(DisplayName = "GetCorrelationId extension method returns the 'correlationid' attribute")]
        public void GetCorrelationIdExtensionMethodHappyPath1()
        {
            var cloudEvent = new CloudEvent
            {
                Attributes = { [CorrelatedEvent.CorrelationIdAttribute] = "MyCorrelationId" }
            };

            var correlationId = cloudEvent.GetCorrelationId();

            correlationId.Should().Be("MyCorrelationId");
        }

        [Fact(DisplayName = "GetCorrelationId extension method adds the 'correlationid' attribute if missing")]
        public void GetCorrelationIdExtensionMethodHappyPath2()
        {
            var cloudEvent = new CloudEvent();

            cloudEvent.Attributes.Should().BeEmpty();

            cloudEvent.GetCorrelationId();

            cloudEvent.Attributes.Should().ContainKey(CorrelatedEvent.CorrelationIdAttribute)
                .WhoseValue.Should().NotBeNull();
        }

        [Fact(DisplayName = "GetCorrelationId extension method throws if cloudEvent parameter is null")]
        public void GetCorrelationIdExtensionMethodSadPath()
        {
            CloudEvent cloudEvent = null!;

            Action act = () => cloudEvent.GetCorrelationId();

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*cloudEvent*");
        }

        [Fact(DisplayName = "SetCorrelationId extension method sets the 'correlationid' attribute")]
        public void SetCorrelationIdExtensionMethodHappyPath()
        {
            var cloudEvent = new CloudEvent();

            cloudEvent.SetCorrelationId("MyCorrelationId");

            cloudEvent.Attributes.Should().ContainKey(CorrelatedEvent.CorrelationIdAttribute)
                .WhoseValue.Should().Be("MyCorrelationId");
        }

        [Fact(DisplayName = "SetCorrelationId extension method throws if cloudEvent is null")]
        public void SetCorrelationIdExtensionMethodSadPath1()
        {
            CloudEvent cloudEvent = null!;

            Action act = () => cloudEvent.SetCorrelationId("MyCorrelationId");

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*cloudEvent*");
        }

        [Theory(DisplayName = "SetCorrelationId extension method throws if correlationId is null or empty")]
        [InlineData(null)]
        [InlineData("")]
        public void SetCorrelationIdExtensionMethodSadPath2(string? correlationId)
        {
            var cloudEvent = new CloudEvent();

            Action act = () => cloudEvent.SetCorrelationId(correlationId!);

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*correlationId*");
        }
    }
}
