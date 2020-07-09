namespace RockLib.Messaging.CloudEvents
{
    /// <summary>
    /// This extension defines two attributes that can be included within a CloudEvent to describe the
    /// position of an event in the ordered sequence of events produced by a unique event source.
    /// 
    /// <para>The <c>sequence</c> attribute represents the value of this event's order in the stream of
    /// events. The exact value and meaning of this attribute is defined by the <c>sequencetype</c>
    /// attribute. If the <c>sequencetype</c> is missing, or not defined in this specification, event
    /// consumers will need to have some out-of-band communication with the event producer to understand
    /// how to interpret the value of the attribute.</para>
    /// </summary>
    public class SequentialEvent : CloudEvent
    {
        /// <summary>The name of the <see cref="Sequence"/> attribute.</summary>
        public const string SequenceAttribute = "sequence";

        /// <summary>The name of the <see cref="SequenceType"/> attribute.</summary>
        public const string SequenceTypeAttribute = "sequencetype";

        /// <summary>
        /// Initializes a new instance of the <see cref="SequentialEvent"/> class.
        /// </summary>
        public SequentialEvent() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequentialEvent"/> class.
        /// </summary>
        /// <param name="data">The data (payload) of the sequential event.</param>
        public SequentialEvent(string data) : base(data) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequentialEvent"/> class.
        /// </summary>
        /// <param name="data">The data (payload) of the sequential event.</param>
        public SequentialEvent(byte[] data) : base(data) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequentialEvent"/> class based on the
        /// source sequential event. All cloud and sequential event attributes except <see cref=
        /// "CloudEvent.Id"/> and <see cref="CloudEvent.Time"/> are copied to the new instance. If
        /// <see cref="SequenceType"/> is <see cref="SequenceTypes.Integer"/>, then the value of
        /// <see cref="Sequence"/> is incremented for the new instance. Note that neither the
        /// source's <see cref="CloudEvent.Data"/> nor any of its <see cref=
        /// "CloudEvent.AdditionalAttributes"/> are copied to the new instance.
        /// </summary>
        /// <param name="source">
        /// The source for cloud and sequential event attribute values.
        /// </param>
        public SequentialEvent(SequentialEvent source)
            : base(source)
        {
            SequenceType = source.SequenceType;

            if (SequenceType == SequenceTypes.Integer
                && int.TryParse(source.Sequence, out int sequence))
            {
                Sequence = unchecked(sequence + 1).ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequentialEvent"/> class and sets its properties
        /// according to the payload and headers of the <paramref name="receiverMessage"/>.
        /// </summary>
        /// <param name="receiverMessage">
        /// The <see cref="IReceiverMessage"/> with headers that map to cloud event attributes.
        /// </param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map <see cref="IReceiverMessage"/> headers to
        /// CloudEvent attributes. If <see langword="null"/>, then <see cref="CloudEvent.DefaultProtocolBinding"/>
        /// is used instead (and replaces the value of the <c>ref</c> parameter).
        /// </param>
        public SequentialEvent(IReceiverMessage receiverMessage, IProtocolBinding protocolBinding = null)
            : base(receiverMessage, protocolBinding)
        {
            if (protocolBinding is null)
                protocolBinding = DefaultProtocolBinding;

            var sequenceHeader = protocolBinding.GetHeaderName(SequenceAttribute);
            if (receiverMessage.Headers.TryGetValue(sequenceHeader, out string sequence))
            {
                Sequence = sequence;
                AdditionalAttributes.Remove(sequenceHeader);
            }

            var sequenceTypeHeader = protocolBinding.GetHeaderName(SequenceTypeAttribute);
            if (receiverMessage.Headers.TryGetValue(sequenceTypeHeader, out string sequenceType))
            {
                SequenceType = sequenceType;
                AdditionalAttributes.Remove(sequenceTypeHeader);
            }
        }

        /// <summary>
        /// REQUIRED. Value expressing the relative order of the event. This enables interpretation of
        /// data supercedence.
        /// </summary>
        public string Sequence { get; set; }

        /// <summary>
        /// Specifies the semantics of the sequence attribute. See the <see cref="SequenceTypes"/> class
        /// for known values of this attribute.
        /// </summary>
        public string SequenceType { get; set; }

        /// <summary>
        /// Creates a <see cref="SenderMessage"/> with headers mapped from the attributes of this sequential event.
        /// </summary>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map SequentialEvent attributes to <see cref="SenderMessage"/>
        /// headers. If <see langword="null"/>, then <see cref="CloudEvent.DefaultProtocolBinding"/> is used
        /// instead.
        /// </param>
        /// <returns>The mapped <see cref="SenderMessage"/>.</returns>
        public override SenderMessage ToSenderMessage(IProtocolBinding protocolBinding = null)
        {
            if (protocolBinding is null)
                protocolBinding = DefaultProtocolBinding;

            var senderMessage = base.ToSenderMessage(protocolBinding);

            if (Sequence != null)
                senderMessage.Headers[protocolBinding.GetHeaderName(SequenceAttribute)] = Sequence;

            if (SequenceType != null)
                senderMessage.Headers[protocolBinding.GetHeaderName(SequenceTypeAttribute)] = SequenceType;

            return senderMessage;
        }

        /// <inheritdoc/>
        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrEmpty(Sequence))
                throw new CloudEventValidationException("Sequence cannot be null or empty.");
        }

        /// <summary>
        /// Ensures that the attributes for the sequential event are present.
        /// </summary>
        /// <param name="senderMessage">The <see cref="SenderMessage"/> to validate.</param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map CloudEvent attributes to <see cref="SenderMessage"/>
        /// headers. If <see langword="null"/>, then <see cref="CloudEvent.DefaultProtocolBinding"/> is used
        /// instead.
        /// </param>
        public static void Validate(SenderMessage senderMessage, IProtocolBinding protocolBinding = null)
        {
            if (protocolBinding is null)
                protocolBinding = DefaultProtocolBinding;

            ValidateCore(senderMessage, protocolBinding);

            var sequenceHeader = protocolBinding.GetHeaderName(SequenceAttribute);
            if (!ContainsHeader<string>(senderMessage, sequenceHeader))
                throw new CloudEventValidationException($"The '{sequenceHeader}' header is missing from the SenderMessage.");
        }
    }
}
