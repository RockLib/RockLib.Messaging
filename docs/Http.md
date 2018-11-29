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

```c#
// Note that implementations of the ISender interface are somewhat expensive and intended to be
// long-lived. They are thread-safe, so you can use a single instance throughout your application.
// Instances should be disposed before the application exits.

// MessagingScenarioFactory uses the above JSON configuration to create an HttpClientSender:
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

The `HttpClientSender.Url` property supports the concept of "URL tokens" - where its value contains some identifier surrounded by curly braces. When a message is sent, the actual URL is calculated by replacing the token with the value of corresponding message header.

```c#
ISender sender = new HttpClientSender("my_sender", "http://foo.com/api/{api_version}/commands");

SenderMessage message = new SenderMessage("Hello, world!");
message.Headers.Add("api_version", "v2");

// The actual URL for the http request will be "http://foo.com/api/v2/commands".
sender.Send(message);
```

*Note that if a sender message does not have a header for each URL token, an exception will be thrown when sending that message.*

[.NET Core example]: ../Example.Messaging.Http.DotNetCore20
[.NET Framework example]: ../Example.Messaging.Http.DotNetFramework451
