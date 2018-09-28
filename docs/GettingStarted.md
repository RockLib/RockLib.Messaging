# Getting Started

RockLib.Messaging is a simple API that allows you to send and receive messages. In this tutorial, we will be building a pair of console applications that send messages to each other using a [named pipe](https://en.wikipedia.org/wiki/Named_pipe).

---

Create two .NET Core 2.0 (or above) console applications named "SenderApp" and "ReceiverApp".

---

Add a nuget reference for "RockLib.Messaging.NamedPipes" to each project.

---

Add a new JSON file to each project named 'appsettings.json'. Set its 'Copy to Output Directory' setting to 'Copy always'. Add the following configuration to each file:

```json
{
  "RockLib.Messaging": {
    "Type": "RockLib.Messaging.NamedPipes.NamedPipeMessagingScenarioFactory, RockLib.Messaging",
    "Value": {
      "NamedPipeConfigs": {
        "Name": "Example",
        "PipeName": "example_pipe"
      }
    }
  }
}
```

---

Edit the `Program.cs` file in the SenderApp project as follows:

```c#
using RockLib.Messaging;
using System;

namespace SenderApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ISender sender = MessagingScenarioFactory.CreateQueueProducer("Example"))
            {
                Console.WriteLine($"Enter a message for sender '{sender.Name}'. Leave blank to quit.");
                string message;
                while (true)
                {
                    Console.Write("message>");
                    if ((message = Console.ReadLine()) == "")
                        return;
                    sender.Send(message);
                }
            }
        }
    }
}
```

---

Edit the `Program.cs` file in the ReceiverApp project as follows:

```c#
using RockLib.Messaging;
using System;

namespace ReceiverApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using (IReceiver receiver = MessagingScenarioFactory.CreateQueueConsumer("Example"))
            {
                receiver.MessageReceived += (s, e) => Console.WriteLine(e.Message.GetStringValue());
                Console.WriteLine($"Receiving messages from receiver '{receiver.Name}'. Press <enter> to quit.");
                receiver.Start();
                while (Console.ReadKey(true).Key != ConsoleKey.Enter) {}
            }
        }
    }
}
```

---

Start both apps. SenderApp will receive input from the user, send it to the named pipe, then exit. ReceiverApp will listen to the named pipe, display any received messages, wait for the user to press any key, then exit.