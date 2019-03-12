# How to use and configure RockLib.Messaging.SQS

See the [.NET Core example] or [.NET Framework example] for a complete demo application.

## SQSSender

The SQSSender class can be directly instantiated and has the following parameters:

- name
  - The name of the instance of SQSSender.
- queueUrl
  - The url of the SQS queue.
- region (optional, defaults to the null)
  - The region of the SQS queue.

MessagingScenarioFactory can be configured with an `SQSSender` named "commands" as follows:

```json
{
    "RockLib.Messaging": {
        "Senders": {
            "Type": "RockLib.Messaging.SQS.SQSSender, RockLib.Messaging.SQS",
            "Value": {
                "Name": "commands",
                "QueueUrl": "https://sqs.us-west-2.amazonaws.com/123456789012/your_queue_name",
                "Region": "us-west-2"
            }
        }
    }
}
```

```c#
// Note that implementations of the ISender interface are somewhat expensive and intended to be
// long-lived. They are thread-safe, so you can use a single instance throughout your application.
// Instances should be disposed before the application exits.

// MessagingScenarioFactory uses the above JSON configuration to create a SQSSender:
ISender sender = MessagingScenarioFactory.CreateSender("commands");

// SQSSender can also be instantiated directly:
// ISender sender = new SQSSender("commands", "https://sqs.us-west-2.amazonaws.com/123456789012/your_queue_name",
//     region: "us-west-2");

// Use the sender (for good, not evil):
sender.Send("DROP DATABASE Production;");

// Always dispose the sender when done with it.
sender.Dispose();
```

## SQSReceiver

The SQSReceiver class can be directly instantiated and has the following parameters:

- name
  - The name of the instance of SQSReceiver.
- queueUrl
  - The url of the SQS queue.
- region (optional, defaults to the null)
  - The region of the SQS queue.
- maxMessages (optional, defaults to 3)
  - The maximum number of messages to return with each call to the SQS endpoint. Amazon SQS never returns more messages than this value (however, fewer messages might be returned). Valid values are 1 to 10.
- autoAcknowledge (optional, defaults to true)
  - Whether messages will be automatically acknowledged after the message handler executes.
- waitTimeSeconds (optional, defaults to 0)
  - The duration (in seconds) for which calls to ReceiveMessage wait for a message to arrive in the queue before returning. If a message is available, the call returns sooner than WaitTimeSeconds. If no messages are available and the wait time expires, the call returns successfully with an empty list of messages.
- unpackSNS (optional, defaults to false)
  - Whether to attempt to unpack the message body as an SNS message.

MessagingScenarioFactory can be configured with an `SQSReceiver` named "commands" as follows:

```json
{
    "RockLib.Messaging": {
        "Receivers": {
            "Type": "RockLib.Messaging.SQS.SQSReceiver, RockLib.Messaging.SQS",
            "Value": {
                "Name": "commands",
                "QueueUrl": "https://sqs.us-west-2.amazonaws.com/123456789012/your_queue_name",
                "Region": "us-west-2",
                "MaxMessages": 3,
                "AutoAcknowledge": true,
                "WaitTimeSeconds": 0,
                "UnpackSNS": false
            }
        }
    }
}
```

```c#
// MessagingScenarioFactory uses the above JSON configuration to create a SQSReceiver:
IReceiver receiver = MessagingScenarioFactory.CreateReceiver("commands");

// SQSReceiver can also be instantiated directly:
// IReceiver receiver = new SQSReceiver("commands", "https://sqs.us-west-2.amazonaws.com/123456789012/your_queue_name",
//     region:"us-west-2", maxMessages: 3, autoAcknowledge: true, waitTimeSeconds: 0, unpackSNS: false);

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

[.NET Core example]: ../Example.Messaging.SQS.DotNetCore20
[.NET Framework example]: ../Example.Messaging.SQS.DotNetFramework451