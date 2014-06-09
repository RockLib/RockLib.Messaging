using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Rock;
using Rock.Messaging.Routing;

// ReSharper disable once CheckNamespace
namespace MessageRouterTests
{
    public class MessageRouterTests
    {
        protected Mock<IExceptionHandler> _mockExceptionHandler;
        protected MessageRouter _router;

        [SetUp]
        public void Setup()
        {
            _mockExceptionHandler = new Mock<IExceptionHandler>();
            _router = new MessageRouter(exceptionHandler:_mockExceptionHandler.Object);

        }

        public class TheRouteMethod : MessageRouterTests
        {
            [Test]
            public void InstantiatesAnInstanceOfTheMessageHandler()
            {
                var instancesBefore = FooCommand10Handler.Instances;

                _router.Route("<FooCommand10/>").Wait();

                var instancesAfter = FooCommand10Handler.Instances;

                Assert.That(instancesAfter, Is.EqualTo(instancesBefore + 1));
            }

            [Test]
            public void CallsTheHandleMethodOfTheMessageHandler()
            {
                var handledCountBefore = FooCommand10Handler.HandledCount;

                _router.Route("<FooCommand10/>").Wait();

                var handledCountAfter = FooCommand10Handler.HandledCount;

                Assert.That(handledCountAfter, Is.EqualTo(handledCountBefore + 1));
            }

            [Test]
            public void InvokesTheRegisteredDefaultCompletionWhenThereIsNoCompletionRegisteredForTheMessageType()
            {
                var called = false;
                _router.RegisterDefaultCompletion(() => { called = true; });

                _router.Route("<FooCommand10/>").Wait();

                Assert.That(called, Is.True);
            }

            [Test]
            public void InvokesTheRegisteredCompletionForTheMessageType()
            {
                var called = false;
                _router.RegisterCompletion<FooCommand10>(fooCommand9 => { called = true; });

                _router.Route("<FooCommand10/>").Wait();

                Assert.That(called, Is.True);
            }

            // ReSharper disable ExplicitCallerInfoArgument
            [Test]
            public void HandlesAnExceptionWhenTheHandlerMethodOfTheMessageHandlerThrowsOne()
            {
                _router.Route("<FooCommand11/>").Wait();

                _mockExceptionHandler.Verify(m => m.HandleException(It.IsAny<Exception>()), Times.Once());
            }

            [Test]
            public void HandlesAnExceptionWhenTheCompletionActionThrowsAnExceptionOne()
            {
                _router.RegisterCompletion<FooCommand10>(fooCommand9 => { throw new Exception(); });

                _router.Route("<FooCommand10/>").Wait();

                _mockExceptionHandler.Verify(m => m.HandleException(It.IsAny<Exception>()), Times.Once());
            }
            // ReSharper restore ExplicitCallerInfoArgument
        }

        public class FooCommand10 : IMessage
        {
        }

        public class FooCommand11 : IMessage
        {
        }

        public class FooCommand10Handler : IMessageHandler<FooCommand10>
        {
            public FooCommand10Handler()
            {
                Instances++;
            }

            public static int Instances { get; private set; }
            public static int HandledCount { get; private set; }

            public Task<FooCommand10> Handle(FooCommand10 message)
            {
                HandledCount++;
                return Task.FromResult(message);
            }
        }

        public class FooCommand11Handler : IMessageHandler<FooCommand11>
        {
            public Task<FooCommand11> Handle(FooCommand11 message)
            {
                throw new Exception();
            }
        }
    }
}