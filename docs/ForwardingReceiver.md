---
sidebar_position: 10
---

# ForwardingReceiver

The `ForwardingReceiver` class is a [decorator] implementation of the `IReceiver` interface that can automatically forward messages to an `ISender` when they are handled. This makes it very simple to implement the "fault queue" pattern.

For example, an application might need to forward messages to a "reject" queue if a message is malformed and forward messages to a "retry" queue if an external error occurred while processing a message. With a forwarding receiver, the message handler for this app could be very simple:

```csharp
ForwardingReceiver receiver = // TODO: initialize

receiver.Start(async message =>
{
    if (IsMalformed(message.StringPayload))
    {
        await message.RejectAsync(); // Forwards to "reject" queue
        return;
    }

    try
    {
        ProcessMessage(message.StringPayload);
        await message.AcknowledgeAsync();
    }
    catch (Exception ex)
    {
        await message.RollbackAsync(); // Forwards to "retry" queue
        // TODO: Probably log the error instead of just throwing
        throw;
    }
});
```

---

Adding a forwarding receiver using dependency injection (using SQS for example):

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSQSSender("RejectQueue", options => options.QueueUrl = "my-reject-queue-url");
    services.AddSQSSender("RetryQueue", options => options.QueueUrl = "my-retry-queue-url");

    services.AddSQSReceiver("MyForwardingReceiver", options =>
        {
            options.QueueUrl = "my-main-queue-url";
            options.AutoAcknowledge = false;
        })
        .AddForwardingReceiver(options =>
        {
            options.RejectForwarderName = "RejectQueue";
            options.RejectOutcome = ForwardingOutcome.Acknowledge;

            options.RollbackForwarderName = "RetryQueue";
            options.RollbackOutcome = ForwardingOutcome.Acknowledge;
        });
}
```

---

An equivalent forwarding receiver defined in config:

```json
{
  "RockLib.Messaging": {
    "Senders": [
      {
        "Type": "RockLib.Messaging.SQS.SQSSender, RockLib.Messaging.SQS",
        "Value": {
          "Name": "RejectQueue",
          "QueueUrl": "my-reject-queue-url"
        }
      },
      {
        "Type": "RockLib.Messaging.SQS.SQSSender, RockLib.Messaging.SQS",
        "Value": {
          "Name": "RetryQueue",
          "QueueUrl": "my-retry-queue-url"
        }
      }
    ],
    "Receivers": [
      {
        "Type": "RockLib.Messaging.SQS.SQSReceiver, RockLib.Messaging.SQS",
        "Value": {
          "Name": "MainQueue",
          "QueueUrl": "my-main-queue-url",
          "AutoAcknowledge": false
        }
      },
      {
        "Type": "RockLib.Messaging.ForwardingReceiver, RockLib.Messaging",
        "Value": {
          "Name": "MyForwardingReceiver",
          "ReceiverName": "MainQueue",
          "RejectForwarderName": "RejectQueue",
          "RejectOutcome": "Acknowledge",
          "RollbackForwarderName": "RetryQueue",
          "RollbackOutcome": "Acknowledge"
        }
      }
    ]
  }
}
```

This forwarding receiver can be created by `MessagingScenarioFactory`:
Note that the Value object's properties in the json must map to a valid constructor since CreateSender Creates instances using [RockLib.Configuration.ObjectFactory](https://github.com/RockLib/RockLib.Configuration/tree/main/RockLib.Configuration.ObjectFactory#rocklibconfigurationobjectfactory)

```csharp
IReceiver receiver = MessagingScenarioFactory.CreateReceiver("MyForwardingReceiver");
```

[decorator]: https://en.wikipedia.org/wiki/Decorator_pattern
