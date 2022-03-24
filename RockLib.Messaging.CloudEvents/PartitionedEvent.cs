using RockLib.Messaging.CloudEvents.Partitioning;

namespace RockLib.Messaging.CloudEvents
{
    /// <summary>
    /// This extension defines an attribute for use by message brokers and their clients that
    /// support partitioning of events, typically for the purpose of scaling.
    /// <para>
    /// Often in large scale systems, during times of heavy load, events being received need to be
    /// partitioned into multiple buckets so that each bucket can be separately processed in order
    /// for the system to manage the incoming load. A partitioning key can be used to determine
    /// which bucket each event goes into. The entity sending the events can ensure that events
    /// that need to be placed into the same bucket are done so by using the same partition key on
    /// those events.
    /// </para></summary>
    public class PartitionedEvent : CloudEvent
    {
        /// <summary>The name of the <see cref="PartitionKey"/> attribute.</summary>
        public const string PartitionKeyAttribute = "partitionkey";

        /// <summary>
        /// Initializes a new instance of the <see cref="PartitionedEvent"/> class.
        /// </summary>
        public PartitionedEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudEvent"/> class based on the source
        /// cloud event. All cloud event attributes except <see cref="CloudEvent.Id"/> and <see
        /// cref="CloudEvent.Time"/> are copied to the new instance. Note that the source event's
        /// data is <em>not</em> copied to the new instance.
        /// </summary>
        /// <param name="source">
        /// The source for cloud event attribute values.
        /// </param>
        public PartitionedEvent(CloudEvent source)
            : base(source)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartitionedEvent"/> class and sets its
        /// data, attributes, and headers according to the payload and headers of the <paramref
        /// name="receiverMessage"/>.
        /// </summary>
        /// <param name="receiverMessage">
        /// The <see cref="IReceiverMessage"/> with headers that map to cloud event attributes.
        /// </param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map <see cref="IReceiverMessage"/> headers
        /// to CloudEvent attributes. If <see langword="null"/>, then <see cref=
        /// "CloudEvent.DefaultProtocolBinding"/> is used instead.
        /// </param>
        public PartitionedEvent(IReceiverMessage receiverMessage, IProtocolBinding? protocolBinding = null)
            : base(receiverMessage, protocolBinding)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartitionedEvent"/> class and sets its
        /// data and attributes according to the <a href=
        /// "https://github.com/cloudevents/spec/blob/v1.0/json-format.md">JSON Formatted
        /// CloudEvent</a>.
        /// </summary>
        /// <param name="jsonFormattedCloudEvent">
        /// A JSON Formatted CloudEvent.
        /// </param>
        public PartitionedEvent(string jsonFormattedCloudEvent)
            : base(jsonFormattedCloudEvent)
        {
        }

        /// <summary>
        /// A partition key for the event, typically for the purposes of defining a causal
        /// relationship/grouping between multiple events. In cases where the CloudEvent is
        /// delivered to an event consumer via multiple hops, it is possible that the value of this
        /// attribute might change, or even be removed, due to protocol semantics or business
        /// processing logic within each hop.
        /// </summary>
        public string? PartitionKey
        {
            get => this.GetPartitionKey();
            set => this.SetPartitionKey(value);
        }

        /// <inheritdoc/>
        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrEmpty(PartitionKey))
            {
                throw new CloudEventValidationException("PartitionKey cannot be null or empty.");
            }
        }

        /// <summary>
        /// Ensures that the attributes for the partitioned event are present.
        /// </summary>
        /// <param name="senderMessage">The <see cref="SenderMessage"/> to validate.</param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map CloudEvent attributes to <see cref="SenderMessage"/>
        /// headers. If <see langword="null"/>, then <see cref="CloudEvent.DefaultProtocolBinding"/> is used
        /// instead.
        /// </param>
        /// <exception cref="CloudEventValidationException">
        /// If the <see cref="SenderMessage"/> is not valid.
        /// </exception>
        public static new void Validate(SenderMessage senderMessage, IProtocolBinding? protocolBinding = null)
        {
            if (protocolBinding is null)
            {
                protocolBinding = DefaultProtocolBinding;
            }

            CloudEvent.Validate(senderMessage, protocolBinding);

            var partitionKeyHeader = protocolBinding.GetHeaderName(PartitionKeyAttribute);
            if (!ContainsHeader<string>(senderMessage, partitionKeyHeader))
            {
                throw new CloudEventValidationException(
                    $"The '{partitionKeyHeader}' header is missing from the SenderMessage.");
            }
        }
    }
}
