using System;

namespace RockLib.Messaging.CloudEvents
{
    /// <summary>
    /// The default CloudEvent.
    /// </summary>
    public sealed class DefaultCloudEvent : CloudEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCloudEvent"/> type.
        /// </summary>
        public DefaultCloudEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCloudEvent"/> type.
        /// </summary>
        /// <param name="data">The data of the cloud event.</param>
        public DefaultCloudEvent(string data) => SetData(data);

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCloudEvent"/> type.
        /// </summary>
        /// <param name="data">The data of the cloud event.</param>
        public DefaultCloudEvent(byte[] data) => SetData(data);

        /// <summary>
        /// Validates that the <paramref name="senderMessage"/> parameter has the correct headers in
        /// order to represent a CloudEvent. Adds any missing required headers that can be determined
        /// at runtime.
        /// </summary>
        /// <param name="senderMessage"></param>
        public static void Validate(SenderMessage senderMessage) =>
            ValidateCore(senderMessage ?? throw new ArgumentNullException(nameof(senderMessage)));

        /// <summary>
        /// Creates an instance of <see cref="DefaultCloudEvent"/> with properties mapped from the headers of
        /// <paramref name="receiverMessage"/>.
        /// </summary>
        /// <param name="receiverMessage">
        /// The <see cref="IReceiverMessage"/> to be mapped to the new <see cref="DefaultCloudEvent"/>.
        /// </param>
        /// <returns>
        /// A new <see cref="DefaultCloudEvent"/> with properties mapped from the headers of the <see cref="IReceiverMessage"/>.
        /// </returns>
        public static DefaultCloudEvent Create(IReceiverMessage receiverMessage) =>
            Create<DefaultCloudEvent>(receiverMessage ?? throw new ArgumentNullException(nameof(receiverMessage)));

        /// <summary>
        /// Converts the <see cref="DefaultCloudEvent"/> to a <see cref="SenderMessage"/> by calling
        /// <see cref="CloudEvent.ToSenderMessage"/>.
        /// </summary>
        /// <param name="cloudEvent">The <see cref="DefaultCloudEvent"/> to convert to a <see cref="SenderMessage"/>.</param>
        public static implicit operator SenderMessage(DefaultCloudEvent cloudEvent) =>
            cloudEvent?.ToSenderMessage();
    }
}
