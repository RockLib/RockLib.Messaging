using NUnit.Framework;
using Rock.Messaging.Defaults.Implementation;

namespace Rock.Messaging
{
    public class ConfigTests
    {
        [Test]
        public void TheMessagingScenarioFactoryDefinedInConfigIsAssignedToDefaultMessagingScenarioFactory()
        {
            Assert.That(Default.MessagingScenarioFactory, Is.InstanceOf<ExampleMessagingScenarioFactory>());
            
            var factory = (ExampleMessagingScenarioFactory)Default.MessagingScenarioFactory;
            
            Assert.That(factory.IntData, Is.EqualTo(123));
        }
    }
}
