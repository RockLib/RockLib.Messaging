using System;
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Threading;

namespace RockLib.Messaging.NamedPipes
{
    /// <summary>
    /// An implementation of <see cref="IReceiver"/> that uses named pipes as
    /// its communication mechanism.
    /// </summary>
    public class NamedPipeReceiver : Receiver
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
            : base(name)
        {
            PipeName = pipeName ?? name;
            _consumerThread = new Thread(Consume);
        }

        /// <summary>
        /// Gets the name of the named pipe.
        /// </summary>
        public string PipeName { get; }

        /// <summary>
        /// Starts a new pipe server and the consumer background thread.
        /// </summary>
        protected override void Start()
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
                MessageHandler.OnMessageReceived(this, new NamedPipeReceiverMessage(sentMessage));
        }

        /// <summary>
        /// Stop the pipe server and wait for the background thread to finish.
        /// </summary>
        protected override void Dispose(bool disposing)
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

            base.Dispose(disposing);
        }
    }
}