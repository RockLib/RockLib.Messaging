using RockLib.Messaging.SNS;
using System;
using System.Diagnostics.CodeAnalysis;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// Defines the settings for creating instances of <see cref="SNSSender"/>.
    /// </summary>
    public class SNSSenderOptions
    {
        private string? _topicArn;

        /// <summary>
        /// Gets or sets the arn of the SNS topic.
        /// </summary>
        public string TopicArn
        {
            [return: MaybeNull] get => _topicArn!;
            set => _topicArn = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the region of the SNS topic.
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// Gets or sets the tag that specifies that a message belongs to a specific message
        /// group. Messages that belong to the same message group are processed in a FIFO manner
        /// (however, messages in different message groups might be processed out of order). To
        /// interleave multiple ordered streams within a single topic, use MessageGroupId values
        /// (for example, session data for multiple users). In this scenario, multiple consumers
        /// can process the topic, but the session data of each user is processed in a FIFO
        /// fashion.
        /// <para>This parameter applies only to FIFO (first-in-first-out) topics.</para>
        /// </summary>
        public string? MessageGroupId { get; set; }
    }
}
