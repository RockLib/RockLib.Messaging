---
sidebar_position: 4
---

# How to use and configure RockLib.Messaging.NamedPipes

See the [.NET Core example] or [.NET Framework example] for a complete demo application.

## NamedPipeSender

The NamedPipeSender class can be directly instantiated and has the following parameters:

- name
  - The name of the instance of NamedPipeSender.
- pipeName (optional)
  - Name of the named pipe. If not provided, the value of the `name` parameter is used.

```csharp
ISender sender = new NamedPipeSender("MySender", "MyPipeName");
```

---

To add a NamedPipeSender to a service collection for dependency injection, use the `AddNamedPipeSender` method, optionally passing in a `configureOptions` callback:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddNamedPipeSender("MySender", options => options.PipeName = "MyPipeName");
}
```

To bind the `NamedPipeOptions` to a configuration section, use the name of the sender when calling the `Configure` method:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<NamedPipeOptions>("MySender", Configuration.GetSection("MyNamedPipe"));
    services.AddNamedPipeSender("MySender");
}

/* appsettings.json:
{
  "MyNamedPipe": {
    "PipeName": "MyPipeName"
  }
}
*/
```

---

MessagingScenarioFactory can be configured with a `NamedPipeSender` named "commands" as follows:

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

```csharp
// Note that implementations of the ISender interface are somewhat expensive and intended to be
// long-lived. They are thread-safe, so you can use a single instance throughout your application.
// Instances should be disposed before the application exits.

// MessagingScenarioFactory uses the above JSON configuration to create a NamedPipeSender
// Note that the Value object's properties in the json must map to a valid constructor since CreateSender Creates instances using [RockLib.Configuration.ObjectFactory](https://github.com/RockLib/RockLib.Configuration/tree/main/RockLib.Configuration.ObjectFactory#rocklibconfigurationobjectfactory)
ISender sender = MessagingScenarioFactory.CreateSender("commands");

// Use the sender (for good, not evil):
sender.Send("DROP DATABASE Production;");

// Always dispose the sender when done with it.
sender.Dispose();
```

## NamedPipeReceiver

The NamedPipeReceiver class can be directly instantiated and has the following parameters:

- name
  - The name of the instance of NamedPipeReceiver.
- pipeName (optional)
  - Name of the named pipe. If not provided, the value of the `name` parameter is used.

```csharp
IReceiver receiver = new NamedPipeReceiver("MyReceiver", "MyPipeName");
```

---

To add a NamedPipeSender to a service collection for dependency injection, use the `AddNamedPipeSender` method, optionally passing in a `configureOptions` callback:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddNamedPipeReceiver("MyReceiver", options => options.PipeName = "MyPipeName");
}
```

To bind the `NamedPipeOptions` to a configuration section, use the name of the sender when calling the `Configure` method:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<NamedPipeOptions>("MyReceiver", Configuration.GetSection("MyNamedPipe"));
    services.AddNamedPipeReceiver("MyReceiver");
}

/* appsettings.json:
{
  "MyNamedPipe": {
    "PipeName": "MyPipeName"
  }
}
*/
```

---

MessagingScenarioFactory can be configured with a `NamedPipeReceiver` named "commands" as follows:

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

```csharp
// MessagingScenarioFactory uses the above JSON configuration to create a NamedPipeReceiver
// Note that the Value object's properties in the json must map to a valid constructor since CreateSender Creates instances using [RockLib.Configuration.ObjectFactory](https://github.com/RockLib/RockLib.Configuration/tree/main/RockLib.Configuration.ObjectFactory#rocklibconfigurationobjectfactory)
IReceiver receiver = MessagingScenarioFactory.CreateReceiver("commands");

// Start the receiver, passing in a lambda function callback to be invoked when a message is received.
receiver.Start(async message =>
{
    Console.WriteLine(message.StringPayload);
    
    // Since AutoAcknowledge is false in this example, the message must be acknowledged.
    await message.AcknowledgeAsync();
});

// Wait for messages (demo code, don't judge)
Console.ReadLine();

// When finished listening, dispose the receiver.
receiver.Dispose();
```

[.NET Core example]: ../Example.Messaging.NamedPipes.DotNetCore20
[.NET Framework example]: ../Example.Messaging.NamedPipes.DotNetFramework451
