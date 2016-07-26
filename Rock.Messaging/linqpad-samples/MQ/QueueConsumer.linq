<Query Kind="Program">
  <NuGetReference>Rock.Messaging</NuGetReference>
  <Namespace>Rock.Messaging</Namespace>
  <Namespace>Rock.Messaging.NamedPipes</Namespace>
</Query>

void Main()
{
    // For this example, use named pipes as the messaging scenario.
    MessagingScenarioFactory.SetCurrent(new NamedPipeMessagingScenarioFactory());

    // Create an instance of IReceiver that uses the queue consumer scenario.
    using (var consumer = MessagingScenarioFactory.CreateQueueConsumer("my_queue"))
    {
        // Register to receive messages.
        consumer.MessageReceived += OnMessageReceived;
        
        // Start listening for messages.
        consumer.Start();
        
        Util.ReadLine("Press enter to exit");
    }
}

private void OnMessageReceived(object sender, MessageReceivedEventArgs args)
{
    // To get the contents of the message, call GetStringMessage or
    // GetBinaryMessage on the args' Message property.
    string message = args.Message.GetStringValue();
    
    // To retrieve the value of a header, call GetHeaderValue.
    var fooHeader = args.Message.GetHeaderValue("Foo");
    
    Console.WriteLine("Message: {0}", message);
    if (fooHeader != null)
    {
        Console.WriteLine("\tFoo Header: {0}", fooHeader);
    }
    
    // Acknowledge the message when finished with it.
    args.Message.Acknowledge();
}