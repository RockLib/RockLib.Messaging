using System;
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
        /// Returns null.
        /// </summary>
        public override byte? Priority => null;

        /// <summary>
        /// Does nothing.
        /// </summary>
        public override void Acknowledge() {}

        /// <summary>
        /// Does nothing.
        /// </summary>
        public override void Rollback() {}
        
        /// <inheritdoc />
        protected override void InitializeHeaders(IDictionary<string, object> headers)
        {
            foreach (var header in _namedPipeMessage.Headers)
                headers.Add(header.Key, header.Value);
        }
    }
}