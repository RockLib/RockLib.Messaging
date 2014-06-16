using System;
using Moq;
using NUnit.Framework;
using Rock.Messaging.Routing;

// ReSharper disable once CheckNamespace
namespace RouteWithCallbacksExtensionMethod
{
    public class TheRouteMethod
    {
        [Test]
        public void ThrowsAnArgumentNullExceptionIfTheMessageRouterIsNull()
        {
            IMessageRouter messageRouter = null;

            Assert.That(async () => await messageRouter.Route("", message => {}), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void DoesNotThrowAnExceptionIfOnSuccessCallbackThrowsAnException()
        {
            var mockRouter = new Mock<IMessageRouter>();
            mockRouter.Setup(m => m.Route(It.IsAny<string>())).ReturnsAsync(new RouteResult(new TestMessage()));

            var thrown = false;
            Assert.That(async () => await mockRouter.Object.Route("", message => { thrown = true; throw new Exception(); }), Throws.Nothing);
            Assert.That(thrown, Is.True);
        }

        [Test]
        public void DoesNotThrowAnExceptionIfOnFailureCallbackThrowsAnException()
        {
            var mockRouter = new Mock<IMessageRouter>();
            mockRouter.Setup(m => m.Route(It.IsAny<string>())).ReturnsAsync(new RouteResult(new Exception()));

            var thrown = false;
            Assert.That(async () => await mockRouter.Object.Route("", onFailure:exception => { thrown = true; throw new Exception(); }), Throws.Nothing);
            Assert.That(thrown, Is.True);
        }

        [Test]
        public void DoesNotThrowAnExceptionIfOnCompleteCallbackThrowsAnException()
        {
            var mockRouter = new Mock<IMessageRouter>();
            mockRouter.Setup(m => m.Route(It.IsAny<string>())).ReturnsAsync(new RouteResult(new TestMessage()));

            var thrown = false;
            Assert.That(async () => await mockRouter.Object.Route("", onComplete:() => { thrown = true; throw new Exception(); }), Throws.Nothing);
            Assert.That(thrown, Is.True);
        }

        [Test]
        public async void InvokesTheOnSuccessCallbackWhenTheInterfaceRouteMethodReturnsASuccessfulResult()
        {
            var messageFromRouter = new TestMessage();

            var mockRouter = new Mock<IMessageRouter>();
            mockRouter.Setup(m => m.Route(It.IsAny<string>())).ReturnsAsync(new RouteResult(messageFromRouter));

            IMessage message = null;
            await mockRouter.Object.Route("", m => message = m);

            Assert.That(message, Is.SameAs(messageFromRouter));
        }

        [Test]
        public async void InvokesTheOnFailureCallbackWhenTheInterfaceRouteMethodReturnsAnUnsuccessfulResult()
        {
            var thrownException = new Exception();

            var mockRouter = new Mock<IMessageRouter>();
            mockRouter.Setup(m => m.Route(It.IsAny<string>())).ReturnsAsync(new RouteResult(thrownException));

            Exception exception = null;
            await mockRouter.Object.Route("", onFailure: ex => exception = ex);

            Assert.That(exception, Is.SameAs(thrownException));
        }

        [Test]
        public async void InvokesTheOnFailureCallbackWhenTheInterfaceRouteMethodThrowsAnException()
        {
            var thrownException = new Exception();

            var mockRouter = new Mock<IMessageRouter>();
            mockRouter.Setup(m => m.Route(It.IsAny<string>())).ThrowsAsync(thrownException);

            Exception exception = null;
            await mockRouter.Object.Route("", onFailure: ex => exception = ex);

            Assert.That(exception, Is.SameAs(thrownException));
        }

        [Test]
        public async void InvokesTheOnCompletedCallbackWhenTheInterfaceRouteMethodReturnsASuccessfulResult()
        {
            var mockRouter = new Mock<IMessageRouter>();
            mockRouter.Setup(m => m.Route(It.IsAny<string>())).ReturnsAsync(new RouteResult(new TestMessage()));

            bool called = false;
            await mockRouter.Object.Route("", onComplete: () => called = true);

            Assert.That(called, Is.True);
        }

        [Test]
        public async void InvokesTheOnCompletedCallbackWhenTheInterfaceRouteMethodReturnsAnUnsuccessfulResult()
        {
            var mockRouter = new Mock<IMessageRouter>();
            mockRouter.Setup(m => m.Route(It.IsAny<string>())).ReturnsAsync(new RouteResult(new Exception()));

            bool called = false;
            await mockRouter.Object.Route("", onComplete: () => called = true);

            Assert.That(called, Is.True);
        }

        [Test]
        public async void InvokesTheOnCompletedCallbackWhenTheInterfaceRouteMethodThrowsAnException()
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
