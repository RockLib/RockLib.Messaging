using System;
using static RockLib.Messaging.CloudEvents.CorrelatedEvent;

namespace RockLib.Messaging.CloudEvents.Correlating
{
    /// <summary>
    /// Extension methods for getting and setting the Correlation ID of an event.
    /// </summary>
    public static class CorrelatingExtensions
    {
        /// <summary>
        /// Gets the Correlation ID of the event.
        /// </summary>
        /// <param name="cloudEvent">The cloud event.</param>
        /// <returns>The Correlation ID of the event.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="cloudEvent"/> is <see langword="null"/>.
        /// </exception>
        public static string GetCorrelationId(this CloudEvent cloudEvent)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(cloudEvent);
#else
            if (cloudEvent is null) { throw new ArgumentNullException(nameof(cloudEvent)); }
#endif

            if (cloudEvent.Attributes.TryGetValue(CorrelationIdAttribute, out var value) &&
                value is string correlationId)
            {
                return correlationId;
            }

            correlationId = NewCorrelationId();
            cloudEvent.Attributes[CorrelationIdAttribute] = correlationId;
            return correlationId;
        }

        /// <summary>
        /// Sets the Correlation ID of the event.
        /// </summary>
        /// <param name="cloudEvent">The cloud event.</param>
        /// <param name="correlationId">The Correlation ID of the event.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="cloudEvent"/> or <paramref name="correlationId"/> is <see langword=
        /// "null"/>.
        /// </exception>
        public static void SetCorrelationId(this CloudEvent cloudEvent, string correlationId)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(cloudEvent);
#else
            if (cloudEvent is null) { throw new ArgumentNullException(nameof(cloudEvent)); }
#endif

            if (string.IsNullOrEmpty(correlationId))
            {
                throw new ArgumentNullException(nameof(correlationId));
            }

            cloudEvent.Attributes[CorrelationIdAttribute] = correlationId;
        }
    }
}
