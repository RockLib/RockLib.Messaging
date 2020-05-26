# Getting Started

RockLib.Messaging is a simple API that allows you to send and receive messages. In this tutorial, we will be building a pair of console applications that send messages to each other using a [named pipe](https://en.wikipedia.org/wiki/Named_pipe).

---

Create two .NET Core 2.0 (or above) console applications named "SenderApp" and "ReceiverApp".

---

Add a nuget references for "RockLib.Messaging.NamedPipes" and "Microsoft.Extensions.Hosting" to each project.

---

Add a class named 'SendingService' to the SenderApp project and replace its contents with the following:

```c#
using Microsoft.Extensions.Hosting;
using RockLib.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SenderApp
{
    public class SendingService : IHostedService
    {
        private readonly ISender _sender;

        public SendingService(ISender sender)
        {
            _sender = sender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            ThreadPool.QueueUserWorkItem(SendMessages);
            return Task.CompletedTask;
        }

        private void SendMessages(object state)
        {
            Console.WriteLine($"Enter messages for sender '{_sender.Name}'.");
            string message;
            while (true)
            {
                Console.Write(">");
                if ((message = Console.ReadLine()) == null)
                    return;
                _sender.Send(message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _sender.Dispose();
            return Task.CompletedTask;
        }
    }
}
```

---

Add a class named 'ReceivingService' to the ReceiverApp project and replace its contents with the following:

```c#
using Microsoft.Extensions.Hosting;
using RockLib.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReceiverApp
{
    public class ReceivingService : IHostedService
    {
        private readonly IReceiver _receiver;

        public ReceivingService(IReceiver receiver)
        {
            _receiver = receiver;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _receiver.Start(OnMessageReceived);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _receiver.Dispose();
            return Task.CompletedTask;
        }

        private async Task OnMessageReceived(IReceiverMessage message)
        {
            Console.WriteLine(message.StringPayload);
            await message.AcknowledgeAsync();
        }
    }
}
```

---

Edit the `Program.cs` file in the SenderApp project as follows:

```c#
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RockLib.Messaging.DependencyInjection;

namespace SenderApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddNamedPipeSender("MySender", options => options.PipeName = "ExamplePipe");
                    services.AddHostedService<SendingService>();
                });
        }
    }
}
```

---

Edit the `Program.cs` file in the ReceiverApp project as follows:

```c#
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RockLib.Messaging.DependencyInjection;

namespace ReceiverApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddNamedPipeReceiver("MyReceiver", options => options.PipeName = "ExamplePipe");
                    services.AddHostedService<ReceivingService>();
                });
        }
    }
}
```

---

Start both apps. SenderApp will receive input from the user and send it to the named pipe. ReceiverApp will listen to the named pipe and display any received messages. Press Ctrl+C to exit each app.