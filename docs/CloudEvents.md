# How to send and receive messages as CloudEvents

The RockLib.Messaging.CloudEvents package allows messages to be sent, received, and validated according to the [CloudEvents spec](https://github.com/cloudevents/spec).

### Sending CloudEvents with an ISender

<!-- TODO: Add docs for sending CloudEvents -->

### Receiving CloudEvents from an IReceiver

<!-- TODO: Add docs for receiving CloudEvents -->

### CloudEvent class

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
- IReceiverMessage constructor
- Alternative: `.As<CloudEvent()` extension method
- ToSenderMessage method
- Implicit conversion operator
- ToHttpRequestMessage methods
- Validate instance method
- Validate static method
- ContainsHeader method
- TryGetHeaderValue method
    
-->

### SequentialEvent class

The `SequentialEvent` class defines two additional cloud event attributes, `Sequence`, and `SequenceType`.

<!--

TODO: Add more about SequentialEvent class:
- SequenceEvent attributes:
  - Sequence (required)
  - SequenceType (defined value: "Integer")
- Default constructor
- Copy constructor
- IReceiverMessage constructor
- ToSenderMessage method (overrides base method)
- Validate instance method (overrides base method)
- Validate static method (hides base method)

-->

### CorrelatedEvent class

The `CorrelatedEvent` class defines one additional cloud event attribute, `CorrelationId`.

<!--

TODO: Add more about CorrelatedEvent class:
- CorrelatedEvent attributes:
  - CorrelationId (required, auto-set)
- Default constructor
- Copy constructor
- IReceiverMessage constructor
- ToSenderMessage method (overrides base method)
- Validate static method (hides base method)

-->

### Protocol Bindings

<!-- TODO: Add docs for IProtocolBinding interface and ProtocolBindings static class -->
