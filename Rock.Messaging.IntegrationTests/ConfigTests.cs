using System.Linq;
using NUnit.Framework;

namespace Rock.Messaging
{
    public class ConfigTests
    {
        [Test]
        public void TheMessagingScenarioFactoriesDefinedInConfigAreAssignedToDefaultMessagingScenarioFactory()
        {
            Assert.That(MessagingScenarioFactory.Current, Is.InstanceOf<CompositeMessagingScenarioFactory>());

            var factory = (CompositeMessagingScenarioFactory)MessagingScenarioFactory.Current;

            Assert.That(factory.Factories, Has.Length.EqualTo(2));

            Assert.That(factory.Factories, Has.Exactly(1).InstanceOf<ExampleMessagingScenarioFactory>());
            Assert.That(factory.Factories, Has.Exactly(1).InstanceOf<AnotherMessagingScenarioFactory>());

            var exampleFactory = factory.Factories.OfType<ExampleMessagingScenarioFactory>().Single();
            var anotherFactory = factory.Factories.OfType<AnotherMessagingScenarioFactory>().Single();

            Assert.That(exampleFactory.IntData, Is.EqualTo(123));
            Assert.That(anotherFactory.BoolData, Is.True);
        }
    }
}
