using System;
using static RockLib.Messaging.CloudEvents.SequentialEvent;

namespace RockLib.Messaging.CloudEvents.Sequencing
{
    /// <summary>
    /// Extension methods for getting and setting the Sequence and Sequence Type of an event.
    /// </summary>
    public static class SequencingExtensions
    {
        /// <summary>
        /// Gets the value expressing the relative order of the event. This enables interpretation
        /// of data supercedence.
        /// </summary>
        /// <param name="cloudEvent">The cloud event.</param>
        /// <returns>The Sequence of the event.</returns>
        public static string? GetSequence(this CloudEvent cloudEvent)
        {
            if (cloudEvent is null)
                throw new ArgumentNullException(nameof(cloudEvent));

            if (cloudEvent.Attributes.TryGetValue(SequenceAttribute, out var value) && value is string sequence)
                return sequence;

            return null;
        }

        /// <summary>
        /// Sets the value expressing the relative order of the event. This enables interpretation
        /// of data supercedence.
        /// </summary>
        /// <param name="cloudEvent">The cloud event.</param>
        /// <param name="sequence">
        /// The Sequence of the event. If <see langword="null"/>, the Sequence of the event is
        /// removed.
        /// </param>
        public static void SetSequence(this CloudEvent cloudEvent, string sequence)
        {
            if (cloudEvent is null)
                throw new ArgumentNullException(nameof(cloudEvent));

            if (sequence != null)
                cloudEvent.Attributes[SequenceAttribute] = sequence;
            else
                cloudEvent.Attributes.Remove(SequenceAttribute);
        }

        /// <summary>
        /// Gets a value representing the semantics of the sequence attribute. See the <see cref=
        /// "SequenceTypes"/> class for known values of this attribute.
        /// </summary>
        /// <param name="cloudEvent">The cloud event.</param>
        /// <returns>The Sequence Type of the event.</returns>
        public static string? GetSequenceType(this CloudEvent cloudEvent)
        {
            if (cloudEvent is null)
                throw new ArgumentNullException(nameof(cloudEvent));

            if (cloudEvent.Attributes.TryGetValue(SequenceTypeAttribute, out var value) && value is string sequenceType)
                return sequenceType;

            return null;
        }

        /// <summary>
        /// Sets the value representing the semantics of the sequence attribute. See the <see cref=
        /// "SequenceTypes"/> class for known values of this attribute.
        /// </summary>
        /// <param name="cloudEvent">The cloud event.</param>
        /// <param name="sequenceType">
        /// The Sequence Type of the event. If <see langword="null"/>, the Sequence Type of the
        /// event is removed.
        /// </param>
        public static void SetSequenceType(this CloudEvent cloudEvent, string sequenceType)
        {
            if (cloudEvent is null)
                throw new ArgumentNullException(nameof(cloudEvent));

            if (sequenceType != null)
                cloudEvent.Attributes[SequenceTypeAttribute] = sequenceType;
            else
                cloudEvent.Attributes.Remove(SequenceTypeAttribute);
        }
    }
}
