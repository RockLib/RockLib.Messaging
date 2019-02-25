# How to use and configure RockLib.Messaging.Kafka

See the [.NET Core example] or [.NET Framework example] for a complete demo application.

## KafkaSender

The KafkaSender class can be directly instantiated and has the following parameters:

- name
  - The name of the instance of KafkaSender.
- topic
  - The topic to produce messages to.
- bootstrapServers
  - Initial list of brokers as a CSV list of broker host or host:port.
- messageTimeoutMs (optional, defaults to 10000)
  - Local message timeout. This value is only enforced locally and limits the time a produced message waits for successful delivery. A time of 0 is infinite. This is the maximum time librdkafka may use to deliver a message (including retries). Delivery error occurs when either the retry count or the message timeout are exceeded.
- config (optional, defaults to null)
  - A collection of librdkafka configuration parameters (refer to https://github.com/edenhill/librdkafka/blob/master/CONFIGURATION.md) and parameters specific to this client (refer to: Confluent.Kafka.ConfigPropertyNames).

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
                "Config": {
                    "SocketNagleDisable": true
                }
            }
        }
    }
}
```

```c#
// Note that implementations of the ISender interface are somewhat expensive and intended to be
// long-lived. They are thread-safe, so you can use a single instance throughout your application.
// Instances should be disposed before the application exits.

// MessagingScenarioFactory uses the above JSON configuration to create a KafkaSender:
ISender sender = MessagingScenarioFactory.CreateSender("commands");

// KafkaSender can also be instantiated directly:
// ISender sender = new KafkaSender("commands", "test", "localhost:9092",
//     useBeginProduce: true, config: new ProducerConfig { SocketNagleDisable = true });

// Use the sender (for good, not evil):
sender.Send("DROP DATABASE Production;");

// Always dispose the sender when done with it.
sender.Dispose();
```

## KafkaReceiver

The KafkaReceiver class can be directly instantiated and has the following parameters:

- name
  - The name of the instance of KafkaReceiver.
- topic
  - The topic to subscribe to. A regex can be specified to subscribe to the set of all matching topics (which is updated as topics are added / removed from the cluster). A regex must be front anchored to be recognized as a regex. e.g. `^myregex`
- groupId
  - Client group id string. All clients sharing the same group.id belong to the same group.
- bootstrapServers
  - List of brokers as a CSV list of broker host or host:port.
- config
  - A collection of librdkafka configuration parameters (refer to https://github.com/edenhill/librdkafka/blob/master/CONFIGURATION.md) and parameters specific to this client (refer to: Confluent.Kafka.ConfigPropertyNames).

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
                "Config": {
                    "SocketNagleDisable": true
                }
            }
        }
    }
}
```

```c#
// MessagingScenarioFactory uses the above JSON configuration to create a KafkaReceiver:
IReceiver receiver = MessagingScenarioFactory.CreateReceiver("commands");

// KafkaReceiver can also be instantiated directly:
// IReceiver receiver = new KafkaReceiver("commands", "test", "test-consumer-group", "localhost:9092",
//     config: new ConsumerConfig { SocketNagleDisable = true });

// Start the receiver, passing in a lambda function callback to be invoked when a message is received.
receiver.Start(message =>
{
    Console.WriteLine(message.StringPayload);
    
    // Since AutoAcknowledge is false in this example, the message must be acknowledged.
    message.Acknowledge();
});

// Wait for messages (demo code, don't judge)
Console.ReadLine();

// When finished listening, dispose the receiver.
receiver.Dispose();
```

---

[.NET Core example]: ../Example.Messaging.Kafka.DotNetCore20
[.NET Framework example]: ../Example.Messaging.Kafka.DotNetFramework451