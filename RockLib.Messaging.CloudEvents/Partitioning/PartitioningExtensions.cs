using System;
using static RockLib.Messaging.CloudEvents.PartitionedEvent;

namespace RockLib.Messaging.CloudEvents.Partitioning
{
    /// <summary>
    /// Extension methods for getting and setting the Partition Key of an event.
    /// </summary>
    public static class PartitioningExtensions
    {
        /// <summary>
        /// Gets the Partition Key for the event, typically for the purposes of defining a causal
        /// relationship/grouping between multiple events. In cases where the CloudEvent is
        /// delivered to an event consumer via multiple hops, it is possible that the value of this
        /// attribute might change, or even be removed, due to protocol semantics or business
        /// processing logic within each hop.
        /// </summary>
        /// <param name="cloudEvent">The cloud event.</param>
        /// <returns>The Partition Key for the event.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="cloudEvent"/> is <see langword="null"/>.
        /// </exception>
        public static string? GetPartitionKey(this CloudEvent cloudEvent)
        {
            if (cloudEvent is null)
            {
                throw new ArgumentNullException(nameof(cloudEvent));
            }

            if (cloudEvent.Attributes.TryGetValue(PartitionKeyAttribute, out var value) && value is string key)
            {
                return key;
            }

            return null;
        }

        /// <summary>
        /// Sets the Partition Key for the event, typically for the purposes of defining a causal
        /// relationship/grouping between multiple events. In cases where the CloudEvent is
        /// delivered to an event consumer via multiple hops, it is possible that the value of this
        /// attribute might change, or even be removed, due to protocol semantics or business
        /// processing logic within each hop.
        /// </summary>
        /// <param name="cloudEvent">The cloud event.</param>
        /// <param name="partitionKey">
        /// The Partition Key for the event. If <see langword="null"/>, the Partition Key for the
        /// event is removed.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="cloudEvent"/> is <see langword="null"/>.
        /// </exception>
        public static void SetPartitionKey(this CloudEvent cloudEvent, string? partitionKey)
        {
            if (cloudEvent is null)
            {
                throw new ArgumentNullException(nameof(cloudEvent));
            }

            if (partitionKey is not null)
            {
                cloudEvent.Attributes[PartitionKeyAttribute] = partitionKey;
            }
            else
            {
                cloudEvent.Attributes.Remove(PartitionKeyAttribute);
            }
        }
    }
}
