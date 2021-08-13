# RockLib.Messaging Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 2.5.3 - 2021-08-12

#### Changed

- Changes "Quicken Loans" to "Rocket Mortgage".
- Updates RockLib.Compression to latest version, [1.0.6](https://github.com/RockLib/RockLib.Compression/blob/main/RockLib.Compression/CHANGELOG.md#106---2021-08-11).
- Updates RockLib.Configuration to latest version, [2.5.3](https://github.com/RockLib/RockLib.Configuration/blob/main/RockLib.Configuration/CHANGELOG.md#253---2021-08-11).
- Updates RockLib.Configuration.ObjectFactory to latest version, [1.6.9](https://github.com/RockLib/RockLib.Configuration/blob/main/RockLib.Configuration.ObjectFactory/CHANGELOG.md#169---2021-08-11).

## 2.5.2 - 2021-05-07

#### Added

- Adds SourceLink to nuget package.

#### Changed

- Updates RockLib.Configuration and RockLib.Configuration.ObjectFactory packages to latest versions, which include SourceLink.

----

**Note:** Release notes in the above format are not available for earlier versions of
RockLib.Messaging. What follows below are the original release notes.

----

## 2.5.1

Fixes a Resolver bug that leads to infinite recursion if using dependency injection on types that take their own interface in their constructor, like decorators.

## 2.5.0

Adds an overload for the AddSender and AddReceiver extension methods that creates objects capable of reloading themselves when their options change.

## 2.4.4

Adds net5.0 target.

## 2.4.3

Adds icon to project and nuget package.

## 2.4.2

Update dependency packages.

## 2.4.1

Updates to align with Nuget conventions.

## 2.4.0

- Adds `ValidatingSender` decorator and extension methods for DI registration.
- Adds `System.Net.Mime.ContentType` to default allowed header types for sender messages.
- Fixes bug in receiver message headers, where the `DateTimeKind` of the value was not preserved. For example, UTC DateTime strings such as "2020-07-02T22:47:24.4298756Z" would be converted with a `Kind` of `Local`.
- Fixes bug in the `byte[]` constructor of `FakeReceiverMessage`, where the `IsBinaryPayload` header was not being set.

## 2.3.1

A few small improvements:
- Lookup delegates should always be registered as singleton.
- Create messaging scenarios from the registered configuration, instead of from the static `MessagingScenarioFactory` class.

## 2.3.0

Always register a lookup delegate from the `AddSender`, `AddTransactionalSender`, and `AddReceiver` extension methods. The previous version would only register the lookup delegate if all of the messaging services of that type were registered as singleton. Also, for each implementation of lookup delegate, if a transient/scoped service is created but not needed, dispose it.


## 2.2.0

The Add(Receiver|Sender|TransactionalSender) extension methods also register a lookup delegate when lifetime is Singleton. The lookup delegates all have a string parameter and return the messaging scenario with the matching name. Useful when more than one sender/receiver is registered.

## 2.1.0

Adds dependency injection extension methods and builders.

## 2.0.4

Adds support for RockLib_Messaging config section.

## 2.0.3

Adds assembly-level [ConfigSection] attributes.

## 2.0.2

Bug fix: don't remove the message id header when clearing sender message headers.

## 2.0.1

- Obsoletes and hides the synchronous receiver API.
- Embeds the license file in the nuget package.

## 2.0.0

- Changes to `MessagingScenarioFactory`:
  - No longer has a concept of "queue vs topic". The factory methods are `CreateSender` and `CreateReceiver`.
  - Uses `IConfiguration` as its data source instead of `IMessagingScenarioFactory` (which no longer exists).
  - Configurations are assumed to have the subsections, `Senders` and/or `Receivers` (case-insensitive), that respectively contain one or more type-defined implementations of `ISender` and `IReceiver`.
  - Uses `RockLib.Configuration.Config.Root.GetSection("RockLib.Messaging")` as the default value for its configuration.
    - Can call `MessagingScenarioFactory.SetConfiguration` to set the configuration programmatically.
  - Has `CreateSender` and `CreateReceiver` extension methods that work with any instance of `IConfiguration` to create senders and receivers.
  - Adds optional `defaultTypes`, `valueConverters`, `resolver`, and `reloadOnConfigChange` parameters to the create methods, allowing greater customization of the sender or receiver.
- Changes to the sending API:
  - There is no longer an `ISenderMessage` interface, just the concrete `SenderMessage` class.
    - Automatically handles encoding and compression.
    - Replaces the `MessageFormat` property with `IsBinary`.
    - Renames `StringValue` and `BinaryValue` properties to `StringPayload` and `BinaryPayload`.
    - Removes the `Priority` property.
  - The `ISender` interface's `SendAsync` method takes a `CancellationToken` parameter.
- Changes to the receiving API:
  - `IReceiverMessage`
    - The `Acknowledge` method is renamed to `AcknowledgeAsync` and made asynchronous (it returns a `Task`).
    - Adds `RollbackAsync` and `RejectAsync` methods.
      - RollbackAsync allows users to indicate that a message was not successfully processed but should be (or should be allowed to be) redelivered.
      - RejectAsync allows users to indicate that a message could not be successfully processed and should not be redelivered.
      - AcknowledgeAsync allows users to indicate that a message was successfully processed and should not be redelivered.
    - Adds `Handled` property.
      - Returns false for a message that has not yet been handled by the `AcknowledgeAsync`, `RollbackAsync`, or `RejectAsync` methods.
      - Returns true for a message after it has been handled by the `AcknowledgeAsync`, `RollbackAsync`, or `RejectAsync` methods.
    - Replaces `GetStringValue` and `GetBinaryValue` methods with `StringPayload` and `BinaryPayload` properties.
    - Replaces `GetHeaderValue` and `GetHeaderNames` methods with `Headers` property.
    - Removes the `Priority` property.
  - `IReceiver`
    - The `MessageReceived` event and `Start` method are replaced by the read/write `MessageHandler` property of type `IMessageHandler`.
    - The receiver is started when the property is set.
    - The receiver calls the `OnMessageReceivedAsync` method of its `MessageHandler` when a message is received.
    - A receiver can also be started by invoking the `Start` extension method.
      - The delegate parameter passed to the extension method is invoked when the receiver receives a message.
    - Adds an `Error` event for implementations to invoke when an internal error occurs - useful when errors originate in a background thread.
  - `Receiver` base class
    - An abstract class to simplify implementations of the `IReceiver` interface and make them more consistent.
    - Adds validation in the setter of the `MessageHandler` property so the a receiver can only be started once. Also disallows a null value.
  - `ReceiverMessage` base class
    - An abstract class to simplify implementations of the `IReceiverMessage` interface and make them more consistent.
    - Its constructor handles the decoding and decompression of incoming message payloads, whether they be string or binary.
    - Provides an abstract method for implementations to populate its headers.
    - Provides abstract methods for the handler methods: `AcknowledgeAsync`, `RollbackAsync`, and `RejectAsync`.
    - Ensures that the message is only handled once.
- The Named Pipes feature has been moved to its own package, RockLib.Messaging.NamedPipes (it is no longer included in the main RockLib.Messaging package).
- Adds the RockLib.Messaging.SNS package.
  - Adds `SNSSender` class, which allows messages to be sent to an SNS topic.
- Changes to the RockLib.Messaging.SQS package:
  - Renames `SQSQueueSender` and `SQSQueueReceiver` to `SQSSender` and `SQSReceiver`.
  - Adds optional `region` parameter to `SQSSender` and `SQSReceiver`.
  - Adds optional `unpackSNS` flag to `SQSReceiver`.
    - Useful when the SQS queue receives messages from an SNS topic - the payloads of such messages contain a JSON object representing the entire raw SNS message.
    - When set, the JSON object is unpacked, and the body and headers are populated from the unpacked message.
- Adds RockLib.Messaging.Http package.
  - `HttpClientSender`
    - An adapter for the `System.Net.Http.HttpClient` class that implements the `ISender` interface.
    - Allows HTTP messages to be sent using the RockLib.Messaging API.
    - Supports "url tokens", where tokens in the url are replaced with the matching header value of the outgoing sender message.
      - For example, a url value of `http://some-url/api/product/{product_id}` would require messages to have a `product_id` header. If the value of that header was `3`, the actual url that the message is sent to would be `http://some-url/api/product/3`.
  - `HttpListenerReceiver`
    - An adapter for the `System.Net.HttpListener` class that implements the `IReceiver` interface.
    - Allows HTTP messages to be received using the RockLib.Messaging API.
    - Intended for use by non-web (console / background process) applications that need to be accessible via HTTP.
    - Supports "path tokens", where headers are added to incoming messages according to the the tokens in the path.
    - For example, if a receiver had a path value of `/api/product/{product_id}`, all messages it receives will have a header named `product_id`. If a client send the request to `/api/product/4`, the message would contain a `product_id` header with a value of `4`.
- Adds the `ForwardingReceiver` class to the main RockLib.Messaging package.
  - This is a decorator implementation of the `IReceiver` interface that takes three optional `ISender` instances: one for `AcknowledgeAsync`, one for `RollbackAsync`, and one for `RejectAsync`.
  - When a message received by the decorated `IReceiver` is acknowledged (or rolled back / rejected), if the `ForwardingReceiver` is configured with an `AcknowledgeForwarder` (or `RollbackForwarder` / `RejectForwarder`), the message is sent to that `ISender`.
- Adds a `FakeReceiverMessage` class.
  - Allows users to more easily test their application's receiver message handler (without requiring the use of a mocking framework).

## 2.0.0-alpha15

- Adds optional parameters to MessagingScenarioFactory methods: defaultTypes, valueConverters, resolver, and reloadOnConfigChange.
- Removes the defaultSenderType and defaultReceiverType settings from MessagingScenarioFactory.

## 2.0.0-alpha14

- Adds an `Error` event to the `IReceiver` interface.
- The `IMessageHandler` interface is now async. Added additional asynchronous `IReceiver.Start` extension method overloads.

## 2.0.0-alpha13

Adds cancellation token parameter to ReceiverMessage's abstract handler methods.

## 2.0.0-alpha12

- The IReceiverMessage handling methods (acknowledge, rollback, reject) are now asynchronous.
- Added synchronous message handler extension methods.

## 2.0.0-alpha11

Updates the RockLib.Configuration.ObjectFactory package to the latest version.

## 2.0.0-alpha10

Adds two public classes:

- RockLib.Messaging.ForwardingReceiver: A decorator for the IReceiver interface that can forward messages using an ISender instance when acknowledged, rolled back, or rejected.
- RockLib.Messaging.Testing.FakeReceiverMessage: A fake implementation of the IReceiverMessage interface that allows an application's IReceiver message handler implementation to be unit tested without requiring a mocking framework.

## 2.0.0-alpha09

Small improvements to the API:

- Added Handled flag to IReceiverMessage. Messages shouldn't be handled multiple times.
- Added GetValue\<T\> method to receiver message HeaderDictionary.
- Remove Priority from sender and receiver messages.

## 2.0.0-alpha08

The `Receiver` base class and `IReceiever.Start` extension methods do not allow the receiver to be started more than once.

## 2.0.0-alpha07

Adds `Reject` method to `IReceiverMessage` interface.

## 2.0.0-alpha06

HeaderDictionary just has a generic TryGetValue method - no need for TryGetStringValue, TryGetInt32Value, or TryGetBooleanValue anymore.

## 2.0.0-alpha05

Minor fixes:
- Adds support for enum and TimeSpan values for SenderMessage headers.
- Improves int and bool conversions in receiver's HeaderDictionary.

## 2.0.0-alpha04

Updates the configuration packages to the latest versions.

## 2.0.0-alpha03

Adds a cancellation token parameter to SendAsync methods.

## 2.0.0-alpha02

Add .NET Framework 4.6.2 as a target.

## 2.0.0-alpha01

Initial prerelease of 2.x package.

## 1.0.2

Update the RockLib.Configuration and RockLib.Configuration.ObjectFactory nuget packages.

## 1.0.1

Update RockLib.Configuration and RockLib.Configuration.ObjectFactory nuget packages to latest versions.

## 1.0.0

Initial release.
