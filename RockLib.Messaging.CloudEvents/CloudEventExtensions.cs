using RockLib.Messaging.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RockLib.Messaging.CloudEvents
{
    /// <summary>
    /// Extension methods related to CloudEvents.
    /// </summary>
    public static partial class CloudEventExtensions
    {
        private static readonly ConcurrentDictionary<Type, Constructor> _constructors = new ConcurrentDictionary<Type, Constructor>();

        /// <summary>
        /// Creates an instance of <typeparamref name="TCloudEvent"/> with properties mapped from
        /// the headers of <paramref name="receiverMessage"/>.
        /// <para>The <typeparamref name="TCloudEvent"/> type <em>must</em> have a constructor with
        /// the exact parameters: <see cref="IReceiverMessage"/>, <see cref="IProtocolBinding"/>.
        ///  A <see cref="MissingMemberException"/> is thrown if it does not.</para>
        /// </summary>
        /// <typeparam name="TCloudEvent">The type of <see cref="CloudEvent"/> to create.</typeparam>
        /// <param name="receiverMessage">
        /// The <see cref="IReceiverMessage"/> to be mapped to the new <typeparamref name="TCloudEvent"/>.
        /// </param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map <see cref="IReceiverMessage"/> headers to
        /// CloudEvent attributes.
        /// </param>
        /// <returns>
        /// A new <typeparamref name="TCloudEvent"/> with properties mapped from the headers of the <see cref="IReceiverMessage"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="MissingMemberException"></exception>
        public static TCloudEvent To<TCloudEvent>(this IReceiverMessage receiverMessage, IProtocolBinding protocolBinding = null)
            where TCloudEvent : CloudEvent
        {
            if (receiverMessage is null)
                throw new ArgumentNullException(nameof(receiverMessage));

            var constructor = _constructors.GetOrAdd(typeof(TCloudEvent), Constructor.Create)
                ?? throw MissingCloudEventConstructor(typeof(TCloudEvent));

            return (TCloudEvent)constructor.Invoke(receiverMessage, protocolBinding);
        }

        /// <summary>
        /// Start listening for CloudEvents and handle them using the specified callback function.
        /// </summary>
        /// <param name="receiver">The receiver to start.</param>
        /// <param name="onEventReceivedAsync">A function that is invoked when a CloudEvent is received.</param>
        public static void Start<TCloudEvent>(this IReceiver receiver, Func<TCloudEvent, IReceiverMessage, Task> onEventReceivedAsync, IProtocolBinding protocolBinding = null)
            where TCloudEvent : CloudEvent
        {
            if (!Constructor.Exists(typeof(TCloudEvent)))
                throw MissingCloudEventConstructor(typeof(TCloudEvent));

            receiver.Start(message => onEventReceivedAsync(message.To<TCloudEvent>(protocolBinding), message));
        }

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
            builder.AddValidation(message => CloudEvent.Validate(message, protocolBinding));

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

        private static MissingMemberException MissingCloudEventConstructor(Type cloudEventType) =>
            new MissingMemberException($"CloudEvent type '{cloudEventType.Name}' must have a public constructor"
                + $" with the exact parameters: '{nameof(IReceiverMessage)}', '{nameof(IProtocolBinding)}'.");
    }
}
