# Testing components that use RockLib.Messaging

This project is intended to demonstrate how to test classes that have a dependency on an `ISender` or `IReceiver`.

---

The [`FizzBuzzEngine`](FizzBuzzEngine.cs) class is an implementation of [FizzBuzz](https://en.wikipedia.org/wiki/Fizz_buzz#Programming) that sends the result to an `ISender`. [To test it](Tests/FizzBuzzEngineTests.cs), we pass a mock `ISender` to the constructor, then verify that the correct message for the given value was sent to it.

---

This [`ExampleService`](ExampleService.cs) class receives messages from an `IReceiver` and, depending on the value of a message's `Operation` header, creates, updates, or deletes the message's payload using an `IDatabase` interface. [For testing](Tests/ExampleServiceTests.cs), we pass a mock `IReceiver` and a mock `IDatabase` to the constructor. Then we simulate receiving a message by starting the receiver and passing a fake receiver message to the `OnMessageReceivedAsync` method of the receiver's `MessageHandler`. We then verify that the fake receiver message was handled correctly and that the correct method was called on the `IDatabase`.
