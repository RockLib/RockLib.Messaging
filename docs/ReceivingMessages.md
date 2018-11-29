# How to receive messages

## Starting a receiver

In order to receive messages, an instance of the `IReceiver` interface is needed. So throughout this section, it is assumed that you have an `IReceiver` variable named `receiver` declared and initialized somewhere. Like this:

```c#
IReceiver receiver; // TODO: Initialize the receiver variable
```

---

A receiver must be started to begin receiving messages. There are several ways of starting a receiver. One was is to call the `Start` extension method, passing it a callback function to be invoked when a message is received:

```c#
void HandleMessage(IReceiverMessage message)
{
    // TODO: do something with the message
}

receiver.Start(HandleMessage);
```

---

If you need to know which instance of `IReceiver` received the message, pass a callback function with this signature to the `Start` method:

```c#
void HandleMessage(IReceiver receiver, IReceiverMessage message)
{
    // TODO: do something with the receiver and message
}

receiver.Start(HandleMessage);
```

---

If you need to encapsulate the message handler in a class, implement the `IMessageHandler` interface and pass an instance of the object to the `Start` method:

```c#
class MyMessageHandler : IMessageHandler
{
    public void OnMessageReceived(IReceiver receiver, IReceiverMessage message)
    {
        // TODO: do something with the receiver and message
    }
}

IMessageHandler messageHandler = new MyMessageHandler();
receiver.Start(messageHandler);
```

---

An equivalent way of starting a receiver with an `IMessageHandler` implementation is by setting the `MessageHandler` property:

```c#
class MyMessageHandler : IMessageHandler
{
    public void OnMessageReceived(IReceiver receiver, IReceiverMessage message)
    {
        // TODO: do something with the receiver and message
    }
}

IMessageHandler messageHandler = new MyMessageHandler();
receiver.MessageHandler = messageHandler;
```

---

## Handling a message

Received messages implement the `IReceiverMessage` interface. The examples in this section assume there is an `IReceiverMessage` parameter named `message`. To access the message payload, use the `StringPayload` or `BinaryPayload` properties:

```c#
string stringPayload = message.StringPayload;
byte[] binaryPayload = message.BinaryPayload;
```

---

To access the headers of the received message, use the `Headers` property.

```c#
// The TryGetValue method is generic. Note the out bool parameter.
if (message.Headers.TryGetValue("IsDemoCode", out bool isDemoCode))
{
    // TODO: Do something with the isDemoCode flag.
}

// Note the out int parameter.
if (message.Headers.TryGetValue("CodeSnippetIndex", out int codeSnippetIndex))
{
    // TODO: Do something with codeSnippetIndex.
}

// Note the out string parameter.
if (message.Headers.TryGetValue("ExampleDescription", out string exampleDescription))
{
    // TODO: Do something with exampleDescription.
}

```

---

The `IReceiverMessage` interface provides three methods to signal to the sender of the message what the outcome of processing the message was. If the message was processed successfully, it should be acknowledged:

```c#
message.Acknowledge();
```

---

If the message was not processed successfully, and the sender should *not* redeliver it, the message should be rejected:

```c#
message.Reject();
```

---

If the message was not processed successfully, and the sender *should* redeliver it, the message should be rolled back:

```c#
message.Rollback();
```
