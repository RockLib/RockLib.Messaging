using System;

namespace RockLib.Messaging.CloudEvents
{
    /// <summary>
    /// Defines a cloud event with a correlation ID.
    /// </summary>
    public class CorrelatedEvent : CloudEvent
    {
        /// <summary>The name of the <see cref="CorrelationId"/> attribute.</summary>
        public const string CorrelationIdAttribute = "correlationid";

        private string _correlationId;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelatedEvent"/> class.
        /// </summary>
        public CorrelatedEvent() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequentialEvent"/> class based on the
        /// source sequential event. All cloud and correlated event attributes except <see cref=
        /// "CloudEvent.Id"/> and <see cref="CloudEvent.Time"/> are copied to the new instance.
        /// Note that the source event's data is <em>not</em> copied to the new instance.
        /// </summary>
        /// <param name="source">
        /// The source for cloud and correlated event attribute values.
        /// </param>
        public CorrelatedEvent(CorrelatedEvent source)
            : base(source)
        {
            CorrelationId = source.CorrelationId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelatedEvent"/> class and sets its properties
        /// according to the payload and headers of the <paramref name="receiverMessage"/>.
        /// </summary>
        /// <param name="receiverMessage">
        /// The <see cref="IReceiverMessage"/> with headers that map to cloud event attributes.
        /// </param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map <see cref="IReceiverMessage"/> headers to
        /// CloudEvent attributes. If <see langword="null"/>, then <see cref="CloudEvent.DefaultProtocolBinding"/>
        /// is used instead.
        /// </param>
        public CorrelatedEvent(IReceiverMessage receiverMessage, IProtocolBinding protocolBinding = null)
            : base(receiverMessage, protocolBinding)
        {
            if (receiverMessage.Headers.TryGetValue(CorrelationIdHeader, out string correlationId))
            {
                CorrelationId = correlationId;
                AdditionalAttributes.Remove(CorrelationIdHeader);
            }
        }

        /// <summary>
        /// The correlation ID of the event.
        /// </summary>
        public string CorrelationId
        {
            get => _correlationId ?? (_correlationId = NewCorrelationId());
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));
                _correlationId = value;
            }
        }

        /// <summary>
        /// Creates a <see cref="SenderMessage"/> with headers mapped from the attributes of this correlated event.
        /// </summary>
        /// <returns>The mapped <see cref="SenderMessage"/>.</returns>
        public override SenderMessage ToSenderMessage()
        {
            var senderMessage = base.ToSenderMessage();

            senderMessage.Headers[CorrelationIdHeader] = CorrelationId;

            return senderMessage;
        }

        /// <summary>
        /// Ensures that the attributes for the correlated event are present.
        /// </summary>
        /// <param name="senderMessage">The <see cref="SenderMessage"/> to validate.</param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map CloudEvent attributes to <see cref="SenderMessage"/>
        /// headers. If <see langword="null"/>, then <see cref="CloudEvent.DefaultProtocolBinding"/> is used
        /// instead.
        /// </param>
        public static new void Validate(SenderMessage senderMessage, IProtocolBinding protocolBinding = null)
        {
            if (protocolBinding is null)
                protocolBinding = DefaultProtocolBinding;

            CloudEvent.Validate(senderMessage, protocolBinding);

            var correlationIdHeader = protocolBinding.GetHeaderName(CorrelationIdAttribute);
            if (!ContainsHeader<string>(senderMessage, correlationIdHeader))
                senderMessage.Headers[correlationIdHeader] = NewCorrelationId();
        }

        private string CorrelationIdHeader => ProtocolBinding.GetHeaderName(CorrelationIdAttribute);

        private static string NewCorrelationId() => Guid.NewGuid().ToString();
    }
}
