# How to use and configure RockLib.Messaging.SNS

See the [.NET Core example] for a complete demo application.

## SNSSender

The SNSSender class can be directly instantiated and has the following parameters:

- name
  - The name of the instance of SNSSender.
- topicArn
  - The ARN (Amazon Resource Name) of the SNS topic.
- region (optional, defaults to null)
  - The region of the SNS topic.
  
```c#
ISender sender = new SNSSender("MySender", "arn:aws:sns:us-west-2:123456789012:MyTopic", "us-west-2");
```

---

To add an SNSSender to a service collection for dependency injection, use the `AddSNSSender` method, optionally passing in a `configureOptions` callback:

```c#
services.AddSNSSender("MySender", options =>
{
    options.Region = "us-west-2";
    options.TopicArn = "arn:aws:sns:us-west-2:123456789012:MyTopic";
});```

To bind `SNSSenderOptions` to a configuration section, use the name of the sender when calling the `Configure` method:

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<SNSSenderOptions>("MySender", Configuration.GetSection("MySNSSender"));
    services.AddSNSSender("MySender");
}

/* appsettings.json:
{
  "MySNSSender": {
    "Region": "us-west-2",
    "TopicArn": "arn:aws:sns:us-west-2:123456789012:MyTopic"
  }
}
*/
```

---

MessagingScenarioFactory can be configured with an `SNSSender` named "commands" as follows:

```json
{
  "RockLib.Messaging": {
    "Senders": {
      "Type": "RockLib.Messaging.SNS.SNSSender, RockLib.Messaging.SNS",
      "Value": {
        "Name": "commands",
        "TopicArn": "arn:aws:sns:us-west-2:123456789012:MyTopic",
        "Region": "us-west-2"
      }
    }
  }
}
```

```c#
// Note that implementations of the ISender interface are somewhat expensive and intended to be
// long-lived. They are thread-safe, so you can use a single instance throughout your application.
// Instances should be disposed before the application exits.

// MessagingScenarioFactory uses the above JSON configuration to create a SNSSender
// Note that the Value object's properties in the json must map to a valid constructor since CreateSender Creates instances using [RockLib.Configuration.ObjectFactory](https://github.com/RockLib/RockLib.Configuration/tree/main/RockLib.Configuration.ObjectFactory#rocklibconfigurationobjectfactory)
ISender sender = MessagingScenarioFactory.CreateSender("commands");

// Use the sender (for good, not evil):
sender.Send("DROP DATABASE Production;");

// Always dispose the sender when done with it.
sender.Dispose();
```

---

[.NET Core example]: ../Example.Messaging.SNS.DotNetCore20
