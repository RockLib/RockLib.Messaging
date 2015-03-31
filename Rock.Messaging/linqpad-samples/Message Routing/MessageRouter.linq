<Query Kind="Program">
  <NuGetReference Prerelease="true">Rock.Messaging</NuGetReference>
  <Namespace>Rock.Messaging</Namespace>
  <Namespace>Rock.Messaging.Routing</Namespace>
  <Namespace>Rock.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

void Main()
{
    // The MessageRouter class can be used to handle generic string messages.

    MessageRouter router = new MessageRouter();
    
    // Simulate the receiving of messages.
    foreach (string rawMessage in GetRawMessages())
    {
        // When a raw string message is passed to the message router,
        // the router takes care of:
        //   - Figuring out what type of message it is.
        //   - Creating and initializing an instance of that message type.
        //   - Figuring out what type of message handler to use.
        //   - Getting an instance of the message handler type.
        //   - Passing the message to the message handler.
        
        router.Route(rawMessage);
    }

    // By default, the MessageRouter class assumes the messages to be xml, but
    // a custom parser can be passed to the constructor to customize the message
    // format.
    
    // The mechanism for determining the message type and message handler type
    // can also be customized via the constructor.
    
    // MessageRouter also works with Rock.Core's dependency injection interface,
    // IResolver. If an instance of an IResolver implementation is passed to
    // MessageRouter's constructor, then MessageRouter will use the IResolver
    // to create the message and message handler instances.
}

/// <summary>
/// This method simulates the receiving of raw string messages.
/// </summary>
private IEnumerable<string> GetRawMessages()
{
    yield return "<HelloMessage><Recipient>world</Recipient></HelloMessage>";
    yield return "<GoodbyeMessage><Recipient>world</Recipient></GoodbyeMessage>";
}

/// <summary>
/// A message class for saying hello. This class could be defined in a
/// common library, used by many applications.
/// </summary>
public class HelloMessage
{
    public string Recipient { get; set; }
}

/// <summary>
/// A message class for saying goodbye. This class could be defined in a
/// common library, used by many applications.
/// </summary>
public class GoodbyeMessage
{
    public string Recipient { get; set; }
}

/// <summary>
/// A handler for HelloMessage objects. This class could be defined in an
/// application or application-specific library, with a reference to the
/// library that defines the HelloMessage class.
/// </summary>
public class HelloMessageHandler : IMessageHandler<HelloMessage>
{
    public Task<object> Handle(HelloMessage message)
    {
        // Our handler just writes a hello message to console.
        Console.WriteLine("Hello, {0}!", message.Recipient);
        
        // Return a completed task with a null result.
        return Task.FromResult<object>(null);
    }
}

/// <summary>
/// A handler for GoodbyeMessage objects. This class could be defined in an
/// application or application-specific library, with a reference to the
/// library that defines the HelloMessage class.
/// </summary>
public class GoodbyeMessageHandler : IMessageHandler<GoodbyeMessage>
{
    public Task<object> Handle(GoodbyeMessage message)
    {
        // Our handler just writes a goodbye message to console.
        Console.WriteLine("Goodbye, cruel {0}!", message.Recipient);
        
        // Return a completed task with a null result.
        return Task.FromResult<object>(null);
    }
}