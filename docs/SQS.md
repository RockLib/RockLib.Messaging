# How to use and configure RockLib.Messaging.SQS

See the [.NET Core example] or [.NET Framework example] for a complete demo application.

## SQSSender

The SQSSender class can be directly instantiated and has the following parameters:

- name
  - The name of the instance of SQSSender.
- queueUrl
  - The url of the SQS queue.
- region (optional, defaults to null)
  - The region of the SQS queue.
- messageGroupId (optional, defaults to null)
  - The tag that specifies that a message belongs to a specific message group. Messages that belong to the same message group are processed in a FIFO manner (however, messages in different message groups might be processed out of order). To interleave multiple ordered streams within a single queue, use MessageGroupId values (for example, session data for multiple users). In this scenario, multiple consumers can process the queue, but the session data of each user is processed in a FIFO fashion. This parameter applies only to FIFO (first-in-first-out) queues.

```c#
ISender sender = new SQSSender("MySender", new Uri("https://sqs.us-west-2.amazonaws.com/123456789012/your_queue_name"),
    region: "us-west-2", messageGroupId: null);
```

---

To add an SQSSender to a service collection for dependency injection, use the `AddSQSSender` method, optionally passing in a `configureOptions` callback:

```c#
services.AddSQSSender("MySender", options =>
{
    options.Region = "us-west-2";
    options.QueueUrl = new Uri("https://sqs.us-west-2.amazonaws.com/123456789012/your_queue_name");
    options.MessageGroupId = null;
});
```

To bind `SQSSenderOptions` to a configuration section, use the name of the sender when calling the `Configure` method:

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<SQSSenderOptions>("MySender", Configuration.GetSection("MySQSSender"));
    services.AddSQSSender("MySender");
}

/* appsettings.json:
{
  "MySQSSender": {
    "Region": "us-west-2",
    "QueueUrl": "https://sqs.us-west-2.amazonaws.com/123456789012/your_queue_name"
  }
}
*/
```

---

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

// MessagingScenarioFactory uses the above JSON configuration to create a SQSSender
// Note that the Value object's properties in the json must map to a valid constructor since CreateSender Creates instances using [RockLib.Configuration.ObjectFactory](https://github.com/RockLib/RockLib.Configuration/tree/main/RockLib.Configuration.ObjectFactory#rocklibconfigurationobjectfactory)
ISender sender = MessagingScenarioFactory.CreateSender("commands");

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
- region (optional, defaults to null)
  - The region of the SQS queue.
- maxMessages (optional, defaults to 3)
  - The maximum number of messages to return with each call to the SQS endpoint. Amazon SQS never returns more messages than this value (however, fewer messages might be returned). Valid values are 1 to 10.
- autoAcknowledge (optional, defaults to true)
  - Whether messages will be automatically acknowledged after the message handler executes.
- waitTimeSeconds (optional, defaults to 0)
  - The duration (in seconds) for which calls to ReceiveMessage wait for a message to arrive in the queue before returning. If a message is available, the call returns sooner than WaitTimeSeconds. If no messages are available and the wait time expires, the call returns successfully with an empty list of messages.
- unpackSNS (optional, defaults to false)
  - Whether to attempt to unpack the message body as an SNS message.
- terminateMessageVisibilityTimeoutOnRollback (optional, defaults to false)
  - Whether to terminate the message visibility timeout when SQSReceiverMessage.RollbackMessageAsync is called. Terminating the message visibility timeout allows the message to immediately become available for queue consumers to process.

```c#
IReceiver receiver = new SQSReceiver("MyReceiver", new Uri("https://sqs.us-west-2.amazonaws.com/123456789012/your_queue_name"),
    region:"us-west-2", maxMessages: 3, autoAcknowledge: true, waitTimeSeconds: 0, unpackSNS: false, terminateMessageVisibilityTimeoutOnRollback: false);
```

---

To add an SQSReceiver to a service collection for dependency injection, use the `AddSQSReceiver` method, optionally passing in a `configureOptions` callback:

```c#
services.AddSQSReceiver("MySender", options =>
{
    options.Region = "us-west-2";
    options.QueueUrl = new Uri("https://sqs.us-west-2.amazonaws.com/123456789012/your_queue_name");
    options.MaxMessages = 3;
    options.AutoAcknowledge = true;
    options.WaitTimeSeconds = 0;
    options.UnpackSNS = false;
    options.TerminateMessageVisibilityTimeoutOnRollback = false;
});
```

To bind `SQSReceiverOptions` to a configuration section, use the name of the sender when calling the `Configure` method:

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<SQSReceiverOptions>("MyReceiver", Configuration.GetSection("MySQSReceiver"));
    services.AddSQSReceiver("MyReceiver");
}

/* appsettings.json:
{
  "MySQSReceiver": {
    "Region": "us-west-2",
    "QueueUrl": "https://sqs.us-west-2.amazonaws.com/123456789012/your_queue_name",
    "MaxMessages": 3,
    "AutoAcknowledge": true,
    "WaitTimeSeconds": 0,
    "UnpackSNS": false,
    "TerminateMessageVisibilityTimeoutOnRollback": false
  }
}
*/
```

---

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
                "UnpackSNS": false,
                "TerminateMessageVisibilityTimeoutOnRollback": false
            }
        }
    }
}
```

```c#
// MessagingScenarioFactory uses the above JSON configuration to create a SQSReceiver
// Note that the Value object's properties in the json must map to a valid constructor since CreateSender Creates instances using [RockLib.Configuration.ObjectFactory](https://github.com/RockLib/RockLib.Configuration/tree/main/RockLib.Configuration.ObjectFactory#rocklibconfigurationobjectfactory)
IReceiver receiver = MessagingScenarioFactory.CreateReceiver("commands");

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
