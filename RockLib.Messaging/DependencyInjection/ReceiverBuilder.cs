#if !NET451
using System;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// The default implementation of the <see cref="IReceiverBuilder"/> interface.
    /// </summary>
    public class ReceiverBuilder : IReceiverBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiverBuilder"/> class.
        /// </summary>
        /// <param name="registration">
        /// The registration delegate that is responsible for creating the <see cref="IReceiver"/>.
        /// </param>
        public ReceiverBuilder(ReceiverRegistration registration) =>
            Registration = registration ?? throw new ArgumentNullException(nameof(registration));

        /// <summary>
        /// Gets the registration delegate that is responsible for creating the <see cref="IReceiver"/>.
        /// </summary>
        public ReceiverRegistration Registration { get; private set; }

        /// <summary>
        /// Adds a decoration delegate to the builder.
        /// </summary>
        /// <param name="decoration">The decoration delegate.</param>
        public IReceiverBuilder AddDecorator(ReceiverDecoration decoration)
        {
            if (decoration is null)
                throw new ArgumentNullException(nameof(decoration));

            var registration = Registration;
            Registration = serviceProvider =>
                decoration.Invoke(registration.Invoke(serviceProvider), serviceProvider);
            return this;
        }

        /// <summary>
        /// Creates an instance of <see cref="IReceiver"/> using the <see cref="Registration"/>.
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> that retrieves the services required to create the <see cref="IReceiver"/>.
        /// </param>
        /// <returns>An instance of <see cref="IReceiver"/>.</returns>
        public IReceiver Build(IServiceProvider serviceProvider) => Registration(serviceProvider);
    }
}
#endif
