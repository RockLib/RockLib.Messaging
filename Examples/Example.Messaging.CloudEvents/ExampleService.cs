using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RockLib.Messaging;
using RockLib.Messaging.CloudEvents;
using RockLib.Messaging.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Example.Messaging.CloudEvents
{
    public class ExampleService : IHostedService
    {
        private readonly string _cloudEventSource;

        private readonly ISender _userPipeSender;
        private readonly IReceiver _userPipeReceiver;

        private readonly ISender _workerPipe1Sender;
        private readonly IReceiver _workerPipe1Receiver;

        private readonly ISender _workerPipe2Sender;
        private readonly IReceiver _workerPipe2Receiver;

        public ExampleService(SenderLookup senderLookup, ReceiverLookup receiverLookup, IOptions<ExampleOptions> options)
        {
            if (string.IsNullOrEmpty(options.Value.Source))
                throw new ArgumentException("Invalid ExampleOptions: Source must have non-empty value.", nameof(options));

            _cloudEventSource = options.Value.Source;

            _userPipeSender = senderLookup("user-pipe");
            _userPipeReceiver = receiverLookup("user-pipe");

            _workerPipe1Sender = senderLookup("worker-pipe-1");
            _workerPipe1Receiver = receiverLookup("worker-pipe-1");

            _workerPipe2Sender = senderLookup("worker-pipe-2");
            _workerPipe2Receiver = receiverLookup("worker-pipe-2");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _userPipeReceiver.Start<CorrelatedEvent>(OnUserEventReceivedAsync);
            _workerPipe1Receiver.Start<CorrelatedEvent>(OnWorker1EventReceivedAsync);
            _workerPipe2Receiver.Start<CorrelatedEvent>(OnWorker2EventReceivedAsync);
            ThreadPool.QueueUserWorkItem(UserThread);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _userPipeSender.Dispose();
            _userPipeReceiver.Dispose();

            _workerPipe1Sender.Dispose();
            _workerPipe2Sender.Dispose();

            _workerPipe1Receiver.Dispose();
            _workerPipe2Receiver.Dispose();

            return Task.CompletedTask;
        }

        private async void UserThread(object _)
        {
            Console.Write("User event data>");
            if (!(Console.ReadLine() is string input))
                return;

            Console.WriteLine();

            var cloudEvent = new CloudEvent()
            {
                Source = _cloudEventSource,
                Type = "example"
            }.SetData(input);

            await _userPipeSender.SendAsync(cloudEvent);
        }

        private async Task OnUserEventReceivedAsync(CorrelatedEvent userEvent, IReceiverMessage message)
        {
            Console.WriteLine($"Received user event: '{userEvent.StringData}' with correlation id: {userEvent.CorrelationId}");

            var worker1Event = userEvent.Copy();
            var worker2Event = userEvent.Copy();

            var middle = userEvent.StringData.Length / 2;

            worker1Event.SetData(userEvent.StringData[..middle]);
            worker2Event.SetData(userEvent.StringData[middle..]);

            await _workerPipe1Sender.SendAsync(worker1Event);
            await _workerPipe2Sender.SendAsync(worker2Event);

            await message.AcknowledgeAsync();
        }

        private async Task OnWorker1EventReceivedAsync(CorrelatedEvent worker1Event, IReceiverMessage message)
        {
            Console.WriteLine($"Received Worker 1 event: '{worker1Event.StringData}' with correlation id: {worker1Event.CorrelationId}");
            await message.AcknowledgeAsync();
        }

        private async Task OnWorker2EventReceivedAsync(CorrelatedEvent worker2Event, IReceiverMessage message)
        {
            Console.WriteLine($"Received Worker 2 event: '{worker2Event.StringData}' with correlation id: {worker2Event.CorrelationId}");
            await message.AcknowledgeAsync();
        }
    }
}
