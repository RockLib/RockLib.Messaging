# How to use and configure RockLib.Messaging.NamedPipes

See the [.NET Core example] or [.NET Framework example] for a complete demo application.

## NamedPipeSender

The NamedPipeSender class can be directly instantiated and has the following parameters:

- name
  - The name of the instance of NamedPipeSender.
- pipeName (optional)
  - Name of the named pipe. If not provided, the value of the `name` parameter is used.

MessagingScenarioFactory can be configured to use named pipes as follows:

```json
{
    "RockLib.Messaging": {
        "Senders": {
            "Type": "RockLib.Messaging.NamedPipes.NamedPipeSender, RockLib.Messaging.NamedPipes",
            "Value": {
                "Name": "commands",
                "PipeName": "my-pipe-name"
            }
        }
    }
}
```

```c#
// Note that implementations of the ISender interface are somewhat expensive and intended to be
// long-lived. They are thread-safe, so you can use a single instance throughout your application.
// Instances should be disposed before the application exits.

// MessagingScenarioFactory uses the above JSON configuration to create a NamedPipeSender:
ISender sender = MessagingScenarioFactory.CreateSender("commands");

// NamedPipeSender can also be instantiated directly:
// ISender sender = new NamedPipeSender("commands", "my-pipe-name");

// Use the sender (for good, not evil):
sender.Send("DROP DATABASE Production;");

// Always dispose the sender when done with it.
sender.Dispose();
```

## NamedPipeReceiver

The NamedPipeReceiver class can be directly instantiated and has the following parameters:

- name
  - The name of the instance of NamedPipeSender.
- pipeName (optional)
  - Name of the named pipe. If not provided, the value of the `name` parameter is used.

MessagingScenarioFactory can be configured to use named pipes as follows:

```json
{
    "RockLib.Messaging": {
        "Receivers": {
            "Type": "RockLib.Messaging.NamedPipes.NamedPipeReceiver, RockLib.Messaging.NamedPipes",
            "Value": {
                "Name": "commands",
                "PipeName": "my-pipe-name"
            }
        }
    }
}
```

```c#
// MessagingScenarioFactory uses the above JSON configuration to create a NamedPipeReceiver:
IReceiver receiver = MessagingScenarioFactory.CreateReceiver("commands");

// NamedPipeReceiver can also be instantiated directly:
// IReceiver receiver = new NamedPipeReceiver("commands", "my-pipe-name");

// Register to receive messages:
receiver.MessageReceived += OnMessageReceived;

// Start listening for messages:
receiver.Start();

// Wait for messages (demo code, don't judge)
Console.ReadLine();

// When finished listening, dispose the receiver.
receiver.Dispose();

void OnMessageReceived(object sender, MessageReceivedEventArgs args)
{
    Console.WriteLine(args.Message.StringPayload);
}
```

[.NET Core example]: https://github.com/RockLib/RockLib.Messaging/tree/master/Example.Messaging.NamedPipes.DotNetCore20
[.NET Framework example]: https://github.com/RockLib/RockLib.Messaging/tree/master/Example.Messaging.NamedPipes.DotNetFramework451