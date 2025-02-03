using System;
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.NamedPipes
{
    /// <summary>
    /// An implementation of <see cref="IReceiver"/> that uses named pipes as
    /// its communication mechanism.
    /// </summary>
    public class NamedPipeReceiver : Receiver
    {
        private readonly BlockingCollection<NamedPipeMessage> _messages = new();
        private NamedPipeServerStream? _pipeServer;

        private bool _disposed;
        private Task? _consumer;
        private CancellationTokenSource _consumerCancellation;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeReceiver"/> class.
        /// </summary>
        /// <param name="name">The name of this instance of <see cref="NamedPipeReceiver"/>.</param>
        /// <param name="pipeName">Name of the named pipe.</param>
        public NamedPipeReceiver(string name, string? pipeName = null)
            : base(name)
        {
            PipeName = pipeName ?? name;
            _consumerCancellation = new CancellationTokenSource();
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
            if (_pipeServer is null)
            {
                StartNewPipeServer();
                _consumer = Task.Factory.StartNew(async () => await ConsumeAsync(_consumerCancellation.Token).ConfigureAwait(false),
                    _consumerCancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        private void StartNewPipeServer()
        {
#pragma warning disable CA1416 // Validate platform compatibility
            _pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.In, 254, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
#pragma warning restore CA1416 // Validate platform compatibility
            _pipeServer.BeginWaitForConnection(WaitForConnectionCallBack, null);
        }

        private void WaitForConnectionCallBack(IAsyncResult result)
        {
            // We can assume _pipeServer will always be non-null here.
            try
            {
                _pipeServer!.EndWaitForConnection(result);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                if (!_disposed)
                {
                    StartNewPipeServer();
                }
                return;
            }

            try
            {
                var sentMessage = NamedPipeMessageSerializer.DeserializeFromStream<NamedPipeMessage>(_pipeServer);

                if (sentMessage is not null)
                {
                    _messages.Add(sentMessage);
                }
            }
            finally
            {
                try 
                { 
                    _pipeServer.Dispose(); 
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch { }
#pragma warning restore CA1031 // Do not catch general exception types
                StartNewPipeServer();
            }
        }

        private async Task ConsumeAsync(CancellationToken cancellationToken)
        {
            try
            {
                foreach (var sentMessage in _messages.GetConsumingEnumerable(cancellationToken))
                {
                    try
                    {
#pragma warning disable CA2000 // Dispose objects before losing scope
                        await MessageHandler!.OnMessageReceivedAsync(this, new NamedPipeReceiverMessage(sentMessage)).ConfigureAwait(false);
#pragma warning restore CA2000 // Dispose objects before losing scope
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        OnError("Error in MessageHandler.OnMessageReceivedAsync.", ex);
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        /// <summary>
        /// Stop the pipe server and wait for the background thread to finish.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_pipeServer is not null)
            {
                _consumerCancellation.Cancel();

                _messages.CompleteAdding();
                _messages.Dispose();

                _pipeServer.Dispose();

                _consumerCancellation.Dispose();

                if(_consumer?.IsCompleted ?? false)
                {
                    _consumer?.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}