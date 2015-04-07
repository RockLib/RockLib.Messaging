using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Rock.Serialization;

namespace Rock.Messaging.NamedPipes
{
    /// <summary>
    /// An implementation of <see cref="ISender"/> that uses named pipes as
    /// its communication mechanism.
    /// </summary>
    public class NamedPipeQueueProducer : ISender
    {
        private static readonly Task _completedTask = Task.FromResult(0);

        private readonly string _name;
        private readonly string _pipeName;
        private readonly ISerializer _serializer;
        private readonly BlockingCollection<string> _messages;
        private readonly Thread _runThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeQueueProducer"/> class.
        /// </summary>
        /// <param name="name">The name of this instance of <see cref="NamedPipeQueueProducer"/>.</param>
        /// <param name="pipeName">Name of the named pipe.</param>
        /// <param name="serializer">The serializer to use when sending messages.</param>
        public NamedPipeQueueProducer(string name, string pipeName, ISerializer serializer)
        {
            _name = name;
            _pipeName = pipeName;
            _serializer = serializer;

            _messages = new BlockingCollection<string>();

            _runThread = new Thread(Run);
            _runThread.Start();
        }

        /// <summary>
        /// Gets the name of this instance of <see cref="ISender" />.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public Task SendAsync(ISenderMessage message)
        {
            var messageString = _serializer.SerializeToString(message);
            _messages.Add(messageString);
            return _completedTask;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _messages.CompleteAdding();
            _runThread.Join();
        }

        private void Run()
        {
            foreach (var message in _messages.GetConsumingEnumerable())
            {
                try
                {
                    var pipe = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out, PipeOptions.Asynchronous);

                    try
                    {
                        pipe.Connect(0);
                    }
                    catch (TimeoutException)
                    {
                        continue;
                    }

                    using (var writer = new StreamWriter(pipe))
                    {
                        writer.WriteLine(message);
                    }
                }
                catch (Exception ex)
                {
                    // TODO: Something?
                    continue;
                }
            }
        }
    }
}