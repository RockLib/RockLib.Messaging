<Query Kind="Program">
  <NuGetReference Prerelease="true">Rock.Core</NuGetReference>
  <NuGetReference Prerelease="true">Rock.Messaging</NuGetReference>
  <Namespace>Rock.Messaging</Namespace>
</Query>

void Main()
{
    // Create an instance of ISender that uses the queue producer scenario.
    using (var producer = MessagingScenarioFactory.CreateQueueProducer("my_queue"))
    {
        string message;
        while (!string.IsNullOrEmpty(message = Util.ReadLine("Enter a message (leave blank to exit)")))
        {
            // Comment / uncomment below to experiment with sending different
            // kinds of messages.
        
            SendWithHeader(producer, message);
//            SendWithoutHeader(producer, message);
//            SendBinaryWithHeader(producer, Encoding.UTF8.GetBytes(message));
//            SendBinaryWithoutHeader(producer, Encoding.UTF8.GetBytes(message));
        }
    }
}

/// <summary>
/// If you need to set the headers of a string message, create a
/// StringSenderMessage object. Then add values to its Headers property.
/// </summary>
private void SendWithHeader(ISender producer, string message)
{
    StringSenderMessage senderMessage = new StringSenderMessage(message);
    senderMessage.Headers.Add("Foo", "Bar");
    producer.Send(senderMessage);
}

/// <summary>
/// If you don't need to set the headers of the message, just pass
/// the string message to the Send method.
/// </summary>
private void SendWithoutHeader(ISender producer, string message)
{
    producer.Send(message);
}

/// <summary>
/// If you need to set the headers of a binary message, create a
/// BinarySenderMessage object. Then add values to its Headers property.
/// </summary>
private void SendBinaryWithHeader(ISender producer, byte[] message)
{
    BinarySenderMessage senderMessage = new BinarySenderMessage(message);
    senderMessage.Headers.Add("Foo", "Bar");
    producer.Send(senderMessage);
}

/// <summary>
/// If you don't need to set the headers of the message, just pass
/// the binary message to the Send method.
/// </summary>
private void SendBinaryWithoutHeader(ISender producer, byte[] message)
{
    producer.Send(message);
}