# ForwardingReceiver

The `ForwardingReceiver` class is a [decorator] implementation of the `IReceiver` interface that can automatically forward messages to an `ISender` when they are handled. This makes it very simple to implement the "fault queue" pattern.

For example, an application might need to forward messages to a "reject" queue if a message is malformed and forward messages to a "retry" queue if an external error occurred while processing a message. With a forwarding receiver, the message handler for this app could be very simple:

```c#
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

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddSQSSender("RejectQueue", options => options.QueueUrl = "my-reject-queue-url");
    services.AddSQSSender("RetryQueue", options => options.QueueUrl = "my-retry-queue-url");

    services.AddSQSReceiver("MainQueue", options =>
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

An equivalent forwarding receiver can be defined in config:

```json
{
  "RockLib.Messaging": {
    "Senders": [
      {
        "Type": "RockLib.Messaging.NamedPipes.NamedPipeSender, RockLib.Messaging.NamedPipes",
        "Value": {
          "Name": "RejectQueue",
          "PipeName": "example_pipe"
        }
      },
      {
        "Type": "RockLib.Messaging.NamedPipes.NamedPipeSender, RockLib.Messaging.NamedPipes",
        "Value": {
          "Name": "RetryQueue",
          "PipeName": "example_pipe"
        }
      }
    ],
    "Receivers": [
      {
        "Type": "RockLib.Messaging.NamedPipes.NamedPipeReceiver, RockLib.Messaging.NamedPipes",
        "Value": {
          "Name": "MainQueue",
          "PipeName": "example_pipe"
        }
      },
      {
        "Type": "RockLib.Messaging.ForwardingReceiver, RockLib.Messaging",
        "Value": {
          "Name": "ForwardingQueue",
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

```c#
IReceiver receiver = MessagingScenarioFactory.CreateReceiver("ForwardingQueue");
```

[decorator]: https://en.wikipedia.org/wiki/Decorator_pattern
