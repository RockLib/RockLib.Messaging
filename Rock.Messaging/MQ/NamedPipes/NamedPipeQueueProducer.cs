using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

#if ROCKLIB
using RockLib.Messaging.Internal;
#else
using Rock.Messaging.Internal;
#endif

#if ROCKLIB
namespace RockLib.Messaging.NamedPipes
#else
namespace Rock.Messaging.NamedPipes
#endif
{
    /// <summary>
    /// An implementation of <see cref="ISender"/> that uses named pipes as
    /// its communication mechanism.
    /// </summary>
    public class NamedPipeQueueProducer : ISender
    {
        private static readonly Task _completedTask = Task.FromResult(0);

        //private readonly ISerializer _serializer = NamedPipeMessageSerializer.Instance;

        private readonly string _pipeName;
        private readonly bool _compressed;
        private readonly BlockingCollection<string> _messages;
        private readonly Thread _runThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeQueueProducer"/> class.
        /// </summary>
        /// <param name="name">The name of this instance of <see cref="NamedPipeQueueProducer"/>.</param>
        /// <param name="pipeName">Name of the named pipe.</param>
        /// <param name="compressed">Whether messages should be compressed.</param>
        public NamedPipeQueueProducer(string name, string pipeName, bool compressed)
        {
            Name = name;
            _pipeName = pipeName;
            _compressed = compressed;

            _messages = new BlockingCollection<string>();

            _runThread = new Thread(Run);
            _runThread.Start();
        }

        /// <summary>
        /// Gets the name of this instance of <see cref="ISender" />.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public Task SendAsync(ISenderMessage message)
        {
            var shouldCompress = message.ShouldCompress(_compressed);

            var stringValue = shouldCompress
                ? MessageCompression.Compress(message.StringValue)
                : message.StringValue;

            var namedPipeMessage = new NamedPipeMessage
            {
                StringValue = stringValue,
                MessageFormat = message.MessageFormat,
                Priority = message.Priority,
                Headers = new Dictionary<string, string>()
            };

            var originatingSystemAlreadyExists = false;

            foreach (var header in message.Headers)
            {
                if (header.Key == HeaderName.OriginatingSystem)
                {
                    originatingSystemAlreadyExists = true;
                }

                namedPipeMessage.Headers.Add(header.Key, header.Value);
            }

            namedPipeMessage.Headers[HeaderName.MessageFormat] = message.MessageFormat.ToString();

            if (!originatingSystemAlreadyExists)
            {
                namedPipeMessage.Headers[HeaderName.OriginatingSystem] = "NamedPipe";
            }

            if (shouldCompress)
            {
                namedPipeMessage.Headers[HeaderName.CompressedPayload] = "true";
            }

            var messageString = JsonConvert.SerializeObject(namedPipeMessage);
            _messages.Add(messageString);
            
            Trace.TraceError($"[Rock.Messaging.NamedPipeProducer] - [SendAsync] - sending message of {messageString}");

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