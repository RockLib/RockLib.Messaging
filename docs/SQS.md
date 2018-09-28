# How to use and configure RockLib.Messaging.SQS

See the [.NET Core example] or [.NET Framework example] for a complete demo application.

## SQSQueueSender

The SQSQueueSender class can be directly instantiated and has the following parameters:

- name
  - The name of the instance of SQSQueueSender.
- queueUrl
  - The url of the SQS queue.

MessagingScenarioFactory can be configured to use SQS as follows:

```json
{
    "RockLib.Messaging": {
        "Senders": {
            "Type": "RockLib.Messaging.SQS.SQSQueueSender, RockLib.Messaging.SQS",
            "Value": {
                "Name": "commands",
                "QueueUrl": "https://sqs.us-west-2.amazonaws.com/123456789012/your_queue_name"
            }
        }
    }
}
```

```c#
// Note that implementations of the ISender interface are somewhat expensive and intended to be
// long-lived. They are thread-safe, so you can use a single instance throughout your application.
// Instances should be disposed before the application exits.

// MessagingScenarioFactory uses the above JSON configuration to create a SQSQueueSender:
ISender sender = MessagingScenarioFactory.CreateSender("commands");

// SQSQueueSender can also be instantiated directly:
// ISender sender = new SQSQueueSender("commands", "https://sqs.us-west-2.amazonaws.com/123456789012/your_queue_name");

// Use the sender (for good, not evil):
sender.Send("DROP DATABASE Production;");

// Always dispose the sender when done with it.
sender.Dispose();
```

## SQSQueueReceiver

The SQSQueueReceiver class can be directly instantiated and has the following parameters:

- name
  - The name of the instance of SQSQueueSender.
- queueUrl
  - The url of the SQS queue.
- maxMessages (optional, defaults to 3)
  - The maximum number of messages to return with each call to the SQS endpoint. Amazon SQS never returns more messages than this value (however, fewer messages might be returned). Valid values are 1 to 10.
- autoAcknowledge (optional, defaults to true)
  - Whether messages will be automatically acknowledged after any event handlers execute.
- parallelHandling (optional, defaults to false)
  - Whether, in the case of when multiple messages are received from an SQS request, messages are handled in parallel or sequentially.

MessagingScenarioFactory can be configured to use SQS as follows:

```json
{
    "RockLib.Messaging": {
        "Receivers": {
            "Type": "RockLib.Messaging.SQS.SQSQueueReceiver, RockLib.Messaging.SQS",
            "Value": {
                "Name": "commands",
                "QueueUrl": "https://sqs.us-west-2.amazonaws.com/123456789012/your_queue_name",
                "MaxMessages": 3,
                "AutoAcknowledge": false,
                "ParallelHandling": false
            }
        }
    }
}
```

```c#
// MessagingScenarioFactory uses the above JSON configuration to create a SQSQueueReceiver:
IReceiver receiver = MessagingScenarioFactory.CreateReceiver("commands");

// SQSQueueReceiver can also be instantiated directly:
// IReceiver receiver = new SQSQueueReceiver("commands", "https://sqs.us-west-2.amazonaws.com/123456789012/your_queue_name",
//     maxMessages: 3, autoAcknowledge: false, parallelHandling: false);

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
    
    // Since AutoAcknowledge is false in this example, the message must be acknowledged.
    args.Message.Acknowledge();
}
```

[.NET Core example]: https://github.com/RockLib/RockLib.Messaging/tree/master/Example.Messaging.SQS.DotNetCore20
[.NET Framework example]: https://github.com/RockLib/RockLib.Messaging/tree/master/Example.Messaging.SQS.DotNetFramework451