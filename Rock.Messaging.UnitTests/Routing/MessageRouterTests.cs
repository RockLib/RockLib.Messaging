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
            public void InvokesTheCompletion()
            {
                bool called = false;

                _router.Route("<FooCommand10/>", result => called = true).Wait();

                Assert.That(called, Is.True);
            }

            [Test]
            public void PassesTheMessageToTheCompletion()
            {
                FooCommand10 message = null;

                _router.Route("<FooCommand10/>", result => message = result.Message as FooCommand10).Wait();

                Assert.That(message, Is.Not.Null);
                Assert.That(message, Is.InstanceOf<FooCommand10>());
            }

            [Test]
            public void PassesTheExceptionToTheCompletionWhenThereIsAnException()
            {
                Exception exception = null;

                _router.Route("<FooCommand11/>", result => exception = result.Exception).Wait();

                Assert.That(exception, Is.Not.Null);
            }

            [Test]
            public void HandlesAnExceptionThrownFromTheMessageConstructor()
            {
                Exception exception = null;

                _router.Route("<FooCommand11/>", result => exception = result.Exception).Wait();

                _mockExceptionHandler.Verify(m => m.HandleException(It.IsAny<Exception>()), Times.Once());
                Assert.That(exception, Is.Not.Null);
            }

            [Test]
            public void HandlesAnExceptionThrownFromTheMessageHandlerConstructor()
            {
                Exception exception = null;

                _router.Route("<FooCommand12/>", result => exception = result.Exception).Wait();

                _mockExceptionHandler.Verify(m => m.HandleException(It.IsAny<Exception>()), Times.Once());
                Assert.That(exception, Is.Not.Null);
            }

            [Test]
            public void HandlesAnExceptionThrownFromTheHandleMethodOfTheMessageHandler()
            {
                Exception exception = null;

                _router.Route("<FooCommand13/>", result => exception = result.Exception).Wait();

                _mockExceptionHandler.Verify(m => m.HandleException(It.IsAny<Exception>()), Times.Once());
                Assert.That(exception, Is.Not.Null);
            }

            [Test]
            public void HandlesAnExceptionThrownFromTheCompletionMethodOfTheRouteMethod()
            {
                _router.Route("<FooCommand10/>", result => { throw new Exception(); }).Wait();

                _mockExceptionHandler.Verify(m => m.HandleException(It.IsAny<Exception>()), Times.Once());
            }

            [Test]
            public void HandlesAnExceptionResultingFromAnIncompleteMessage()
            {
                Exception exception = null;

                _router.Route("<FooCom", result => exception = result.Exception).Wait();

                _mockExceptionHandler.Verify(m => m.HandleException(It.IsAny<Exception>()), Times.Once());
                Assert.That(exception, Is.Not.Null);
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

            public Task<IMessage> Handle(FooCommand10 message)
            {
                HandledCount++;
                return Task.FromResult<IMessage>(message);
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
            public Task<IMessage> Handle(FooCommand11 message)
            {
                return Task.FromResult<IMessage>(message);
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

            public Task<IMessage> Handle(FooCommand12 message)
            {
                return Task.FromResult<IMessage>(message);
            }
        }

        public class FooCommand13 : IMessage
        {
        }

        public class FooCommand13Handler : IMessageHandler<FooCommand13>
        {
            public Task<IMessage> Handle(FooCommand13 message)
            {
                throw new Exception();
            }
        }
    }
}