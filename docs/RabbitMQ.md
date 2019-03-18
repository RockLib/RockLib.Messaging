# How to use and configure RockLib.Messaging.RabbitMQ

See the [.NET Core example] or [.NET Framework example] for a complete demo application.

## RabbitSender

The RabbitSender class can be directly instantiated and has the following parameters:

- name
  - The name of the instance of RabbitSender.
- connection
  - A factory that will create the RabbitMQ connection. Its 'HostName' property is required.
- exchange (optional)
  - The exchange to use when publishing messages.
- routingKey (optional)
  - The routing key to use when publishing messages.
- routingKeyHeaderName (optional)
  - The name of the header that contains the routing key to use when publishing messages. Each message sent that has a header with this name will be sent with a routing key of the header value.
- persistent (optional, defaults to true)
  - Whether the RabbitMQ server should save the message to disk upon receipt.

MessagingScenarioFactory can be configured with an `RabbitSender` named "commands" as follows:

```json
{
    "RockLib.Messaging": {
        "Senders": {
            "Type": "RockLib.Messaging.RabbitMQ.RabbitSender, RockLib.Messaging.RabbitMQ",
            "Value": {
                "Name": "commands",
                "Connection": { "HostName": "localhost" },
                "Exchange": "wopr",
                "RoutingKey": "chess",
                "RoutingKeyHeaderName": "bindingKey",
                "Persistent": false
            }
        }
    }
}
```

```c#
// Note that implementations of the ISender interface are somewhat expensive and intended to be
// long-lived. They are thread-safe, so you can use a single instance throughout your application.
// Instances should be disposed before the application exits.

// MessagingScenarioFactory uses the above JSON configuration to create a RabbitSender:
ISender sender = MessagingScenarioFactory.CreateSender("commands");

// RabbitSender can also be instantiated directly:
// ISender sender = new RabbitSender("commands",
//     new ConnectionFactory { HostName = "localhost" },
//     exchange: "wopr", routingKey: "chess",
//     routingKeyHeaderName: "bindingKey", persistent: false);

// Since 'RoutingKey' is set, the message will be sent with a routing key of "chess".
await sender.SendAsync("Shall we play a game?");

// Since 'RoutingKeyHeaderName' is set, we can override the routing key for a
// specific message by adding a header with a name equal to the value of the
// 'RoutingKeyHeaderName' setting (in this case, "bindingKey").
SenderMessage message = new SenderMessage("Shall we play a game?");
message.Headers.Add("bindingKey", "global.thermonuclear.war");
await sender.SendAsync(message);

// Always dispose the sender when done with it.
sender.Dispose();
```

## RabbitReceiver

The RabbitReceiver class can be directly instantiated and has the following parameters:

- name
  - The name of the instance of RabbitReceiver.
- connection
  - A factory that will create the RabbitMQ connection. Its 'HostName' property is required.
- queueName
  - The name of the queue to receive messages from. If null and `exchange` is *not* null, then a non-durable, exclusive, autodelete queue with a generated name is created, and the generated name is used as the queue name.
- exchange
  - The name of the exchange to bind the queue to. If null, the queue is not bound to an exchange and the `routingKeys` parameter (if provided) is ignored.
- routingKeys
  - The collection of routing keys used to bind the queue and exchange. Ignored if the `exchange` parameter is null.
- prefetchCount
  - The maximum number of messages that the server will deliver to the channel before being acknowledged.
- autoAck
  - Whether messages should be received in an already-acknowledged state. If true, messages cannot be rolled back.

MessagingScenarioFactory can be configured with an `RabbitReceiver` named "commands" as follows:

```json
{
    "RockLib.Messaging": {
        "Receivers": {
            "Type": "RockLib.Messaging.RabbitMQ.RabbitReceiver, RockLib.Messaging.RabbitMQ",
            "Value": {
                "Name": "commands",
                "Connection": { "HostName": "localhost" },
                "Queue": "my_queue_name",
                "Exchange": "my_exchange",
                "RoutingKeys": [ "my", "routing", "keys" ],
                "PrefetchCount": 10,
                "AutoAck": "false"
            }
        }
    }
}
```

```c#
// MessagingScenarioFactory uses the above JSON configuration to create a RabbitReceiver:
IReceiver receiver = MessagingScenarioFactory.CreateReceiver("commands");

// RabbitReceiver can also be instantiated directly:
// IReceiver receiver = new RabbitReceiver("commands",
//     new ConnectionFactory { HostName = "localhost" },
//     queue: "my_queue_name", exchange: "my_exchange",
//     routingKeys: new[] { "my", "routing", "keys" },
//     prefetchCount: 10, autoAck: false);

// Start the receiver, passing in a lambda function callback to be invoked when a message is received.
receiver.Start(async message =>
{
    Console.WriteLine(message.StringPayload);

    // Always handle the message.
    await message.AcknowledgeAsync();
});

// Wait for messages (demo code, don't judge)
Console.ReadLine();

// When finished listening, dispose the receiver.
receiver.Dispose();
```

---

[.NET Core example]: ../Example.Messaging.RabbitMQ.DotNetCore20
[.NET Framework example]: ../Example.Messaging.RabbitMQ.DotNetFramework451