using System;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// The default implementation of the <see cref="ISenderBuilder"/> interface.
    /// </summary>
    public class SenderBuilder : ISenderBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SenderBuilder"/> class.
        /// </summary>
        /// <param name="registration">
        /// The registration delegate that is responsible for creating the <see cref="ISender"/>.
        /// </param>
        public SenderBuilder(SenderRegistration registration) =>
            Registration = registration ?? throw new ArgumentNullException(nameof(registration));

        /// <summary>
        /// Gets the registration delegate that is responsible for creating the <see cref="ISender"/>.
        /// </summary>
        public SenderRegistration Registration { get; private set; }

        /// <summary>
        /// Adds a decoration delegate to the builder.
        /// </summary>
        /// <param name="decoration">The decoration delegate.</param>
        public ISenderBuilder AddDecorator(SenderDecoration decoration)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(decoration);
#else
            if (decoration is null) { throw new ArgumentNullException(nameof(decoration)); }
#endif

            var registration = Registration;
            Registration = serviceProvider =>
                decoration.Invoke(registration.Invoke(serviceProvider), serviceProvider);
            return this;
        }

        /// <summary>
        /// Creates an instance of <see cref="ISender"/> using the <see cref="Registration"/>.
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> that retrieves the services required to create the <see cref="ISender"/>.
        /// </param>
        /// <returns>An instance of <see cref="ISender"/>.</returns>
        public ISender Build(IServiceProvider serviceProvider) => Registration(serviceProvider);
    }
}