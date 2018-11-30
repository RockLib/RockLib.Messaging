using System.Collections.Generic;

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
            : base(() => namedPipeMessage.StringValue)
        {
            _namedPipeMessage = namedPipeMessage;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        protected override void AcknowledgeMessage() {}

        /// <summary>
        /// Does nothing.
        /// </summary>
        protected override void RollbackMessage() {}

        /// <summary>
        /// Does nothing.
        /// </summary>
        protected override void RejectMessage() {}
        
        /// <inheritdoc />
        protected override void InitializeHeaders(IDictionary<string, object> headers)
        {
            foreach (var header in _namedPipeMessage.Headers)
                headers.Add(header.Key, header.Value);
        }
    }
}