using System;
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Threading;
#if NETSTANDARD1_6
using System.Threading.Tasks;
#endif

namespace RockLib.Messaging.NamedPipes
{
    /// <summary>
    /// An implementation of <see cref="IReceiver"/> that uses named pipes as
    /// its communication mechanism.
    /// </summary>
    public class NamedPipeReceiver : IReceiver
    {
        private readonly BlockingCollection<NamedPipeMessage> _messages = new BlockingCollection<NamedPipeMessage>();
        private readonly Thread _consumerThread;
        private readonly NamedPipeMessageSerializer _serializer = NamedPipeMessageSerializer.Instance;
        private NamedPipeServerStream _pipeServer;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeReceiver"/> class.
        /// </summary>
        /// <param name="name">The name of this instance of <see cref="NamedPipeReceiver"/>.</param>
        /// <param name="pipeName">Name of the named pipe.</param>
        public NamedPipeReceiver(string name, string pipeName = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PipeName = pipeName ?? Name;
            _consumerThread = new Thread(Consume);
        }

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Occurs when a connection is established.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Occurs when a connection is lost.
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Gets the name of this instance of <see cref="IReceiver" />.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the name of the named pipe.
        /// </summary>
        public string PipeName { get; }

        /// <summary>
        /// Starts listening for messages.
        /// </summary>
        /// <param name="selector">Also known as a 'routing key', this value enables only certain messages to be received.</param>
        public void Start(string selector = null)
        {
            if (_pipeServer == null)
            {
                StartNewPipeServer();
                _consumerThread.Start();
            }
        }

        private void StartNewPipeServer()
        {
            _pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.In, 254, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

#if !NETSTANDARD1_6
            _pipeServer.BeginWaitForConnection(WaitForConnectionCallBack, null);
            
#elif NETSTANDARD1_6
            Task.Run(async () =>
            {
                await _pipeServer.WaitForConnectionAsync();
                try
                {
                    WaitForConnectionCallBack(null);
                }
                catch (ObjectDisposedException) { }
            });
#endif
        }
        
        private void WaitForConnectionCallBack(IAsyncResult result)
        {
#if !NETSTANDARD1_6
            try
            {
                _pipeServer.EndWaitForConnection(result);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
#endif
            try
            {
                var sentMessage = _serializer.DeserializeFromStream<NamedPipeMessage>(_pipeServer);

                if (sentMessage != null)
                {
                    _messages.Add(sentMessage);
                }
            }
            finally
            {
                // docs say to call dispose vs close.  Dispose also works across standard/framework
                try { _pipeServer.Dispose(); } // ReSharper disable once EmptyGeneralCatchClause
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

        /// <summary>
        /// Closes and flushes the internal buffer of messages.
        /// </summary>
        public void Dispose()
        {
            if (_pipeServer != null)
            {
                _messages.CompleteAdding();
                // docs say to call dispose vs close.  Dispose also works across standard/framework
                try { _pipeServer.Dispose(); } // ReSharper disable once EmptyGeneralCatchClause
                catch { }
                try { _consumerThread.Join(); } // ReSharper disable once EmptyGeneralCatchClause
                catch { }
            }
        }
    }
}