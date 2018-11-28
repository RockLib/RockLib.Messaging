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

#### URL Tokens

The HttpClientSender supports the concept of "URL tokens" - where the URL contains some identifier surrounded by curly braces. When a message is sent, the actual URL is calculated by replacing the token with the value of corresponding message header.

```c#
ISender sender = new HttpClientSender("my_sender", "http://foo.com/api/{api_version}/commands");

SenderMessage message = new SenderMessage("Hello, world!");
message.Headers.Add("api_version", "v1");

// The actual URL for this message is "http://foo.com/api/v1/commands"
sender.Send(message);
```

*If a sender message does not have a header for each URL token, an exception will be thrown when sending that message.*

[.NET Core example]: ../Example.Messaging.Http.DotNetCore20
[.NET Framework example]: ../Example.Messaging.Http.DotNetFramework451
