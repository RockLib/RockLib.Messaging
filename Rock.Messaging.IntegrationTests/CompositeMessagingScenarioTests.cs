using NUnit.Framework;

namespace Rock.Messaging
{
    [TestFixture]
    class CompositeMessagingScenarioTests
    {
        [Test]
        public void IfMultipleFactoriesMatchTheProvidedNameThenTheFirstFactoryToAppearInConfigIsSelected()
        {
            // There are three factories in config, in this order:
            // ExamplMessagingScenarioFactory
            //     HasScenario returns true when the name parameter is "foo"
            // AnotherMessagingScenarioFactory
            //     HasScenario returns true when the name parameter is "foo" or "bar"
            // PermissiveMessagingScenarioFactory
            //     HasScenario returns true when the name parameter is *not* "qux"

            var testReceiver = (TestReceiver)MessagingScenarioFactory.CreateQueueConsumer("foo");

            Assert.That(testReceiver.Name, Is.EqualTo("foo"));
            Assert.That(testReceiver.FactoryType, Is.EqualTo(typeof(ExampleMessagingScenarioFactory)));

            testReceiver = (TestReceiver)MessagingScenarioFactory.CreateQueueConsumer("bar");

            Assert.That(testReceiver.Name, Is.EqualTo("bar"));
            Assert.That(testReceiver.FactoryType, Is.EqualTo(typeof(AnotherMessagingScenarioFactory)));

            testReceiver = (TestReceiver)MessagingScenarioFactory.CreateQueueConsumer("baz");

            Assert.That(testReceiver.Name, Is.EqualTo("baz"));
            Assert.That(testReceiver.FactoryType, Is.EqualTo(typeof(PermissiveMessagingScenarioFactory)));
        }

        [Test]
        public void IfNoFactoriesMatchTheProvidedNameThenAnInvalidOperationExceptionIsThrown()
        {
            Assert.That(() => MessagingScenarioFactory.CreateQueueConsumer("qux"), Throws.InvalidOperationException);
        }
    }
}
