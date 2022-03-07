using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.NamedPipes
{
    /// <summary>
    /// An implementation of IReceiverMessage for use by the <see cref="NamedPipeReceiver"/>
    /// class.
    /// </summary>
    public class NamedPipeReceiverMessage : ReceiverMessage
    {
        private readonly NamedPipeMessage _namedPipeMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeReceiverMessage"/> class.
        /// </summary>
        /// <param name="namedPipeMessage">The message that was sent.</param>
        internal NamedPipeReceiverMessage(NamedPipeMessage namedPipeMessage)
            : base(() => namedPipeMessage.StringValue!)
        {
            _namedPipeMessage = namedPipeMessage;
        }

        /// <inheritdoc />
        protected override Task AcknowledgeMessageAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <inheritdoc />
        protected override Task RollbackMessageAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <inheritdoc />
        protected override Task RejectMessageAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <inheritdoc />
        protected override void InitializeHeaders(IDictionary<string, object> headers)
        {
            if (headers is null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            if (_namedPipeMessage.Headers is not null)
            {
                foreach (var header in _namedPipeMessage.Headers)
                {
                    headers.Add(header.Key, header.Value);
                }
            }
        }
    }
}