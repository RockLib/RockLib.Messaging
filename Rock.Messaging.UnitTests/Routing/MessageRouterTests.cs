using System;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
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
            private MessageRouter _router;

            [SetUp]
            public void Setup()
            {
                _router = new MessageRouter();
            }

            [Test]
            public async void InstantiatesAnInstanceOfTheMessageHandler()
            {
                var instancesBefore = FooCommand10Handler.Instances;

                await _router.Route("<FooCommand10/>");

                var instancesAfter = FooCommand10Handler.Instances;

                Assert.That(instancesAfter, Is.EqualTo(instancesBefore + 1));
            }

            [Test]
            public async void CallsTheHandleMethodOfTheMessageHandler()
            {
                var handledCountBefore = FooCommand10Handler.HandledCount;

                await _router.Route("<FooCommand10/>");

                var handledCountAfter = FooCommand10Handler.HandledCount;

                Assert.That(handledCountAfter, Is.EqualTo(handledCountBefore + 1));
            }

            [Test]
            public async void ReturnsTheMessageThatWasSuccessfullyHandled()
            {
                var message = (await _router.Route("<FooCommand14><Bar>abc123</Bar></FooCommand14>")).Message as FooCommand14;

                Assert.That(message, Is.Not.Null); // ReSharper disable once PossibleNullReferenceException
                Assert.That(message.Bar, Is.EqualTo("abc123"));
            }

            [Test]
            public async void HandlesAnExceptionThrownFromTheMessageConstructor()
            {
                var exception = (await _router.Route("<FooCommand11/>")).Exception;

                Assert.That(exception, Is.Not.Null);
            }

            [Test]
            public async void HandlesAnExceptionThrownFromTheMessageHandlerConstructor()
            {
                var exception = (await _router.Route("<FooCommand12/>")).Exception;

                Assert.That(exception, Is.Not.Null);
            }

            [Test]
            public async void HandlesAnExceptionThrownFromTheHandleMethodOfTheMessageHandler()
            {
                var exception = (await _router.Route("<FooCommand13/>")).Exception;

                Assert.That(exception, Is.Not.Null);
            }

            [Test]
            public async void HandlesAnExceptionResultingFromAnIncompleteMessage()
            {
                var exception = (await _router.Route("<FooCom")).Exception;

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

        public class FooCommand14 : IMessage
        {
            public string Bar { get; set; }
        }

        public class FooCommand14Handler : IMessageHandler<FooCommand14>
        {
            public Task<IMessage> Handle(FooCommand14 message)
            {
                return Task.FromResult<IMessage>(message);
            }
        }
    }
}