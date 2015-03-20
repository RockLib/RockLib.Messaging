using System;
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Threading;
using Rock.Serialization;

namespace Rock.Messaging.NamedPipes
{
    /// <summary>
    /// An implementation of <see cref="IReceiver"/> that uses named pipes as
    /// its communication mechanism.
    /// </summary>
    public class NamedPipeQueueConsumer : IReceiver
    {
        private readonly ISerializer _serializer;

        private readonly BlockingCollection<SentMessage> _messages = new BlockingCollection<SentMessage>();
        private readonly Thread _consumerThread;

        private readonly string _name;
        private readonly string _pipeName;

        private NamedPipeServerStream _pipeServer;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeQueueConsumer"/> class.
        /// </summary>
        /// <param name="name">The name of this instance of <see cref="NamedPipeQueueConsumer"/>.</param>
        /// <param name="pipeName">Name of the named pipe.</param>
        /// <param name="serializer">The serializer to use when receiving messages.</param>
        public NamedPipeQueueConsumer(string name, string pipeName, ISerializer serializer)
        {
            _name = name;
            _pipeName = pipeName;
            _serializer = serializer;
            _consumerThread = new Thread(Consume);
        }

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Gets the name of this instance of <see cref="IReceiver" />.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Starts listening for messages.
        /// </summary>
        /// <param name="selector">Also known as a 'routing key', this value enables only certain messages to be received.</param>
        public void Start(string selector)
        {
            if (_pipeServer == null)
            {
                StartNewPipeServer();
                _consumerThread.Start();
            }
        }

        private void StartNewPipeServer()
        {
            _pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.In, 254, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            _pipeServer.BeginWaitForConnection(WaitForConnectionCallBack, null);
        }

        private void WaitForConnectionCallBack(IAsyncResult result)
        {
            try
            {
                _pipeServer.EndWaitForConnection(result);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            try
            {
                var sentMessage = _serializer.DeserializeFromStream<SentMessage>(_pipeServer);

                if (sentMessage != null)
                {
                    _messages.Add(sentMessage);
                }
            }
            finally
            {
                try { _pipeServer.Close(); } // ReSharper disable once EmptyGeneralCatchClause
                catch { }
                StartNewPipeServer();
            }
        }

        private void Consume()
        {
            foreach (var sentMessage in _messages.GetConsumingEnumerable())
            {
                var handler = MessageReceived;
                if (handler != null)
                {
                    var message = new NamedPipeReceiverMessage(sentMessage);
                    handler(this, new MessageReceivedEventArgs(message));
                }
            }
        }

        public void Dispose()
        {
            if (_pipeServer != null)
            {
                _messages.CompleteAdding();
                try { _pipeServer.Close(); } // ReSharper disable once EmptyGeneralCatchClause
                catch { }
                try { _consumerThread.Join(); } // ReSharper disable once EmptyGeneralCatchClause
                catch { }
            }
        }
    }
}