using System;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering a validating sender.
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Adds a <see cref="ValidatingSender"/> decorator that ensures all messages are valid according
        /// to the <paramref name="validateMessage"/> parameter.
        /// </summary>
        /// <param name="builder">The <see cref="ISenderBuilder"/>.</param>
        /// <param name="validateMessage">
        /// A delegate that will be invoked before each message is sent. The delegate should throw an exception
        /// if header values are invalid or if required headers are missing. Delegates may also add missing
        /// headers that can be calculated at runtime.
        /// </param>
        /// <returns>The same <see cref="ISenderBuilder"/>.</returns>
        public static ISenderBuilder AddValidation(this ISenderBuilder builder, Action<SenderMessage> validateMessage)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(validateMessage);
#else
            if (builder is null) { throw new ArgumentNullException(nameof(builder)); }
            if (validateMessage is null) { throw new ArgumentNullException(nameof(validateMessage)); }
#endif

            return builder.AddDecorator((sender, serviceProvider) =>
                new ValidatingSender(sender.Name, sender, validateMessage));
        }

        /// <summary>
        /// Adds a <see cref="ValidatingSender"/> decorator that ensures all messages are valid according
        /// to the <paramref name="validateMessage"/> parameter.
        /// </summary>
        /// <param name="builder">The <see cref="ITransactionalSenderBuilder"/>.</param>
        /// <param name="validateMessage">
        /// A delegate that will be invoked before each message is sent or added to a transaction. The delegate
        /// should throw an exception if header values are invalid or if required headers are missing. The delegate
        /// may also add missing headers that can be calculated at runtime.
        /// </param>
        /// <returns>The same <see cref="ITransactionalSenderBuilder"/>.</returns>
        public static ITransactionalSenderBuilder AddValidation(this ITransactionalSenderBuilder builder, Action<SenderMessage> validateMessage)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(validateMessage);
#else
            if (builder is null) { throw new ArgumentNullException(nameof(builder)); }
            if (validateMessage is null) { throw new ArgumentNullException(nameof(validateMessage)); }
#endif

            return builder.AddDecorator((transactionalSender, serviceProvider) =>
                new ValidatingTransactionalSender(transactionalSender.Name, transactionalSender, validateMessage));
        }
    }
}