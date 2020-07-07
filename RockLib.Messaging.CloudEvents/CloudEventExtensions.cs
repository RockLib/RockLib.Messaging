using RockLib.Messaging.DependencyInjection;

namespace RockLib.Messaging.CloudEvents
{
    /// <summary>
    /// Extension methods related to CloudEvents.
    /// </summary>
    public static class CloudEventExtensions
    {
        #region CloudEvent

        /// <summary>
        /// Creates an instance of <see cref="CloudEvent"/> with properties mapped from the headers of
        /// <paramref name="receiverMessage"/>.
        /// </summary>
        /// <param name="receiverMessage">
        /// The <see cref="IReceiverMessage"/> to be mapped to the new <see cref="CloudEvent"/>.
        /// </param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map <see cref="IReceiverMessage"/> headers to
        /// CloudEvent attributes.
        /// </param>
        /// <returns>
        /// A new <see cref="CloudEvent"/> with properties mapped from the headers of the <see cref="IReceiverMessage"/>.
        /// </returns>
        public static CloudEvent ToCloudEvent(this IReceiverMessage receiverMessage, IProtocolBinding protocolBinding = null) =>
            CloudEvent.CreateCore<CloudEvent>(receiverMessage, ref protocolBinding);

        /// <summary>
        /// Adds a <see cref="ValidatingSender"/> decorator that ensures messages are valid CloudEvents.
        /// </summary>
        /// <param name="builder">The <see cref="ISenderBuilder"/>.</param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map CloudEvent attributes to <see cref="SenderMessage"/>
        /// headers.
        /// </param>
        /// <returns>The same <see cref="ISenderBuilder"/>.</returns>
        public static ISenderBuilder AddCloudEventValidation(this ISenderBuilder builder, IProtocolBinding protocolBinding = null) =>
            builder.AddValidation(message => CloudEvent.ValidateCore(message, ref protocolBinding));

        #endregion

        #region SequenceEvent

        /// <summary>
        /// Creates an instance of <see cref="SequenceEvent"/> with properties mapped from the headers of
        /// <paramref name="receiverMessage"/>.
        /// </summary>
        /// <param name="receiverMessage">
        /// The <see cref="IReceiverMessage"/> to be mapped to the new <see cref="SequenceEvent"/>.
        /// </param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map <see cref="IReceiverMessage"/> headers to
        /// SequenceEvent attributes.
        /// </param>
        /// <returns>
        /// A new <see cref="SequenceEvent"/> with properties mapped from the headers of the <see cref="IReceiverMessage"/>.
        /// </returns>
        public static SequenceEvent ToSequenceEvent(this IReceiverMessage receiverMessage, IProtocolBinding protocolBinding = null) =>
            SequenceEvent.Create(receiverMessage, protocolBinding);

        /// <summary>
        /// Adds a <see cref="ValidatingSender"/> decorator that ensures messages are valid SequenceEvents.
        /// </summary>
        /// <param name="builder">The <see cref="ISenderBuilder"/>.</param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map SequenceEvent attributes to <see cref="SenderMessage"/>
        /// headers.
        /// </param>
        /// <returns>The same <see cref="ISenderBuilder"/>.</returns>
        public static ISenderBuilder AddSequenceEventValidation(this ISenderBuilder builder, IProtocolBinding protocolBinding = null) =>
            builder.AddValidation(message => SequenceEvent.Validate(message, protocolBinding));

        #endregion
    }
}
