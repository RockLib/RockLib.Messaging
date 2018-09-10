using System;
using System.Collections.Generic;
using RockLib.Messaging.ImplementationHelpers;

namespace RockLib.Messaging.NamedPipes
{
    /// <summary>
    /// An implementation of IReceiverMessage for use by the <see cref="NamedPipeReceiver"/>
    /// class.
    /// </summary>
    public class NamedPipeReceiverMessage : IReceiverMessage
    {
        private readonly Lazy<string> _stringPayload;
        private readonly Lazy<byte[]> _binaryPayload;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeReceiverMessage"/> class.
        /// </summary>
        /// <param name="namedPipeMessage">The message that was sent.</param>
        internal NamedPipeReceiverMessage(NamedPipeMessage namedPipeMessage)
        {
            var headers = new Dictionary<string, object>();
            foreach (var header in namedPipeMessage.Headers)
                headers.Add(header.Key, header.Value);
            Headers = new HeaderDictionary(headers);
            this.SetLazyPayloadFields(namedPipeMessage.StringValue, out _stringPayload, out _binaryPayload);
        }

        /// <summary>
        /// Gets the payload of the message as a string.
        /// </summary>
        public string StringPayload => _stringPayload.Value;

        /// <summary>
        /// Gets the payload of the message as a byte array.
        /// </summary>
        public byte[] BinaryPayload => _binaryPayload.Value;

        /// <summary>
        /// Gets the headers of the message.
        /// </summary>
        public HeaderDictionary Headers { get; }

        /// <summary>
        /// Returns null.
        /// </summary>
        public byte? Priority => null;

        /// <summary>
        /// Returns false.
        /// </summary>
        public bool IsTransactional => false;

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Acknowledge() {}

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Rollback() {}
    }
}