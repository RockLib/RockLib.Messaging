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

#### CloudEvent Attributes

| Property (`CloudEvent attribute`)   | Type          | Required? | Default Value               |
|:------------------------------------|:--------------|:----------|:----------------------------|
| Id (`id`)                           | `string`      | Yes       | `Guid.NewGuid().ToString()` |
| Source (`source`)                   | `Uri`         | Yes       | N/A                         |
| SpecVersion (`specversion`)         | `string`      | Yes       | `"1.0"`                     |
| Type (`type`)                       | `string`      | Yes       | N/A                         |
| DataContentType (`datacontenttype`) | `ContentType` | No        | N/A                         |
| DataSchema (`dataschema`)           | `Uri`         | No        | N/A                         |
| Subject (`subject`)                 | `string`      | No        | N/A                         |
| Time (`time`)                       | `DateTime`    | No        | `DateTime.UtcNow`           |

#### CloudEvent Data

abc

#### AdditionalAttributes property

abc

#### ProtocolBinding property

abc

#### DefaultProtocolBinding static property

abc

#### CloudEvent Constructors

The `CloudEvent` class defines three constructors:

- `public CloudEvent()`
  - The *default* constructor.
  - This constructor does not set any properties.
- `public CloudEvent(CloudEvent source)`
  - The *copy* constructor.
  -  Creates a new instance of `CloudEvent` based on an existing instance of `CloudEvent`.
  - Copies all cloud event attributes except `Id` and `Time` from the `source` parameter to the new instance.
  - Does not copy the event data (`StringData` or `BinaryData` properties).
  - Alternative: Instead of invoking this constructor directly, call the `.Copy()` extension method on an instance of `CloudEvent`, e.g. `cloudEvent.Copy()`.
- `public CloudEvent(IReceiverMessage receiverMessage, IProtocolBinding protocolBinding = null)`
  - The *message* constructor.
  - Creates an instance of `CloudEvent` that is equivalent to the specified `IReceiverMessage`.
  - Uses the specified `IProtocolBinding` (or `DefaultProtocolBinding` if null) to map message headers to event attributes.
  - Alternative: Instead of invoking this constructor directly, call the `.To<CloudEvent>()` extension method on an instance of `IReceiverMessage`, e.g. `receiverMessage.To<CloudEvent>()`.

#### ToSenderMessage method

The `CloudEvent` class defines a virtual `ToSenderMessage` that creates a new `SenderMessage` based on the event's data and attributes, using the event's `ProtocolBinding` to map those attributes to sender message headers.

Inheritors of the `CloudEvent` class are expected to override the `ToSenderMessage` and map additional attributes to their corresponding sender message headers.

#### ToHttpRequestMessage method

To  convert any `CloudEvent` to an `HttpRequestMessage` (to be used by `HttpClient`), call one of the `ToHttpRequestMessage` overloads, optionally passing an `HttpMethod` or request URI.

#### Validate instance method

abc

#### Validate static method

abc

#### Protected methods

In order to implement custom validation logic, subclasses of `CloudEvent` will need to query the headers of outgoing sender messages. However, the `SenderMessage` class wasn't really designed to have its headers queried, so accessing its headers can be cumbersome. To make it easier for subclasses, the `CloudEvent` class contains two protected helper methods:

- `protected static bool ContainsHeader<T>(SenderMessage senderMessage, string headerName)`
  - Returns whether the `senderMessage` has a header with a name matching the `headerName` and a value of either type `T` or a type convertible to type `T`.
- `protected static bool TryGetHeaderValue<T>(SenderMessage senderMessage, string headerName, out T value)`
  - Gets the value of the header with the specified name as type `T`.

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
