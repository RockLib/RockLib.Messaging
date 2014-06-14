using System;
using System.Reflection;
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
        public class TheMessageRouterClass
        {
            // It's important for MessageRouter to have a public parameterless constructor so that it can
            // be used as a generic argument where the 'new()' type constraint is used. This also ensures
			// that it can be used by Activator.CreateInstance without having to specify paramters.
            [Test]
            public void HasAPublicParameterlessConstructor()
            {
                Assert.That(VerifyPublicParameterlessConstructor, Throws.Nothing);
            }

            private static void VerifyPublicParameterlessConstructor()
            {
                var method = typeof(TheMessageRouterClass).GetMethod("CreateInstance", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(typeof(MessageRouter));
                method.Invoke(null, null);
            }

            // ReSharper disable once UnusedMember.Local
            private static T CreateInstance<T>()
                where T : new()
            {
                return new T();
            }
        }

        public class TheRouteMethod : MessageRouterTests
        {
            private Mock<IExceptionHandler> _mockExceptionHandler;
            private MessageRouter _router;

            [SetUp]
            public void Setup()
            {
                _mockExceptionHandler = new Mock<IExceptionHandler>();
                _router = new MessageRouter(exceptionHandler: _mockExceptionHandler.Object);
            }

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
            public void InvokesTheRegisteredDefaultCompletion()
            {
                var called = false;
                _router.RegisterDefaultCompletion(() => { called = true; });

                _router.Route("<FooCommand10/>").Wait();

                Assert.That(called, Is.True);
            }

            [Test]
            public void InvokesTheLastRegisteredDefaultCompletion()
            {
                var firstCalled = false;
                var lastCalled = false;
                _router.RegisterDefaultCompletion(() => { firstCalled = true; });
                _router.RegisterDefaultCompletion(() => { lastCalled = true; });

                _router.Route("<FooCommand10/>").Wait();

                Assert.That(firstCalled, Is.False);
                Assert.That(lastCalled, Is.True);
            }

            [Test]
            public void InvokesTheRegisteredCompletionForTheMessageType()
            {
                var called = false;
                _router.RegisterCompletion<FooCommand10>(fooCommand10 => { called = true; });

                _router.Route("<FooCommand10/>").Wait();

                Assert.That(called, Is.True);
            }

            [Test]
            public void InvokesTheLastRegisteredCompletionForTheMessageType()
            {
                var firstCalled = false;
                var lastCalled = false;
                _router.RegisterCompletion<FooCommand10>(fooCommand10 => { firstCalled = true; });
                _router.RegisterCompletion<FooCommand10>(fooCommand10 => { lastCalled = true; });

                _router.Route("<FooCommand10/>").Wait();

                Assert.That(firstCalled, Is.False);
                Assert.That(lastCalled, Is.True);
            }

            [Test]
            public void InvokesBothTheRegisteredDefaultCompletionAndTheRegisteredCompletionForTheMessageType()
            {
                var defaultCalled = false;
                var genericCalled = false;
                _router.RegisterDefaultCompletion(() => { defaultCalled = true; });
                _router.RegisterCompletion<FooCommand10>(fooCommand10 => { genericCalled = true; });

                _router.Route("<FooCommand10/>").Wait();

                Assert.That(defaultCalled, Is.True);
                Assert.That(genericCalled, Is.True);
            }

            [Test]
            public void InvokesTheCompletionParameter()
            {
                var called = false;

                _router.RegisterDefaultCompletion(() => { });
                _router.RegisterCompletion<FooCommand10>(command => {});

                _router.Route("<FooCommand10/>", () => called = true).Wait();

                Assert.That(called, Is.True);
            }

            [Test]
            public void HandlesAnExceptionThrownFromTheMessageConstructor()
            {
                _router.Route("<FooCommand11/>").Wait();

                _mockExceptionHandler.Verify(m => m.HandleException(It.IsAny<Exception>()), Times.Once());
            }

            [Test]
            public void HandlesAnExceptionThrownFromTheMessageHandlerConstructor()
            {
                _router.Route("<FooCommand12/>").Wait();

                _mockExceptionHandler.Verify(m => m.HandleException(It.IsAny<Exception>()), Times.Once());
            }

            [Test]
            public void HandlesAnExceptionThrownFromTheHandleMethodOfTheMessageHandler()
            {
                _router.Route("<FooCommand13/>").Wait();

                _mockExceptionHandler.Verify(m => m.HandleException(It.IsAny<Exception>()), Times.Once());
            }

            [Test]
            public void HandlesAnExceptionThrownFromTheDefaultCompletionMethod()
            {
                _router.RegisterDefaultCompletion(() => { throw new Exception(); });

                _router.Route("<FooCommand10/>").Wait();

                _mockExceptionHandler.Verify(m => m.HandleException(It.IsAny<Exception>()), Times.Once());
            }

            [Test]
            public void HandlesAnExceptionThrownFromTheGenericCompletionMethod()
            {
                _router.RegisterCompletion<FooCommand10>(command => { throw new Exception(); });

                _router.Route("<FooCommand10/>").Wait();

                _mockExceptionHandler.Verify(m => m.HandleException(It.IsAny<Exception>()), Times.Once());
            }

            [Test]
            public void HandlesAnExceptionThrownFromTheCompletionMethodOfTheRouteMethod()
            {
                _router.Route("<FooCommand10/>", () => { throw new Exception(); }).Wait();

                _mockExceptionHandler.Verify(m => m.HandleException(It.IsAny<Exception>()), Times.Once());
            }

            [Test]
            public void HandlesAnExceptionResultingFromAnIncompleteMessage()
            {
                _router.Route("<FooCom").Wait();

                _mockExceptionHandler.Verify(m => m.HandleException(It.IsAny<Exception>()), Times.Once());
            }

            [Test]
            public void DoesNotThrowAnExceptionWhenTheExceptionHandlerItselfThrowsAnException()
            {
                _mockExceptionHandler.Setup(m => m.HandleException(It.IsAny<Exception>())).Throws<Exception>();
                _router.RegisterDefaultCompletion(() => { throw new Exception(); });

                Assert.That(() => _router.Route("<FooCommand10/>").Wait(), Throws.Nothing);
                _mockExceptionHandler.Verify(m => m.HandleException(It.IsAny<Exception>()), Times.Once());
            }
        }

        public class FooCommand10 : IMessage
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

        public class FooCommand11 : IMessage
        {
            public FooCommand11()
            {
                throw new Exception();
            }
        }

        public class FooCommand11Handler : IMessageHandler<FooCommand11>
        {
            public Task<FooCommand11> Handle(FooCommand11 message)
            {
                return Task.FromResult(message);
            }
        }

        public class FooCommand12 : IMessage
        {
        }

        public class FooCommand12Handler : IMessageHandler<FooCommand12>
        {
            public FooCommand12Handler()
            {
                throw new Exception();
            }

            public Task<FooCommand12> Handle(FooCommand12 message)
            {
                return Task.FromResult(message);
            }
        }

        public class FooCommand13 : IMessage
        {
        }

        public class FooCommand13Handler : IMessageHandler<FooCommand13>
        {
            public Task<FooCommand13> Handle(FooCommand13 message)
            {
                throw new Exception();
            }
        }
    }
}