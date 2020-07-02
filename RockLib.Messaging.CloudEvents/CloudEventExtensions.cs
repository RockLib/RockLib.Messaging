using RockLib.Messaging.DependencyInjection;

namespace RockLib.Messaging.CloudEvents
{
    /// <summary>
    /// Extension methods related to CloudEvents.
    /// </summary>
    public static class CloudEventExtensions
    {
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
        public static DefaultCloudEvent ToCloudEvent(this IReceiverMessage receiverMessage) =>
            DefaultCloudEvent.Create(receiverMessage);

        /// <summary>
        /// Adds a <see cref="ValidatingSender"/> decorator that ensures messages are valid CloudEvents.
        /// </summary>
        /// <param name="builder">The <see cref="ISenderBuilder"/>.</param>
        /// <returns>The same <see cref="ISenderBuilder"/>.</returns>
        public static ISenderBuilder AddCloudEventValidation(this ISenderBuilder builder) =>
            builder.AddValidation(DefaultCloudEvent.Validate);
    }
}
