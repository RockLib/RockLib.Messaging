using System;
using Moq;
using NUnit.Framework;
using Rock.Messaging.Routing;

namespace RouteWithCallbacksExtensionMethod
{
    public class TheRouteMethod
    {
        [Test]
        public async void PassesTheMessageReturnedFromTheInterfaceRouteMethodToTheOnSuccessCallback()
        {
            var messageFromRouter = new TestMessage();

            var mockRouter = new Mock<IMessageRouter>();
            mockRouter.Setup(m => m.Route(It.IsAny<string>())).ReturnsAsync(messageFromRouter);

            IMessage message = null;
            await mockRouter.Object.Route("", m => message = m);

            Assert.That(message, Is.Not.Null);
            Assert.That(message, Is.SameAs(messageFromRouter));
        }

        [Test]
        public async void PassesTheCaughtExceptionThrownFromTheInterfaceRouteMethodToTheOnFailureCallback()
        {
            var exceptionFromRouter = new Exception();

            var mockRouter = new Mock<IMessageRouter>();
            mockRouter.Setup(m => m.Route(It.IsAny<string>())).ThrowsAsync(exceptionFromRouter);

            Exception exception = null;
            await mockRouter.Object.Route("", onFailue: ex => exception = ex);

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception, Is.SameAs(exceptionFromRouter));
        }

        [Test]
        public async void InvokesTheOnCompletedCallbackWhenTheInterfaceRouteMethodIsSuccessful()
        {
            var mockRouter = new Mock<IMessageRouter>();
            mockRouter.Setup(m => m.Route(It.IsAny<string>())).ReturnsAsync(new TestMessage());

            bool called = false;
            await mockRouter.Object.Route("", onComplete: () => called = true);

            Assert.That(called, Is.True);
        }

        [Test]
        public async void InvokesTheOnCompletedCallbackWhenTheInterfaceRouteMethodIsUnsuccessful()
        {
            var mockRouter = new Mock<IMessageRouter>();
            mockRouter.Setup(m => m.Route(It.IsAny<string>())).ThrowsAsync(new Exception());

            bool called = false;
            await mockRouter.Object.Route("", onComplete: () => called = true);

            Assert.That(called, Is.True);
        }

        private class TestMessage : IMessage
        {
        }
    }
}
