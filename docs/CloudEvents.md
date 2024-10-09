---
sidebar_position: 14
---

# How to send and receive messages as CloudEvents

The RockLib.Messaging.CloudEvents package allows messages to be sent, received, and validated according to the [CloudEvents spec](https://github.com/cloudevents/spec).

---
- [Sending CloudEvents with an ISender](#sending-cloudevents-with-an-isender)
- [Validating messages and events](#validating-messages-and-events)
- [Receiving CloudEvents from an IReceiver](#receiving-cloudevents-from-an-ireceiver)
- [CloudEvent class](#cloudevent-class)
  - [CloudEvent Constructors](#cloudevent-constructors)
  - [CloudEvent Attributes](#cloudevent-attributes)
  - [CloudEvent Headers](#cloudevent-headers)
  - [CloudEvent Data](#cloudevent-data)
  - [ProtocolBinding property](#protocolbinding-property)
  - [DefaultProtocolBinding static property](#defaultprotocolbinding-static-property)
  - [ToJson method](#tojson-method)
  - [ToSenderMessage method](#tosendermessage-method)
  - [ToHttpRequestMessage method](#tohttprequestmessage-method)
  - [Validate instance method](#validate-instance-method)
  - [Validate static method](#validate-static-method)
  - [Protected methods](#protected-methods)
- [SequentialEvent class](#sequentialevent-class)
  - [SequentialEvent Constructors](#sequentialevent-constructors)
- [CorrelatedEvent class](#correlatedevent-class)
  - [CorrelatedEvent Constructors](#correlatedevent-constructors)
- [PartitionedEvent class](#partitionedevent-class)
  - [PartitionedEvent Constructors](#partitionedevent-constructors)
- [Protocol Bindings](#protocol-bindings)
---

## Sending CloudEvents with an ISender

To make it easy to send CloudEvents with any `ISender` or `ITransactionalSender`, the `CloudEvent` class and all of its inheritors are implicitly convertible to `SenderMessage`. Simply instantiate a CloudEvent and pass it anywhere that needs a sender message.

```csharp
ISender sender = // TODO: Initialize

// Source and Type must be provided.
CloudEvent cloudEvent = new CloudEvent
{
    Source = "example.org/sample/123456",
    Type = "example"
};

await sender.SendAsync(cloudEvent);
```

Alternatively, explicitly convert a `CloudEvent` by calling its `ToSenderMessage()` method:

```csharp
await sender.SendAsync(cloudEvent.ToSenderMessage());
```

## Validating messages and events

To ensure that a CloudEvent is valid, call its `Validate()` method - it will throw a `CloudEventValidationException` if the event has missing or invalid attribute values.

To ensure that a `SenderMessage` is valid for a given `IProtocolBinding`, call the static `Validate` method on the specified CloudEvent type - it will throw a `CloudEventValidationException` if the sender message has missing or invalid headers given the specified protocol binding.

To ensure that all messages sent by an `ISender` are in the correct format for a given `IProtocolBinding`, wrap it in a `ValidatingSender` and call a static `Validate(SenderMessage, IProtocolBinding)` method in the callback. There is also a `AddValidation<TCloudEvent>()` extension method for `ISenderBuilder` that calls the static `Validate` method of type `TCloudEvent`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddNamedPipeSender("exampleSender")
        .AddValidation<SequentialEvent>(ProtocolBindings.Default);
}
```

## Receiving CloudEvents from an IReceiver

To receive CloudEvents from any `IReceiver`, start it using the `Start<TCloudEvent>` extension method, where `TCloudEvent` is `CloudEvent` or its inheritor.

```csharp
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

The `CloudEvent` class is the base class for all CloudEvents. Three additional implementations, [SequentialEvent](#sequentialevent-class), [CorrelatedEvent](#correlatedevent-class), and [PartitionedEvent](#partitionedevent-class) are included in this package.

### CloudEvent Constructors

The `CloudEvent` class defines four constructors:

- `public CloudEvent()`
  - The *default* constructor.
  - This constructor does not set any properties.
- `public CloudEvent(CloudEvent source)`
  - The *copy* constructor.
  -  Creates a new instance of `CloudEvent` based on an existing instance of `CloudEvent`.
  - Copies all CloudEvent attributes except `Id` and `Time` from the `source` parameter to the new instance.
  - Copies all non-CloudEvent headers to the new instance.
  - Does not copy the event data (`StringData` or `BinaryData` properties).
  - Alternative: Instead of invoking this constructor directly, call the `.Copy()` extension method on an instance of `CloudEvent`. For example, `cloudEvent.Copy()`.
- `public CloudEvent(IReceiverMessage receiverMessage, IProtocolBinding protocolBinding = null)`
  - The *message* constructor.
  - Creates an instance of `CloudEvent` that is equivalent to the specified `IReceiverMessage`.
  - Uses the specified `IProtocolBinding` (or `DefaultProtocolBinding` if null) to map message headers to event attributes.
  - Alternative: Instead of invoking this constructor directly, call the `.To<CloudEvent>()` extension method on an instance of `IReceiverMessage`. For example, `receiverMessage.To<CloudEvent>()`.
- `public CloudEvent(string jsonFormattedCloudEvent)`
  - The *json* constructor.
  - Creates a `CloudEvent` by parsing the [JSON-formatted CloudEvent](https://github.com/cloudevents/spec/blob/v1.0/json-format.md) string.
  - Sets CloudEvent attributes and data.
  - Does not set non-CloudEvent headers.

*Inheritors of the `CloudEvent` class (and inheritors of those classes) are expected to have a default constructor, a copy constructor, a message constructor, and a json constructor that each call their base constructor.*

### CloudEvent Attributes

The following CloudEvent attributes are defined by the `CloudEvent` class:

| Property (`CloudEvent attribute`)   | Type       | Required? | Default Value               | Notes                                             |
|:------------------------------------|:-----------|:----------|:----------------------------|:--------------------------------------------------|
| Id (`id`)                           | `string`   | Yes       | `Guid.NewGuid().ToString()` |                                                   |
| Source (`source`)                   | `string`   | Yes       | N/A                         | Must be valid relative or absolute URI.           |
| SpecVersion (`specversion`)         | `string`   | Yes       | `"1.0"`                     |                                                   |
| Type (`type`)                       | `string`   | Yes       | N/A                         |                                                   |
| DataContentType (`datacontenttype`) | `string`   | No        | N/A                         | Must be valid Content-Type according to RFC 2616. |
| DataSchema (`dataschema`)           | `string`   | No        | N/A                         | Must be valid relative or absolute URI.           |
| Subject (`subject`)                 | `string`   | No        | N/A                         |                                                   |
| Time (`time`)                       | `DateTime` | No        | `DateTime.UtcNow`           |                                                   |

<!-- TODO: Describe how the Attributes property works. -->

### CloudEvent Data

The raw data (or payload) of an event is available from the `StringData` and `BinaryData` properties, as well as from the `GetData<T>` and `TryGetData<T>` extension methods. The data of a `CloudEvent`, can be set by calling one of the `SetData` extension method overloads.

The following example demonstrates usage of these properties and extension methods:

```csharp
// Note that all of the SetData extension method overloads return the same
// CloudEvent, allowing for an event to be initialized on a single line.

// Setting the event's data as a string:
CorrelatedEvent cloudEvent = new CorrelatedEvent()
    .SetData("Hello, world!");

Console.WriteLine(cloudEvent.StringData ?? "<null>"); // Prints "Hello, world!"
Console.WriteLine(ToHexString(cloudEvent.BinaryData) ?? "<null>"); // Prints "<null>"

// Setting the event's data as a byte object:
cloudEvent.SetData(new byte[] { 0x00, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0xFF });

Console.WriteLine(cloudEvent.StringData ?? "<null>"); // Prints "<null>"
Console.WriteLine(ToHexString(cloudEvent.BinaryData) ?? "<null>"); // Prints "01-02-04-08-10-20-40-80"

// Setting the event's data as type T:
Client client = new Client { FirstName = "Brian", LastName = "Friesen" };
cloudEvent.SetData(client, DataSerialization.Json);

// Prints "{'FirstName':'Brian','LastName':'Friesen'}":
Console.WriteLine(cloudEvent.StringData ?? "<null>");

// Prints "<null>":
Console.WriteLine(ToHexString(cloudEvent.BinaryData) ?? "<null>");

// The same instance of T can be retrieved with GetData:
Client retrievedClient = cloudEvent.GetData<Client>(DataSerialization.Json);

// Prints "same"
Console.WriteLine(ReferenceEquals(client, retrievedClient) ? "same" : "different");

// TryGetData also retrieves the same instance of T:
if (cloudEvent.TryGetData(out Client anotherClient, DataSerialization.Json))
    // Prints "retrieved, same"
    Console.Write("retrieved, "
        + (ReferenceEquals(client, anotherClient) ? "same" : "different"));
else
    // Not executed
    Console.WriteLine("not found");

// To clear the event data, pass null to any of the SetData extension methods:
string nullData = null;
cloudEvent.SetData(nullData);

// Prints "<null>":
Console.WriteLine(cloudEvent.StringData ?? "<null>");

// Prints "<null>":
Console.WriteLine(ToHexString(cloudEvent.BinaryData) ?? "<null>");

if (cloudEvent.TryGetData(out Client notFoundClient, DataSerialization.Json))
    // Not executed
    Console.Write("retrieved "
        + (ReferenceEquals(client, notFoundClient) ? "same" : "different"));
else
    // Prints "not found"
    Console.WriteLine("not found");

// If the StringData is set to a serialized object...
cloudEvent.SetData("{'FirstName':'Brian','LastName':'Friesen'}");

// ...then an object of that type can be retrieved with GetData or TryGetData:
Client deserializedClient = cloudEvent.GetData<Client>(DataSerialization.Json);

// Prints "different"
Console.WriteLine(ReferenceEquals(client, deserializedClient) ? "same" : "different");

static string ToHexString(byte[] binaryData) =>
    binaryData is null ? null : BitConverter.ToString(binaryData);

class Client
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```

### CloudEvent Headers

This property exists to store key/value pairs that are not specific to the CloudEvent specification. It also allows for a lossless conversion from CloudEvent to SenderMessage and from IReceiverMessage to CloudEvent.

### ProtocolBinding property

This property is used to determine the header name of a sender or receiver message in order to map it to/from a `CloudEvent`.

### DefaultProtocolBinding static property

The `DefaultProtocolBinding` property determines which `IProtocolBinding` to use when not otherwise specified.

Its value is used...

- ...by the *default* constructor as the initial value of the `ProtocolBinding` property.
- ...by the *message*  constructor - but only if its `protocolBinding` parameter is null - as the initial value of the `ProtocolBinding` property, and also to map the receiver message headers to CloudEvent attributes.
- ...by the static `Validate` method (and methods that hide it) - but only if its `protocolBinding` parameter is null - to map the CloudEvent attributes to sender message headers.

### ToJson method

This method serializes the CloudEvent in the [JSON Event Format for CloudEvents](https://github.com/cloudevents/spec/blob/v1.0/json-format.md).

### ToSenderMessage method

The `CloudEvent` class defines a virtual `ToSenderMessage` that creates a new `SenderMessage` based on the event's data and attributes, using the event's `ProtocolBinding` to map those attributes to sender message headers. Note that before this method creates the `SenderMessage`, it first calls the [`Validate` instance method](#validate-instance-method).

### ToHttpRequestMessage method

To  convert any `CloudEvent` to an `HttpRequestMessage` (to be used by `HttpClient`), call one of the `ToHttpRequestMessage` overloads, optionally passing an `HttpMethod` or request URI.

### Validate instance method

This virtual method ensures that the `CloudEvent` instance is valid - throws a `CloudEventValidationException` if invalid. This method is called at the beginning of the `ToSenderMessage()` method.

*Inheritors of the `CloudEvent` class (and inheritors of those classes) are expected to __override__ this method, call `base.Validate()`, and add additional validation according to the CloudEvent subclass type.*

### Validate static method

This static method ensures that the `SenderMessage` instance is valid according to the `IProtocolBinding` - throws a `CloudEventValidationException` if invalid.

*Inheritors of the `CloudEvent` class (and inheritors of those classes) are expected to **hide** this method, call `CloudEvent.Validate(senderMessage, protocolBinding)`, and add additional validation according to the CloudEvent subclass type.*

### Protected methods

In order to implement custom validation logic, subclasses of `CloudEvent` will need to query the headers of outgoing sender messages. However, the `SenderMessage` class wasn't really designed to have its headers queried, so accessing its headers can be cumbersome. To make it easier for subclasses, the `CloudEvent` class contains two protected helper methods:

- `protected static bool ContainsHeader<T>(SenderMessage senderMessage, string headerName)`
  - Returns whether the `senderMessage` has a header with a name matching the `headerName` and a value of either type `T` or a type convertible to type `T`.
- `protected static bool TryGetHeaderValue<T>(SenderMessage senderMessage, string headerName, out T value)`
  - Gets the value of the header with the specified name as type `T`.

## SequentialEvent class

The `SequentialEvent` class defines two additional CloudEvent attributes, `Sequence`, and `SequenceType`.

| Property (`CloudEvent attribute`) | Type     | Required? | Default Value |
|:----------------------------------|:---------|:----------|:--------------|
| Sequence (`sequence`)             | `string` | Yes       | N/A           |
| SequenceType (`sequencetype`)     | `string` | No        | N/A           |

If the `SequenceType` attribute is set to `"Integer"`, the `Sequence` attribute has the following semantics:
- The values of sequence are string-encoded signed 32-bit Integers.
- The sequence MUST start with a value of 1 and increase by 1 for each subsequent value (i.e. be contiguous and monotonically increasing).
- The sequence wraps around from 2,147,483,647 (2^31 - 1) to -2,147,483,648 (-2^31).

The `SequenceTypes` static class defines a string constant, `Integer`, with the value `"Integer"`.

### SequentialEvent Constructors

The `SequentialEvent` class defines four constructors:

- `public SequentialEvent()`
  - The *default* constructor.
  - This constructor does not set any properties.
- `public SequentialEvent(SequentialEvent source)`
  - The *copy* constructor.
  -  Creates a new instance of `SequentialEvent` based on an existing instance of `SequentialEvent`.
  - Copies all CloudEvent attributes except `Id` and `Time` from the `source` parameter to the new instance.
  - If the value of the `source` parameter's `SequenceType` is `"Integer"` and its `Sequence` is a numeric string, then the new instance will have a `Sequence` equal to the source's `Sequence`, plus one.
  - Does not copy the event data (`StringData` or `BinaryData` properties).
  - Alternative: Instead of invoking this constructor directly, call the `.Copy()` extension method on an instance of `SequentialEvent`. For example, `sequentialEvent.Copy()`.
- `public SequentialEvent(IReceiverMessage receiverMessage, IProtocolBinding protocolBinding = null)`
  - The *message* constructor.
  - Creates an instance of `SequentialEvent` that is equivalent to the specified `IReceiverMessage`.
  - Uses the specified `IProtocolBinding` (or `DefaultProtocolBinding` if null) to map message headers to event attributes.
  - Alternative: Instead of invoking this constructor directly, call the `.To<SequentialEvent>()` extension method on an instance of `IReceiverMessage`. For example, `receiverMessage.To<SequentialEvent>()`.
- `public SequentialEvent(string jsonFormattedCloudEvent)`
  - The *json* constructor.
  - Creates a `CloudEvent` by parsing the [JSON-formatted CloudEvent](https://github.com/cloudevents/spec/blob/v1.0/json-format.md) string.
  - Sets CloudEvent attributes and data.
  - Does not set non-CloudEvent headers.

*Inheritors of the `SequentialEvent` class are expected to have a default constructor, a copy constructor, a message constructor, and a json constructor that each call their base constructor.*

## CorrelatedEvent class

The `CorrelatedEvent` class defines one additional CloudEvent attribute, `CorrelationId`.

| Property (`CloudEvent attribute`) | Type     | Required? | Default Value               |
|:----------------------------------|:---------|:----------|:----------------------------|
| CorrelationId (`correlationid`)   | `string` | Yes       | `Guid.NewGuid().ToString()` |

### CorrelatedEvent Constructors

The `CorrelatedEvent` class defines four constructors:

- `public CorrelatedEvent()`
  - The *default* constructor.
  - This constructor does not set any properties.
- `public CorrelatedEvent(CorrelatedEvent source)`
  - The *copy* constructor.
  -  Creates a new instance of `CorrelatedEvent` based on an existing instance of `CorrelatedEvent`.
  - Copies all CloudEvent attributes except `Id` and `Time` from the `source` parameter to the new instance.
  - Does not copy the event data (`StringData` or `BinaryData` properties).
  - Alternative: Instead of invoking this constructor directly, call the `.Copy()` extension method on an instance of `CorrelatedEvent`. For example, `correlatedEvent.Copy()`.
- `public CorrelatedEvent(IReceiverMessage receiverMessage, IProtocolBinding protocolBinding = null)`
  - The *message* constructor.
  - Creates an instance of `CorrelatedEvent` that is equivalent to the specified `IReceiverMessage`.
  - Uses the specified `IProtocolBinding` (or `DefaultProtocolBinding` if null) to map message headers to event attributes.
  - Alternative: Instead of invoking this constructor directly, call the `.To<CorrelatedEvent>()` extension method on an instance of `IReceiverMessage`. For example, `receiverMessage.To<CorrelatedEvent>()`.
- `public CorrelatedEvent(string jsonFormattedCloudEvent)`
  - The *json* constructor.
  - Creates a `CloudEvent` by parsing the [JSON-formatted CloudEvent](https://github.com/cloudevents/spec/blob/v1.0/json-format.md) string.
  - Sets CloudEvent attributes and data.
  - Does not set non-CloudEvent headers.

*Inheritors of the `CorrelatedEvent` class (and inheritors of those classes) are expected to have a default constructor, a copy constructor, a message constructor, and a json constructor that each call their base constructor.*

## PartitionedEvent class

The `PartitionedEvent` class defines one additional CloudEvent attribute, `PartitionKey`.

| Property (`CloudEvent attribute`) | Type     | Required? | Default Value |
|:----------------------------------|:---------|:----------|:--------------|
| PartitionKey (`partitionkey`)   | `string` | Yes       | N/A           |

### PartitionedEvent Constructors

The `PartitionedEvent` class defines four constructors:

- `public PartitionedEvent()`
  - The *default* constructor.
  - This constructor does not set any properties.
- `public PartitionedEvent(PartitionedEvent source)`
  - The *copy* constructor.
  -  Creates a new instance of `PartitionedEvent` based on an existing instance of `PartitionedEvent`.
  - Copies all CloudEvent attributes except `Id` and `Time` from the `source` parameter to the new instance.
  - Does not copy the event data (`StringData` or `BinaryData` properties).
  - Alternative: Instead of invoking this constructor directly, call the `.Copy()` extension method on an instance of `PartitionedEvent`. For example, `correlatedEvent.Copy()`.
- `public PartitionedEvent(IReceiverMessage receiverMessage, IProtocolBinding protocolBinding = null)`
  - The *message* constructor.
  - Creates an instance of `PartitionedEvent` that is equivalent to the specified `IReceiverMessage`.
  - Uses the specified `IProtocolBinding` (or `DefaultProtocolBinding` if null) to map message headers to event attributes.
  - Alternative: Instead of invoking this constructor directly, call the `.To<PartitionedEvent>()` extension method on an instance of `IReceiverMessage`. For example, `receiverMessage.To<PartitionedEvent>()`.
- `public PartitionedEvent(string jsonFormattedCloudEvent)`
  - The *json* constructor.
  - Creates a `CloudEvent` by parsing the [JSON-formatted CloudEvent](https://github.com/cloudevents/spec/blob/v1.0/json-format.md) string.
  - Sets CloudEvent attributes and data.
  - Does not set non-CloudEvent headers.

*Inheritors of the `PartitionedEvent` class (and inheritors of those classes) are expected to have a default constructor, a copy constructor, a message constructor, and a json constructor that each call their base constructor.*

## Protocol Bindings

The `ProtocolBindings` static class defines the following bindings (corresponding to the [CloudEvents spec](https://github.com/cloudevents/spec#cloudevents-documents)):

- Default
  - Basically does nothing.
- Kafka
  - Attributes are prefixed with "ce_".
  - Remaps "partitionkey" to/from "Kafka.Key".
