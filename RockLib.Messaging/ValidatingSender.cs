using System;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging
{
    /// <summary>
    /// A decorator for the <see cref="ISender"/> interface that invokes a <c>validate</c> delegate
    /// before each message is sent.
    /// </summary>
    public class ValidatingSender : ISender
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instances of the <see cref="ValidatingSender"/> class.
        /// </summary>
        /// <param name="name">The name of the validating sender.</param>
        /// <param name="sender">The <see cref="ISender"/> that actually sends messages.</param>
        /// <param name="validate">
        /// A delegate that will be invoked before each message is sent. The delegate should throw an exception
        /// if header values are invalid or if required headers are missing. The delegate may also add missing
        /// headers that can be calculated at runtime.
        /// </param>
        public ValidatingSender(string name, ISender sender, Action<SenderMessage> validate)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Validate = validate ?? throw new ArgumentNullException(nameof(validate));
        }

        /// <summary>
        /// Gets the name of the validating sender.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the <see cref="ISender"/> that actually sends messages.
        /// </summary>
        public ISender Sender { get; }

        /// <summary>
        /// Gets the delegate that is invoked before each message is sent.
        /// </summary>
        public Action<SenderMessage> Validate { get; }

        /// <summary>
        /// Validates, then asynchronously sends the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public Task SendAsync(SenderMessage message, CancellationToken cancellationToken)
        {
            Validate(message);
            return Sender.SendAsync(message, cancellationToken);
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing">Set to <c>true</c> when called from <see cref="Dispose()"/></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Sender.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
