using NUnit.Framework;

namespace Rock.Messaging
{
    public class ConfigTests
    {
        [Test]
        public void TheMessagingScenarioFactoryDefinedInConfigIsAssignedToDefaultMessagingScenarioFactory()
        {
            Assert.That(MessagingScenarioFactory.Current, Is.InstanceOf<ExampleMessagingScenarioFactory>());

            var factory = (ExampleMessagingScenarioFactory)MessagingScenarioFactory.Current;
            
            Assert.That(factory.IntData, Is.EqualTo(123));
        }
    }
}
