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
    public class NamedPipeSender : ISender
    {
        private readonly NamedPipeMessageSerializer _serializer = NamedPipeMessageSerializer.Instance;
        private static readonly Task _completedTask = Task.FromResult(0);
        private readonly BlockingCollection<WorkItem> _workItems;
        private readonly Thread _runThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeSender"/> class.
        /// </summary>
        /// <param name="name">The name of this instance of <see cref="NamedPipeSender"/>.</param>
        /// <param name="pipeName">Name of the named pipe.</param>
        public NamedPipeSender(string name, string pipeName = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PipeName = pipeName ?? Name;

            _workItems = new BlockingCollection<WorkItem>();

            _runThread = new Thread(Run);
            _runThread.Start();
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
        public Task SendAsync(SenderMessage message)
        {
            if (message.OriginatingSystem == null)
                message.OriginatingSystem = "NamedPipe";

            var namedPipeMessage = new NamedPipeMessage
            {
                StringValue = message.StringPayload,
                Headers = new Dictionary<string, string>()
            };

            foreach (var header in message.Headers)
                namedPipeMessage.Headers.Add(header.Key, header.Value.ToString());

            var messageString = _serializer.SerializeToString(namedPipeMessage);
            var completion = new TaskCompletionSource<bool>();

            _workItems.Add(new WorkItem { Message = messageString, Completion = completion });

            return completion.Task;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _workItems.CompleteAdding();
            _runThread.Join();
        }

        private void Run()
        {
            foreach (var workItem in _workItems.GetConsumingEnumerable())
            {
                try
                {
                    var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.Asynchronous);

                    try
                    {
                        pipe.Connect(0);
                    }
                    catch (TimeoutException ex)
                    {
                        workItem.Completion.SetException(ex);
                        continue;
                    }

                    using (var writer = new StreamWriter(pipe))
                    {
                        writer.WriteLine(workItem.Message);
                    }

                    workItem.Completion.SetResult(true);
                }
                catch (Exception ex)
                {
                    workItem.Completion.SetException(ex);
                    continue;
                }
            }
        }

        private struct WorkItem
        {
            public string Message;
            public TaskCompletionSource<bool> Completion;
        }
    }
}