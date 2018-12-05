using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RockLib.Messaging.Testing
{
    /// <summary>
    /// An implementation of <see cref="IReceiverMessage"/> that makes testing
    /// <see cref="IReceiver"/> message handlers easier.
    /// </summary>
    public class TestReceiverMessage : IReceiverMessage
    {
        private readonly HeaderDictionary _headerDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestReceiverMessage"/> class.
        /// </summary>
        /// <param name="payload">The payload of the test message.</param>
        public TestReceiverMessage(string payload)
            : this()
        {
            StringPayload = payload ?? throw new ArgumentNullException(nameof(payload));
            BinaryPayload = Encoding.UTF8.GetBytes(payload);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestReceiverMessage"/> class.
        /// </summary>
        /// <param name="payload">The payload of the test message.</param>
        public TestReceiverMessage(byte[] payload)
            : this()
        {
            BinaryPayload = payload ?? throw new ArgumentNullException(nameof(payload));
            StringPayload = Convert.ToBase64String(payload);
        }

        private TestReceiverMessage()
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
        public bool Handled => HandledBy != null;

        /// <summary>
        /// Gets the name of the method (Acknowledge, Rollback, or Reject) that
        /// handled the test message.
        /// </summary>
        public string HandledBy { get; private set; }

        /// <inheritdoc />
        public void Acknowledge()
        {
            lock (this)
            {
                ThrowIfHandled();
                SetHandled();
            }
        }

        /// <inheritdoc />
        public void Rollback()
        {
            lock (this)
            {
                ThrowIfHandled();
                SetHandled();
            }
        }

        /// <inheritdoc />
        public void Reject()
        {
            lock (this)
            {
                ThrowIfHandled();
                SetHandled();
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
