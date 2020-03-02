#if !NET451
using System;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// The default implementation of the <see cref="ITransactionalSenderBuilder"/> interface.
    /// </summary>
    public class TransactionalSenderBuilder : ITransactionalSenderBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionalSenderBuilder"/> class.
        /// </summary>
        /// <param name="registration">
        /// The registration delegate that is responsible for creating the <see cref="ITransactionalSender"/>.
        /// </param>
        public TransactionalSenderBuilder(TransactionalSenderRegistration registration) =>
            Registration = registration ?? throw new ArgumentNullException(nameof(registration));

        /// <summary>
        /// Gets the registration delegate that is responsible for creating the <see cref="ITransactionalSender"/>.
        /// </summary>
        public TransactionalSenderRegistration Registration { get; private set; }

        /// <summary>
        /// Adds a decoration delegate to the builder.
        /// </summary>
        /// <param name="decoration">The decoration delegate.</param>
        public ITransactionalSenderBuilder AddDecorator(TransactionalSenderDecoration decoration)
        {
            if (decoration is null)
                throw new ArgumentNullException(nameof(decoration));

            var registration = Registration;
            Registration = serviceProvider =>
                decoration.Invoke(registration.Invoke(serviceProvider), serviceProvider);
            return this;
        }

        /// <summary>
        /// Creates an instance of <see cref="ITransactionalSender"/> using the <see cref="Registration"/>.
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> that retrieves the services required to create the <see cref="ITransactionalSender"/>.
        /// </param>
        /// <returns>An instance of <see cref="ITransactionalSender"/>.</returns>
        public ITransactionalSender Build(IServiceProvider serviceProvider) => Registration(serviceProvider);
    }
}
#endif
