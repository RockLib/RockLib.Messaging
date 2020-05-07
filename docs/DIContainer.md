# Using RockLib.Messaging with a DI/IOC container

RockLib.Messaging includes extension methods for registering with the Microsoft.Extensions.DependencyInjection container. There are three extension methods - `AddSender`, `AddTransactionalSender`, and `AddReceiver` - each with two variants. One variant registers the sender/receiver returned by a delegate and the other registers the sender/receiver from `MessagingScenarioFactory` by name.

It may not be necessary to call these methods directly - each implementation has its own set of extension methods. For example, SQS has `AddSQSSender` and `AddSQSReceiver` extension methods that end up calling `AddSender` and `AddReceiver`.

---

This example registers by delegate:

```c#
public void RegisterServices(IServiceCollection services)
{
    serivces.AddSender((IServiceProvider serviceProvider) =>
    {
        // TODO: retrieve any dependencies from serviceProvider
        ISender sender = // TODO: create ISender
        return sender;
    });

    serivces.AddTransactionalSender((IServiceProvider serviceProvider) =>
    {
        // TODO: retrieve any dependencies from serviceProvider
        ITransactionalSender sender = // TODO: create ITransactionalSender
        return sender;
    });

    serivces.AddReceiver((IServiceProvider serviceProvider) =>
    {
        // TODO: retrieve any dependencies from serviceProvider
        IReceiver receiver = // TODO: create IReceiver
        return receiver;
    });
}
```

---

This example registers with `MessagingScenarioFactory`:

```c#
public void RegisterServices(IServiceCollection services)
{
    // Registers ISender from MessagingScenarioFactory.CreateSender("MySender")
    serivces.AddSender("MySender");

    // Registers ITransactionalSender from MessagingScenarioFactory.CreateSender("MyTransactionalSender")
    // If the named sender is not an ITransactionalSender, a runtime error will occur.
    serivces.AddTransactionalSender("MyTransactionalSender");

    // Registers IReceiver from MessagingScenarioFactory.CreateReceiver("MyReceiver")
    serivces.AddReceiver("MyReceiver");
}
```

### Dealing with multiple senders or receivers

In addition to registering the `ISender`, `ITransactionalSender`, or `IReceiver` interfaces, the extension methods also register a custom delegate - `SenderLookup`, `TransactionalSenderLookup`, or `ReceiverLookup` respectively. These custom delegates all have a single string parameter that takes the name of the sender/receiver to retrieve.

This example service is dependant on two instances of `IReceiver` and has a `ReceiverLookup` injected to retrieve them:

```c#
public class ExampleService
{
    private readonly IReceiver _dataReceiver;
    private readonly IReceiver _commandReceiver;

    public ExampleService(ReceiverLookup receiverLookup)
    {
        _dataReceiver = receiverLookup("DataReceiver");
        _commandReceiver = receiverLookup("CommandReceiver");
    }
}
```

### Builder objects

The `AddSender`, `AddTransactionalSender`, and `AddReceiver` methods all return a builder interface - `ISenderBuilder`, `ITransactionalSenderBuilder`, and `IReceiverBuilder` - that have a `Decorate` method. These builders allow decorator implementations of `ISender`, `ITransactionalSender`, and `IReceiver` to be registered just as easily as the main implementations.

[`ForwardingReceiver`](ForwardingReceiver.md) is an example of a decorator implementation of `IReceiver`. And just like SQS, which has `AddSQSSender` and `AddSQSReceiver` extension methods, `ForwardingReceiver` has an `AddForwardingReceiver` extension method. Other decorator implementations should follow this example.
