---
sidebar_position: 8
sidebar_label: 'Configure RockLib.Messaging.Kafka'
---

# How to use and configure RockLib.Messaging.Kafka

See the [.NET Core example] or [.NET Framework example] for a complete demo application.

## KafkaSender

The KafkaSender class can be directly instantiated in a few ways.

The first has the following parameters:

- name
  - The name of the instance of KafkaSender.
- topic
  - The topic to produce messages to.
- bootstrapServers
  - List of brokers as a CSV list of broker host or host:port.
- messageTimeoutMs (optional, defaults to 10000)
  - Local message timeout. This value is only enforced locally and limits the time a produced message waits for successful delivery. A time of 0 is infinite. This is the maximum time librdkafka may use to deliver a message (including retries). Delivery error occurs when either the retry count or the message timeout are exceeded.
- statisticsIntervalMs (optional, defaults 0)
  - The statistics emit interval

The second has the following parameters:

- name
  - The name of the instance of KafkaSender.
- topic
  - The topic to produce messages to.
- producerConfig
  - The configuration used in creation of the Kafka producer.

The third has the following parameters:

- name
  - The name of the instance of KafkaSender.
- topic
  - The topic to produce messages to.
- bootstrapServers
  - List of brokers as a CSV list of broker host or host:port.
- schemaId
  - Schema ID for broker to validate message schema against
- messageTimeoutMs (optional, defaults to 10000)
  - Local message timeout. This value is only enforced locally and limits the time a produced message waits for successful delivery. A time of 0 is infinite. This is the maximum time librdkafka may use to deliver a message (including retries). Delivery error occurs when either the retry count or the message timeout are exceeded.
- statisticsIntervalMs (optional, defaults 0)
  - The statistics emit interval

The fourth has the following parameters:

- name
  - The name of the instance of KafkaSender.
- topic
  - The topic to produce messages to.
- schemaId
  - Schema ID for broker to validate message schema against
- producerConfig
  - The configuration used in creation of the Kafka producer.

---

To add an KafkaSender to a service collection for dependency injection, use the `AddKafkaSender` method, optionally passing in a `configureOptions` callback:

```csharp
services.AddKafkaSender("MySender", options =>
{
    options.Topic = "test";
    options.BootstrapServers = "localhost:9092";
    options.MessageTimeoutMs = 10000;
});
```

To bind `KafkaSenderOptions` to a configuration section, use the name of the sender when calling the `Configure` method:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<KafkaSenderOptions>("MySender", Configuration.GetSection("MyKafkaSender"));
    services.AddKafkaSender("MySender");
}

/* appsettings.json:
{
    "MyKafkaSender": {
        "Topic": "test",
        "BootstrapServers": "localhost:9092",
        "MessageTimeoutMs": 10000,
        "SchemaId": 7890
    }
}
*/
```

---

MessagingScenarioFactory can be configured with an `KafkaSender` named "commands" as follows:

```json
{
    "RockLib.Messaging": {
        "Senders": {
            "Type": "RockLib.Messaging.Kafka.KafkaSender, RockLib.Messaging.Kafka",
            "Value": {
                "Name": "commands",
                "Topic": "test",
                "BootstrapServers": "localhost:9092",
                "MessageTimeoutMs": 10000,
                "SchemaId": 1234
            }
        }
    }
}
```

```csharp
// Note that implementations of the ISender interface are somewhat expensive and intended to be
// long-lived. They are thread-safe, so you can use a single instance throughout your application.
// Instances should be disposed before the application exits.

// MessagingScenarioFactory uses the above JSON configuration to create a KafkaSender
// Note that the Value object's properties in the json must map to a valid constructor since CreateSender Creates instances using [RockLib.Configuration.ObjectFactory](https://github.com/RockLib/RockLib.Configuration/tree/main/RockLib.Configuration.ObjectFactory#rocklibconfigurationobjectfactory)
ISender sender = MessagingScenarioFactory.CreateSender("commands");

// KafkaSender can also be instantiated directly:
// ISender sender = new KafkaSender("commands", "test", "localhost:9092", messageTimeoutMs: 10000);

// Use the sender (for good, not evil):
sender.Send("DROP DATABASE Production;");

// Always dispose the sender when done with it.
sender.Dispose();
```

## KafkaReceiver

The KafkaReceiver class can be directly instantiated in one of the two ways.

The first has the following parameters:

- name
  - The name of the instance of KafkaReceiver.
- topic
  - The topic to subscribe to. A regex can be specified to subscribe to the set of all matching topics (which is updated as topics are added / removed from the cluster). A regex must be front anchored to be recognized as a regex. e.g. `^myregex`
- groupId
  - Client group id string. All clients sharing the same group.id belong to the same group.
- bootstrapServers
  - List of brokers as a CSV list of broker host or host:port.
- enableAutoOffsetStore
  - Whether to automatically store offset of last message provided to application.
- autoOffsetReset
  - Action to take when there is no initial offset in offset store or the desired offset is out of range: 'smallest','earliest' - automatically reset the offset to the smallest offset, 'largest','latest' - automatically reset the offset to the largest offset, 'error' - trigger an error which is retrieved by consuming messages and checking 'message->err'.
- schemaIdRequired
  - Should the receiver expect schema information in the message payload
- statisticsIntervalMs (optional, defaults 0)
  - The statistics emit interval

And the second has the following parameters:

- name
  - The name of the instance of KafkaReceiver.
- topic
  - The topic to subscribe to. A regex can be specified to subscribe to the set of all matching topics (which is updated as topics are added / removed from the cluster). A regex must be front anchored to be recognized as a regex. e.g. `^myregex`
- consumerConfig
  - The configuration used in creation of the Kafka consumer.
- schemaIdRequired
  - Should the receiver expect schema information in the message payload

---

To add an KafkaReceiver to a service collection for dependency injection, use the `AddKafkaReceiver` method, optionally passing in a `configureOptions` callback:

```csharp
services.AddKafkaReceiver("MyReceiver", options =>
{
    options.Topic = "test";
    options.BootstrapServers = "localhost:9092";
    options.GroupId = "test-consumer-group";
});
```

To bind `KafkaReceiverOptions` to a configuration section, use the name of the receiver when calling the `Configure` method:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<KafkaReceiverOptions>("MyReceiver", Configuration.GetSection("MyKafkaReceiver"));
    services.AddKafkaReceiver("MyReceiver");
}

/* appsettings.json:
{
    "MyKafkaReceiver": {
        "Topic": "test",
        "BootstrapServers": "localhost:9092",
        "GroupId": "test-consumer-group"
    }
}
*/
```

---

MessagingScenarioFactory can be configured with an `KafkaReceiver` named "commands" as follows:

```json
{
    "RockLib.Messaging": {
        "Receivers": {
            "Type": "RockLib.Messaging.Kafka.KafkaReceiver, RockLib.Messaging.Kafka",
            "Value": {
                "Name": "commands",
                "Topic": "test",
                "GroupId": "test-consumer-group",
                "BootstrapServers": "localhost:9092",
                "SchemaIdRequired": true
            }
        }
    }
}
```

```csharp
// MessagingScenarioFactory uses the above JSON configuration to create a KafkaReceiver
// Note that the Value object's properties in the json must map to a valid constructor since CreateSender Creates instances using [RockLib.Configuration.ObjectFactory](https://github.com/RockLib/RockLib.Configuration/tree/main/RockLib.Configuration.ObjectFactory#rocklibconfigurationobjectfactory)
IReceiver receiver = MessagingScenarioFactory.CreateReceiver("commands");

// KafkaReceiver can also be instantiated directly:
// IReceiver receiver = new KafkaReceiver("commands", "test", "test-consumer-group", "localhost:9092");

// Start the receiver, passing in a lambda function callback to be invoked when a message is received.
receiver.Start(async message =>
{
    Console.WriteLine(message.StringPayload);
    Console.WriteLine(message.Headers[Constants.KafkaSchemaIdHeader]);

    // Always handle the message.
    await message.AcknowledgeAsync();
});

// Wait for messages (demo code, don't judge)
Console.ReadLine();

// When finished listening, dispose the receiver.
receiver.Dispose();
```

---

## Statistics
Both the `KafkaReceiver` and `KafkaSender` have a `StatisticsEmitted` event that is invoked when their underlying client emits statistics. 
This is disabled by default, but can be enabled by setting the `statisticsIntervalMs` parameter during instance construction. The statistics is a JSON formatted string as defined here:
[https://github.com/edenhill/librdkafka/blob/master/STATISTICS.md](https://github.com/edenhill/librdkafka/blob/master/STATISTICS.md).

```json
{
    "RockLib.Messaging": {
        "Receivers": {
            "Type": "RockLib.Messaging.Kafka.KafkaReceiver, RockLib.Messaging.Kafka",
            "Value": {
                "Name": "commands",
                "Topic": "test",
                "GroupId": "test-consumer-group",
                "BootstrapServers": "localhost:9092",
                "StatisticsIntervalMs": 60000
            }
        }
    }
}
```

```csharp
// MessagingScenarioFactory uses the above JSON configuration to create a KafkaReceiver:
IReceiver receiver = MessagingScenarioFactory.CreateReceiver("commands");

//StatisticsEmitted event handler
void OnStatisticsEmitted(object sender, string stats)
{
    Console.WriteLine($"Statistics emitted: {stats}");
}

//convenience extension method for attaching a handler
receiver.AddStatisticsEmittedHandler(OnStatisticsEmitted);
```

---

[.NET Core example]: ../Example.Messaging.Kafka.DotNetCore20
[.NET Framework example]: ../Example.Messaging.Kafka.DotNetFramework451
