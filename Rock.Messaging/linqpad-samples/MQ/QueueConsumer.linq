<Query Kind="Program">
  <NuGetReference Prerelease="true">Rock.Core</NuGetReference>
  <NuGetReference Prerelease="true">Rock.Messaging</NuGetReference>
  <Namespace>Rock.Messaging</Namespace>
</Query>

void Main()
{
    using (var consumer = MessagingScenarioFactory.CreateQueueConsumer("my_queue"))
    {
        consumer.MessageReceived += (sender, args) => args.Message.GetStringValue().Dump();
        consumer.Start();
        
        Util.ReadLine("Press enter to exit");
    }
}