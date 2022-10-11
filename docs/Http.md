---
sidebar_position: 7
---

# How to use and configure RockLib.Messaging.Http

See the [.NET Core example] or [.NET Framework example] for a complete demo application.

## HttpClientSender

The HttpClientSender class ultimately uses a `System.Net.Http.HttpClient` behind the scenes to send messages. It can be directly instantiated and has the following parameters:

- name
  - The name of the instance of HttpClientSender.
- url
  - The URL to send HTTP requests to.
  - See [URL Tokens](#url-tokens) below.
- method (optional, defaults to `"POST"`)
  - The http method to use when sending messages.
- defaultHeaders (optional, defaults to `null`)
  - Optional headers that are added to each http request.

MessagingScenarioFactory can be configured with an `HttpClientSender` named "commands" as follows:

```json
{
    "RockLib.Messaging": {
        "Senders": {
            "Type": "RockLib.Messaging.Http.HttpClientSender, RockLib.Messaging.Http",
            "Value": {
                "Name": "commands",
                "Url": "http://foo.com/api/v1/commands",
                "Method": "PUT",
                "DefaultHeaders": {
                    "Content-Type": "application/json",
                    "Custom-Header": "some_value"
                }
            }
        }
    }
}
```

```csharp
// Note that implementations of the ISender interface are somewhat expensive and intended to be
// long-lived. They are thread-safe, so you can use a single instance throughout your application.
// Instances should be disposed before the application exits.

// MessagingScenarioFactory uses the above JSON configuration to create an HttpClientSender
// Note that the Value object's properties in the json must map to a valid constructor since CreateSender Creates instances using [RockLib.Configuration.ObjectFactory](https://github.com/RockLib/RockLib.Configuration/tree/main/RockLib.Configuration.ObjectFactory#rocklibconfigurationobjectfactory)
ISender sender = MessagingScenarioFactory.CreateSender("commands");

// HttpClientSender can also be instantiated directly:
// ISender sender = new HttpClientSender("commands",
//     "http://foo.com/api/v1/commands",
//     "PUT",
//     new Dictionary<string, string>
//     {
//         ["Content-Type"] = "application/json",
//         ["Custom-Header"] = "some_value/json"
//     });

// Use the sender (for good, not evil):
sender.Send("DROP DATABASE Production;");

// Always dispose the sender when done with it.
sender.Dispose();
```

#### URL Tokens

The `url` parameter supports the concept of "URL tokens" - where its value contains some identifier surrounded by curly braces. When a message is sent, the actual URL is calculated by replacing the token with the value of corresponding message header.

```csharp
ISender sender = new HttpClientSender("my_sender", "http://foo.com/api/{api_version}/commands");

SenderMessage message = new SenderMessage("Hello, world!");
message.Headers.Add("api_version", "v2");

// The actual URL for the http request will be "http://foo.com/api/v2/commands".
sender.Send(message);
```

*Note that if a sender message does not have a header for each URL token, an exception will be thrown when sending that message.*

## HttpListenerReceiver

The  HttpListenerReceiver class ultimately uses a `System.Net.HttpListener` behind the scenes to receive and handle incoming http requests. See https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistener for information on how to use and configure the `HttpListener` class.

#### Constructor 1

This constructor calls [Constructor 2](#constructor-2), passing along the values of all identically named parameters. It uses the value of its `url` parameter to derive values for the other constructor's `prefixes` and `path` parameters.

- name
  - The name of the instance of HttpListenerReceiver.
- url
  - The url that the `HttpListener` should listen to.
  - See [Path Tokens](#path-tokens) below.
- acknowledgeStatusCode (optional, defaults to `200`)
  - The status code to be returned to the client when a message is acknowledged.
- rollbackStatusCode (optional, defaults to `500`)
  - The status code to be returned to the client when a message is rolled back.
- rejectStatusCode (optional, defaults to `400`)
  - The status code to be returned to the client when a message is acknowledged.
- method (optional, defaults to `POST`)
  - The http method that requests must have in order to be handled. Any request that does not have this method will receive a `405 Method Not Allowed` response.
- requiredHeaders (optional, defaults to `null`)
  - The HTTP headers that incoming requests are required to match in order to be handled. Any request that does not have match the required headers will receive a 4xx response.

#### Constructor 2

This constructor calls [Constructor 4](#constructor-4), passing along the values of all identically named parameters. It uses the values of its `acknowledgeStatusCode`, `rollbackStatusCode`, and `rejectStatusCode` parameters to create an instance of `DefaultHttpResponseGenerator` which is passed as the value of the other constructor's `httpResponseGenerator` parameter.

- name
  - The name of the instance of HttpListenerReceiver.
- prefixes
  - The URI prefixes handled by the `HttpListener`.
- path
  - The path that requests must match in order to be handled. Any request whose path does not match this value will receive a `404 Not Found` response.
  - See [Path Tokens](#path-tokens) below.
- acknowledgeStatusCode (optional, defaults to `200`)
  - The status code to be returned to the client when a message is acknowledged.
- rollbackStatusCode (optional, defaults to `500`)
  - The status code to be returned to the client when a message is rolled back.
- rejectStatusCode (optional, defaults to `400`)
  - The status code to be returned to the client when a message is acknowledged.
- method (optional, defaults to `POST`)
  - The http method that requests must have in order to be handled. Any request that does not have this method will receive a `405 Method Not Allowed` response.
- requiredHeaders (optional, defaults to `null`)
  - The HTTP headers that incoming requests are required to match in order to be handled. Any request that does not have match the required headers will receive a 4xx response.

#### Constructor 3

This constructor calls [Constructor 4](#constructor-4), passing along the values of all identically named parameters. It uses the value of its `url` parameter to derive values for the other constructor's `prefixes` and `path` parameters.

- name
  - The name of the instance of HttpListenerReceiver.
- url
  - The url that the `HttpListener` should listen to.
  - See [Path Tokens](#path-tokens) below.
- httpResponseGenerator
  - An object that determines the http response that is returned to clients, depending on whether the message is acknowledged, rejected, or rolled back.
- method (optional, defaults to `POST`)
  - The http method that requests must have in order to be handled. Any request that does not have this method will receive a `405 Method Not Allowed` response.
- requiredHeaders (optional, defaults to `null`)
  - The HTTP headers that incoming requests are required to match in order to be handled. Any request that does not have match the required headers will receive a 4xx response.

#### Constructor 4

This constructor actually does the initialization.

- name
  - The name of the instance of HttpListenerReceiver.
- prefixes
  - The URI prefixes handled by the `HttpListener`.
- path
  - The path that requests must match in order to be handled. Any request whose path does not match this value will receive a `404 Not Found` response.
  - See [Path Tokens](#path-tokens) below.
- httpResponseGenerator
  - An object that determines the http response that is returned to clients, depending on whether the message is acknowledged, rejected, or rolled back.
- method (optional, defaults to `POST`)
  - The http method that requests must have in order to be handled. Any request that does not have this method will receive a `405 Method Not Allowed` response.
- requiredHeaders (optional, defaults to `null`)
  - The HTTP headers that incoming requests are required to match in order to be handled. Any request that does not have match the required headers will receive a 4xx response.

#### Path Tokens

The `path` and `url` parameters in the constructors above support the concept of "path tokens" - where a header is added to the receiver message for each token. The path of the incoming http request determines the value of the token header.

```csharp
IReceiver receiver = new HttpListenerReceiver("my_receiver", "http://localhost:5000/api/{api_version}/commands");

receiver.Start(async message =>
{
    if (message.Headers.TryGetValue("api_version", out string apiVersion))
    {
        // If the incoming http request had a path of "/api/v1/commands",
        // then the value of the apiVersion variable here would be "v1".
    }

    await message.AcknowledgeAsync();
});
```

*Note that since all handled requests are guaranteed to match the path, messages from a receiver with path tokens will always have a header for each token.*

[.NET Core example]: ../Example.Messaging.Http.DotNetCore20
[.NET Framework example]: ../Example.Messaging.Http.DotNetFramework451
