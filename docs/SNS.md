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

MessagingScenarioFactory can be configured with an `SNSSender` named "commands" as follows:

```json
{
  "RockLib.Messaging": {
    "Senders": {
      "Type": "RockLib.Messaging.SNS.SNSSender, RockLib.Messaging.SNS",
      "Value": {
        "Name": "commands",
        "TopicArn": "TODO: Set Topic ARN",
        "Region": "TODO: Set Region"
      }
    }
  }
}
```

```c#
// Note that implementations of the ISender interface are somewhat expensive and intended to be
// long-lived. They are thread-safe, so you can use a single instance throughout your application.
// Instances should be disposed before the application exits.

// MessagingScenarioFactory uses the above JSON configuration to create a SNSSender:
ISender sender = MessagingScenarioFactory.CreateSender("commands");

// SNSSender can also be instantiated directly:
// ISender sender = new SNSSender("commands", "TODO: Set Topic ARN", "TODO: Set Region");

// Use the sender (for good, not evil):
sender.Send("DROP DATABASE Production;");

// Always dispose the sender when done with it.
sender.Dispose();
```

---

[.NET Core example]: ../Example.Messaging.SNS.DotNetCore20