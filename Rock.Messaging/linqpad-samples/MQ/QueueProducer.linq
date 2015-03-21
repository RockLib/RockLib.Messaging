<Query Kind="Program">
  <NuGetReference Prerelease="true">Rock.Core</NuGetReference>
  <NuGetReference Prerelease="true">Rock.Messaging</NuGetReference>
  <Namespace>Rock.Messaging</Namespace>
</Query>

void Main()
{
    using (var producer = MessagingScenarioFactory.CreateQueueProducer("my_queue"))
    {
        string message;
        while (!string.IsNullOrEmpty(message = Util.ReadLine("Enter a message (leave blank to exit)")))
        {
            producer.Send(message);
        }
    }
}