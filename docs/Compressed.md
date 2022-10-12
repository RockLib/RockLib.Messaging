---
sidebar_position: 12
---

# Sending and receiving compressed messages

To send compressed messages with RockLib.Messaging, instantiate the `SenderMessage` object using either of its constructors that has a `payload` parameter. Pass the value of the `payload` parameter uncompressed and pass `true` as the value of the optional `compress` parameter. The payload will be gzip compressed and a header with the name `HeaderNames.IsCompressedPayload` is set to `true`, marking the message as compressed. *Note that if gzip compressing the payload does not make it smaller, then the payload is left uncompressed.*

```csharp
ISender sender = // TODO: instantiate

string stringPayload = // TODO: Load value
await sender.SendAsync(new SenderMessage(stringPayload, compress: true));

byte[] binaryPayload = // TODO: Load value
await sender.SendAsync(new SenderMessage(binaryPayload, compress: true));
```

---

On the receiving side, there is nothing to do. All of the implementations of `IReceiverMessage` in this repository inherit from the `ReceiverMessage` base class, which automatically decompresses any messages that have a header named `HeaderNames.IsCompressedPayload` with a value of `true`.
