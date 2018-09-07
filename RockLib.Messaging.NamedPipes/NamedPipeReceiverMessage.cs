using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public string StringPayload => _stringPayload.Value;

        public byte[] BinaryPayload => _binaryPayload.Value;

        public HeaderDictionary Headers { get; }

        public byte? Priority => null;

        public bool IsTransactional => false;

        public void Acknowledge() {}

        public void Rollback() {}
    }
}