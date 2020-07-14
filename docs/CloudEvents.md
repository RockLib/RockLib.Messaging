# How to send and receive messages as CloudEvents

The RockLib.Messaging.CloudEvents package allows messages to be sent, received, and validated according to the [CloudEvents spec](https://github.com/cloudevents/spec).

---
- [Sending CloudEvents with an ISender](#sending-cloudevents-with-an-isender)
- [Receiving CloudEvents from an IReceiver](#receiving-cloudevents-from-an-ireceiver)
- [CloudEvent class](#cloudevent-class)
- [SequentialEvent class](#sequentialevent-class)
- [CorrelatedEvent class](#correlatedevent-class)
- [Protocol Bindings](#protocol-bindings)
---

## Sending CloudEvents with an ISender

To make it easy to send CloudEvents with any `ISender` or `ITransactionalSender`, the `CloudEvent` class and all of its inheritors are implicitly convertible to `SenderMessage`. Simply instantiate a cloud event and pass it anywhere that needs a sender message.

```c#
ISender sender = // TODO: Initialize

// Source and Type must be provided.
CloudEvent cloudEvent = new CloudEvent
{
    Source = new Uri("example.org/sample/123456", UriKind.RelativeOrAbsolute),
    Type = "example"
};

await sender.SendAsync(cloudEvent);
```

Alternatively, explicitly convert a `CloudEvent` by calling its `ToSenderMessage()` method:

```c#
await sender.SendAsync(cloudEvent.ToSenderMessage());
```

## Receiving CloudEvents from an IReceiver

To receive CloudEvents from any `IReceiver`, start it using the `Start<TCloudEvent>` extension method, where `TCloudEvent` is `CloudEvent` or its inheritor.

```c#
public class MyService : IHostedService
{
    private readonly IReceiver _receiver;

    public MyService(IReceiver receiver) =>
        _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _receiver.Start<CorrelatedEvent>(OnCorrelatedEventReceived, ProtocolBindings.Default);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _receiver.Dispose();
        return Task.CompletedTask;
    }

    private async Task OnCorrelatedEventReceived(CorrelatedEvent correlatedEvent,
        IReceiverMessage receiverMessage)
    {
        Console.WriteLine(correlatedEvent.StringData);
        Console.WriteLine($"  id:            {correlatedEvent.Id}");
        Console.WriteLine($"  source:        {correlatedEvent.Source}");
        Console.WriteLine($"  correlationid: {correlatedEvent.CorrelationId}");

        await receiverMessage.AcknowledgeAsync();
    }
}
```

## CloudEvent class

The `CloudEvent` class is the base class for all cloud events. Two additional implementations, [SequentialEvent](#sequentialevent-class) and [CorrelatedEvent](#correlatedevent-class), are included in this package.

<!--

TODO: Add more about CloudEvent class:
- CloudEvent attributes:
  - Id (required, auto-set)
  - Source (required)
  - SpecVersion (required, readonly, always "1.0")
  - Type (required)
  - DataContentType
  - DataSchema
  - Subject
  - Time (auto-set)
- StringData / BinaryData properties
- AdditionalAttributes property
- ProtocolBinding property
- DefaultProtocolBinding property
- Default constructor
- Copy constructor
  - Alternative: `.Copy()` extension method
- Message constructor
  - Alternative: `.As<CloudEvent>()` extension method
- ToSenderMessage method
- Implicit conversion operator
- ToHttpRequestMessage methods
- Validate instance method
- Validate static method
- ContainsHeader method
- TryGetHeaderValue method
    
-->

## SequentialEvent class

The `SequentialEvent` class defines two additional cloud event attributes, `Sequence`, and `SequenceType`.

<!--

TODO: Add more about SequentialEvent class:
- SequenceEvent attributes:
  - Sequence (required)
  - SequenceType (defined value: "Integer")
- Default constructor
- Copy constructor
  - Alternative: `.Copy()` extension method
- Message constructor
  - Alternative: `.As<SequentialEvent>()` extension method
- ToSenderMessage method (overrides base method)
- Validate instance method (overrides base method)
- Validate static method (hides base method)

-->

## CorrelatedEvent class

The `CorrelatedEvent` class defines one additional cloud event attribute, `CorrelationId`.

<!--

TODO: Add more about CorrelatedEvent class:
- CorrelatedEvent attributes:
  - CorrelationId (required, auto-set)
- Default constructor
- Copy constructor
  - Alternative: `.Copy()` extension method
- Message constructor
  - Alternative: `.As<CorrelatedEvent>()` extension method
- ToSenderMessage method (overrides base method)
- Validate static method (hides base method)

-->

## Protocol Bindings

<!-- TODO: Add docs for IProtocolBinding interface and ProtocolBindings static class -->
