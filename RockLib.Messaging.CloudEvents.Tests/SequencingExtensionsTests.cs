using FluentAssertions;
using RockLib.Messaging.CloudEvents.Sequencing;
using System;
using Xunit;

namespace RockLib.Messaging.CloudEvents.Tests
{
    public class SequencingExtensionsTests
    {
        [Fact(DisplayName = "GetSequence extension method returns the 'sequence' attribute")]
        public void GetSequenceExtensionMethodHappyPath1()
        {
            var cloudEvent = new CloudEvent
            {
                Attributes = { [SequentialEvent.SequenceAttribute] = "MySequence" }
            };

            var sequence = cloudEvent.GetSequence();

            sequence.Should().Be("MySequence");
        }

        [Fact(DisplayName = "GetSequence extension method returns null if 'sequence' attribute is missing")]
        public void GetSequenceExtensionMethodHappyPath2()
        {
            var cloudEvent = new CloudEvent();

            var sequence = cloudEvent.GetSequence();

            sequence.Should().BeNull();
        }

        [Fact(DisplayName = "GetSequence extension method returns null if 'sequence' attribute is not a string")]
        public void GetSequenceExtensionMethodHappyPath3()
        {
            var cloudEvent = new CloudEvent()
            {
                Attributes = { [SequentialEvent.SequenceAttribute] = DateTime.Now }
            };

            var sequence = cloudEvent.GetSequence();

            sequence.Should().BeNull();
        }

        [Fact(DisplayName = "GetSequence extension throws is cloudEvent is null")]
        public void GetSequenceExtensionMethodSadPath()
        {
            CloudEvent cloudEvent = null;

            Action act = () => cloudEvent.GetSequence();

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*cloudEvent*");
        }

        [Fact(DisplayName = "SetSequence extension method sets the 'sequence' attribute")]
        public void SetSequenceExtensionMethodHappyPath1()
        {
            var cloudEvent = new CloudEvent();

            cloudEvent.SetSequence("MySequence");

            cloudEvent.Attributes.Should().ContainKey(SequentialEvent.SequenceAttribute)
                .WhichValue.Should().Be("MySequence");
        }

        [Fact(DisplayName = "SetSequence extension method clears the 'sequence' attribute when value is null")]
        public void SetSequenceExtensionMethodHappyPath2()
        {
            var cloudEvent = new CloudEvent
            {
                Attributes = { [SequentialEvent.SequenceAttribute] = "MySequence" }
            };

            cloudEvent.SetSequence(null);

            cloudEvent.Attributes.Should().NotContainKey(SequentialEvent.SequenceAttribute);
        }

        [Fact(DisplayName = "SetSequence extension throws is cloudEvent is null")]
        public void SetSequenceExtensionMethodSadPath()
        {
            CloudEvent cloudEvent = null;

            Action act = () => cloudEvent.SetSequence("MySequence");

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*cloudEvent*");
        }

        [Fact(DisplayName = "GetSequenceType extension method returns the 'sequencetype' attribute")]
        public void GetSequenceTypeExtensionMethodHappyPath1()
        {
            var cloudEvent = new CloudEvent
            {
                Attributes = { [SequentialEvent.SequenceTypeAttribute] = "MySequenceType" }
            };

            var sequenceType = cloudEvent.GetSequenceType();

            sequenceType.Should().Be("MySequenceType");
        }

        [Fact(DisplayName = "GetSequenceType extension method returns null if 'sequencetype' attribute is missing")]
        public void GetSequenceTypeExtensionMethodHappyPath2()
        {
            var cloudEvent = new CloudEvent();

            var sequenceType = cloudEvent.GetSequenceType();

            sequenceType.Should().BeNull();
        }

        [Fact(DisplayName = "GetSequenceType extension method returns null if 'sequencetype' attribute is not a string")]
        public void GetSequenceTypeExtensionMethodHappyPath3()
        {
            var cloudEvent = new CloudEvent()
            {
                Attributes = { [SequentialEvent.SequenceTypeAttribute] = DateTime.Now }
            };

            var sequenceType = cloudEvent.GetSequenceType();

            sequenceType.Should().BeNull();
        }

        [Fact(DisplayName = "GetSequenceType extension throws is cloudEvent is null")]
        public void GetSequenceTypeExtensionMethodSadPath()
        {
            CloudEvent cloudEvent = null;

            Action act = () => cloudEvent.GetSequenceType();

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*cloudEvent*");
        }

        [Fact(DisplayName = "SetSequenceType extension method sets the 'sequencetype' attribute")]
        public void SetSequenceTypeExtensionMethodHappyPath1()
        {
            var cloudEvent = new CloudEvent();

            cloudEvent.SetSequenceType("MySequenceType");

            cloudEvent.Attributes.Should().ContainKey(SequentialEvent.SequenceTypeAttribute)
                .WhichValue.Should().Be("MySequenceType");
        }

        [Fact(DisplayName = "SetSequenceType extension method clears the 'sequencetype' attribute when value is null")]
        public void SetSequenceTypeExtensionMethodHappyPath2()
        {
            var cloudEvent = new CloudEvent
            {
                Attributes = { [SequentialEvent.SequenceTypeAttribute] = "MySequenceType" }
            };

            cloudEvent.SetSequenceType(null);

            cloudEvent.Attributes.Should().NotContainKey(SequentialEvent.SequenceTypeAttribute);
        }

        [Fact(DisplayName = "SetSequenceType extension throws is cloudEvent is null")]
        public void SetSequenceTypeExtensionMethodSadPath()
        {
            CloudEvent cloudEvent = null;

            Action act = () => cloudEvent.SetSequenceType("MySequenceType");

            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*cloudEvent*");
        }
    }
}
