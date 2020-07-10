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
            new CloudEvent(receiverMessage, protocolBinding);

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
            builder.AddValidation(message => CloudEvent.ValidateCore(message, protocolBinding));

        #endregion

        #region SequentialEvent

        /// <summary>
        /// Creates an instance of <see cref="SequentialEvent"/> with properties mapped from the headers of
        /// <paramref name="receiverMessage"/>.
        /// </summary>
        /// <param name="receiverMessage">
        /// The <see cref="IReceiverMessage"/> to be mapped to the new <see cref="SequentialEvent"/>.
        /// </param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map <see cref="IReceiverMessage"/> headers to
        /// SequentialEvent attributes.
        /// </param>
        /// <returns>
        /// A new <see cref="SequentialEvent"/> with properties mapped from the headers of the <see cref="IReceiverMessage"/>.
        /// </returns>
        public static SequentialEvent ToSequentialEvent(this IReceiverMessage receiverMessage, IProtocolBinding protocolBinding = null) =>
            new SequentialEvent(receiverMessage, protocolBinding);

        /// <summary>
        /// Adds a <see cref="ValidatingSender"/> decorator that ensures messages are valid SequentialEvents.
        /// </summary>
        /// <param name="builder">The <see cref="ISenderBuilder"/>.</param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map SequentialEvent attributes to <see cref="SenderMessage"/>
        /// headers.
        /// </param>
        /// <returns>The same <see cref="ISenderBuilder"/>.</returns>
        public static ISenderBuilder AddSequentialEventValidation(this ISenderBuilder builder, IProtocolBinding protocolBinding = null) =>
            builder.AddValidation(message => SequentialEvent.Validate(message, protocolBinding));

        #endregion

        #region CorrelatedEvent

        /// <summary>
        /// Creates an instance of <see cref="CorrelatedEvent"/> with properties mapped from the headers of
        /// <paramref name="receiverMessage"/>.
        /// </summary>
        /// <param name="receiverMessage">
        /// The <see cref="IReceiverMessage"/> to be mapped to the new <see cref="CorrelatedEvent"/>.
        /// </param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map <see cref="IReceiverMessage"/> headers to
        /// CorrelatedEvent attributes.
        /// </param>
        /// <returns>
        /// A new <see cref="CorrelatedEvent"/> with properties mapped from the headers of the <see cref="IReceiverMessage"/>.
        /// </returns>
        public static CorrelatedEvent ToCorrelatedEvent(this IReceiverMessage receiverMessage, IProtocolBinding protocolBinding = null) =>
            new CorrelatedEvent(receiverMessage, protocolBinding);

        /// <summary>
        /// Adds a <see cref="ValidatingSender"/> decorator that ensures messages are valid CorrelatedEvents.
        /// </summary>
        /// <param name="builder">The <see cref="ISenderBuilder"/>.</param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map CorrelatedEvent attributes to <see cref="SenderMessage"/>
        /// headers.
        /// </param>
        /// <returns>The same <see cref="ISenderBuilder"/>.</returns>
        public static ISenderBuilder AddCorrelatedEventValidation(this ISenderBuilder builder, IProtocolBinding protocolBinding = null) =>
            builder.AddValidation(message => CorrelatedEvent.Validate(message, protocolBinding));

        #endregion
    }
}
