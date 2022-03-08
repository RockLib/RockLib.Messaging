using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.NamedPipes
{
    /// <summary>
    /// An implementation of <see cref="ISender"/> that uses named pipes as
    /// its communication mechanism.
    /// </summary>
#pragma warning disable CA1063 // Implement IDisposable Correctly
    public class NamedPipeSender : ISender
#pragma warning restore CA1063 // Implement IDisposable Correctly
    {
        private readonly BlockingCollection<string> _workItems;
        private Task _sender;
        private CancellationTokenSource _senderCancellation;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeSender"/> class.
        /// </summary>
        /// <param name="name">The name of this instance of <see cref="NamedPipeSender"/>.</param>
        /// <param name="pipeName">Name of the named pipe.</param>
        public NamedPipeSender(string name, string? pipeName = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PipeName = pipeName ?? Name;

            _workItems = new BlockingCollection<string>();
            _senderCancellation = new CancellationTokenSource();
            _sender = Task.Factory.StartNew(() => Run(_senderCancellation.Token), _senderCancellation.Token, 
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>
        /// Gets the name of this instance of <see cref="ISender" />.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the name of the named pipe.
        /// </summary>
        public string PipeName { get; }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public Task SendAsync(SenderMessage message, CancellationToken cancellationToken)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.OriginatingSystem is null)
            {
                message.OriginatingSystem = "NamedPipe";
            }

            var namedPipeMessage = new NamedPipeMessage
            {
                StringValue = message.StringPayload,
                Headers = new Dictionary<string, string>()
            };

            foreach (var header in message.Headers)
            {
                namedPipeMessage.Headers.Add(header.Key, header.Value.ToString()!);
            }

            var messageString = NamedPipeMessageSerializer.SerializeToString(namedPipeMessage);

            _workItems.Add(messageString, cancellationToken);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
#pragma warning restore CA1063 // Implement IDisposable Correctly
        {
            _senderCancellation.Cancel();

            _workItems.CompleteAdding();
            _workItems.Dispose();

            _senderCancellation.Dispose();

            if(_sender.IsCompleted)
            {
                _sender.Dispose();
            }
        }

        private void Run(CancellationToken token)
        {
            try
            {
                foreach (var workItem in _workItems.GetConsumingEnumerable(token))
                {

                    using var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.Asynchronous);

                    try
                    {
                        pipe.Connect(0);
                    }
                    catch (TimeoutException)
                    {
                        continue;
                    }

                    using var writer = new StreamWriter(pipe);
                    writer.WriteLine(workItem);
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}