using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.Tests
{
    /// <summary>
    /// A fake implementation of the <see cref="IReceiverMessage"/> interface that allows
    /// an application's <see cref="IReceiver"/> message handler implementation to be unit
    /// tested without requiring a mocking framework.
    /// </summary>
    public class FakeReceiverMessage : IReceiverMessage
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly HeaderDictionary _headerDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeReceiverMessage"/> class.
        /// </summary>
        /// <param name="payload">The payload of the fake message.</param>
        public FakeReceiverMessage(string payload)
            : this()
        {
            StringPayload = payload ?? throw new ArgumentNullException(nameof(payload));
            BinaryPayload = Encoding.UTF8.GetBytes(payload);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeReceiverMessage"/> class.
        /// </summary>
        /// <param name="payload">The payload of the fake message.</param>
        public FakeReceiverMessage(byte[] payload)
            : this()
        {
            BinaryPayload = payload ?? throw new ArgumentNullException(nameof(payload));
            StringPayload = Convert.ToBase64String(payload);
            Headers.Add(HeaderNames.IsBinaryPayload, true);
        }

        private FakeReceiverMessage()
        {
            var headers = new Dictionary<string, object>();
            Headers = headers;
            _headerDictionary = new HeaderDictionary(headers);
        }

        /// <inheritdoc />
        public string StringPayload { get; }

        /// <inheritdoc />
        public byte[] BinaryPayload { get; }

        /// <summary>
        /// Gets the headers of the test message.
        /// </summary>
        public IDictionary<string, object> Headers { get; }

        HeaderDictionary IReceiverMessage.Headers => _headerDictionary;

        /// <inheritdoc />
        public bool Handled => HandledBy is not null;

        /// <summary>
        /// Gets the name of the method (Acknowledge, Rollback, or Reject) that
        /// handled the test message.
        /// </summary>
        public string HandledBy { get; private set; }

        /// <inheritdoc />
        public async Task AcknowledgeAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                ThrowIfHandled();
                SetHandled();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                ThrowIfHandled();
                SetHandled();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public async Task RejectAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                ThrowIfHandled();
                SetHandled();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void ThrowIfHandled([CallerMemberName] string callerMemberName = null)
        {
            if (Handled)
                throw new InvalidOperationException($"Cannot {callerMemberName} message: the message has already been handled by {HandledBy}.");
        }

        private void SetHandled([CallerMemberName] string callerMemberName = null)
        {
            HandledBy = callerMemberName;
        }
    }
}
