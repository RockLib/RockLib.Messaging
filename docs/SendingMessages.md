---
sidebar_position: 2
---

# How to send messages

To send messages, an instance of the `ISender` interface is needed. Throughout this document, it's assumed that you have an `ISender` variable named `sender` declared and initialized somewhere. Like this:

```csharp
ISender sender; // TODO: Initialize the sender variable
```

---

One way to send a message is by passing the message payload to the `SendAsync` extension method (note that these examples all use `string` payloads, but there are overloads for each method that take a `byte[]`):

```csharp
CancellationTokenSource cancellation = new CancellationTokenSource();
await sender.SendAsync("<message payload goes here>", cancellation.Token);
```

---

If you don't need the call to be cancellable, omit the `CancellationToken`:

```csharp
await sender.SendAsync("<message payload goes here>");
```

---

If you need to make the call synchronously, use the `Send` extension method:

```csharp
sender.Send("<message payload goes here>");
```

---

If you need to set header values in the message, create an instance of `SenderMessage` and pass it to the `SendAsync` method:

```csharp
CancellationTokenSource cancellation = new CancellationTokenSource();

SenderMessage message = new SenderMessage("<message payload goes here>");
message.Headers["IsDemoCode"] = true;

await sender.SendAsync(message, cancellation.Token);
```

If you need to utilize the `MessageGroupId` and `MessageDeduplicationId` properties, add the values to the header:

```csharp
CancellationTokenSource cancellation = new CancellationTokenSource();

SenderMessage message = new SenderMessage("<message payload goes here>");
message.Headers["messageGroupId"] = "message group id";
message.Headers["messageDeduplicationId"] = "message deduplication id";

await sender.SendAsync(message, cancellation.Token);
```

---

The `CancellationToken` can be omitted from this call as well:

```csharp
SenderMessage message = new SenderMessage("<message payload goes here>");
message.Headers["CodeSnippetIndex"] = 5;

await sender.SendAsync(message);
```

---

And there is a synchronous version as expected:

```csharp
SenderMessage message = new SenderMessage("<message payload goes here>");
message.Headers["ExampleDescription"] = "boring";

sender.Send(message);
```
