using System;
using System.Reflection;
using System.Threading;
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

            // ReSharper disable once EmptyGeneralCatchClause
            private static void VerifyPublicParameterlessConstructor()
            {
                var method = typeof(TheMessageRouterClass).GetMethod("CreateInstance", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(typeof(MessageRouter));

                try
                {
                    method.Invoke(null, null);
                }
                catch
                {
                }
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
            private AutoResetEvent _waitHandle;

            [SetUp]
            public void Setup()
            {
                _router = new MessageRouter();
                _waitHandle = new AutoResetEvent(false);
            }

            [Test]
            public void InstantiatesAnInstanceOfTheMessageHandler()
            {
                var instancesBefore = FooCommand10Handler.Instances;

                _router.Route("<FooCommand10/>", onComplete: result => _waitHandle.Set());

                _waitHandle.WaitOne();

                var instancesAfter = FooCommand10Handler.Instances;

                Assert.That(instancesAfter, Is.EqualTo(instancesBefore + 1));
            }

            [Test]
            public void CallsTheHandleMethodOfTheMessageHandler()
            {
                var handledCountBefore = FooCommand10Handler.HandledCount;

                _router.Route("<FooCommand10/>", onComplete: result => _waitHandle.Set());

                _waitHandle.WaitOne();

                var handledCountAfter = FooCommand10Handler.HandledCount;

                Assert.That(handledCountAfter, Is.EqualTo(handledCountBefore + 1));
            }

            [Test]
            public void CallsTheOnCompleteCallbackWhenNoExceptionIsThrown()
            {
                bool called = false;
                _router.Route("<FooCommand13/>", result => { called = true; _waitHandle.Set(); });

                _waitHandle.WaitOne();

                Assert.That(called, Is.True);
            }

            [Test]
            public void PassesTheDeserializedMessageToTheOnCompleteCallbackWhenNoExceptionIsThrown()
            {
                object message = null;
                _router.Route("<FooCommand10/>", result => { message = result.Message; _waitHandle.Set(); });

                _waitHandle.WaitOne();

                Assert.That(message, Is.InstanceOf<FooCommand10>());
            }

            [Test]
            public void PassesTheResultObjectToTheOnCompleteCallbackWhenNoExceptionIsThrown()
            {
                object result = null;
                _router.Route("<FooCommand15><Who>Clarice</Who></FooCommand15>", r => { result = r.Result; _waitHandle.Set(); });

                _waitHandle.WaitOne();

                Assert.That(result, Is.EqualTo("Hello, Clarice!"));
            }

            [Test]
            public void CallsTheOnCompleteCallbackWhenExceptionIsThrown()
            {
                bool called = false;
                _router.Route("<FooCommand13/>", result => { called = true; _waitHandle.Set(); });

                _waitHandle.WaitOne();

                Assert.That(called, Is.True);
            }

            [Test]
            public void PassesTheThrownExceptionToTheOnCompleteCallbackWhenExceptionIsThrown()
            {
                Exception exception = null;
                _router.Route("<FooCommand13/>", result => { exception = result.Exception; _waitHandle.Set(); });

                _waitHandle.WaitOne();

                Assert.That(exception, Is.Not.Null);
            }

            [Test]
            public void CapturesTheExceptionThrownFromTheGetTypeNameMethodOfTheMessageParser()
            {
                Exception exception = null;
                _router.Route("<FooCom", result => { exception = result.Exception; _waitHandle.Set(); });

                _waitHandle.WaitOne();

                Assert.That(exception, Is.Not.Null);
            }

            [Test]
            public void CapturesTheExceptionThrownFromTheMessageConstructor()
            {
                Exception exception = null;
                _router.Route("<FooCommand11/>", result => { exception = result.Exception; _waitHandle.Set(); });

                _waitHandle.WaitOne();

                Assert.That(exception, Is.Not.Null);
            }

            [Test]
            public void CapturesTheExceptionThrownFromTheMessageHandlerConstructor()
            {
                Exception exception = null;
                _router.Route("<FooCommand12/>", result => { exception = result.Exception; _waitHandle.Set(); });

                _waitHandle.WaitOne();

                Assert.That(exception, Is.Not.Null);
            }

            [Test]
            public void CapturesTheExceptionThrownFromTheHandleMethodOfTheMessageHandler()
            {
                Exception exception = null;
                _router.Route("<FooCommand13/>", result => { exception = result.Exception; _waitHandle.Set(); });

                _waitHandle.WaitOne();

                Assert.That(exception, Is.Not.Null);
            }

            [Test]
            public void DoesNotThrowExceptionWhenExceptionIsThrownFromOnCompleteCallbackUponSuccessfulRouteOperation()
            {
                try
                {
                    var thrown = false;
                    _router.Route("<FooCommand10/>", onComplete: result => { thrown = true; throw new Exception(); });
                    
                    Thread.Sleep(100);
                    Assert.That(thrown, Is.True);
                }
                catch (Exception ex)
                {
                    Assert.Fail("Expected: no exception to be thrown, but was:\r\n" + ex);
                }
            }

            [Test]
            public void DoesNotThrowExceptionWhenExceptionIsThrownFromOnCompleteCallbackUponUnsuccessfulRouteOperation()
            {
                try
                {
                    var thrown = false;
                    _router.Route("<FooCommand13/>", onComplete: result => { thrown = true; throw new Exception(); });
                    
                    Thread.Sleep(100);
                    Assert.That(thrown, Is.True);
                }
                catch (Exception ex)
                {
                    Assert.Fail("Expected: no exception to be thrown, but was:\r\n" + ex);
                }
            }
        }

        public class FooCommand10
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

            public Task<object> Handle(FooCommand10 message)
            {
                HandledCount++;
                return Task.FromResult<object>(null);
            }
        }

        public class FooCommand11
        {
            public FooCommand11()
            {
                throw new Exception();
            }
        }

        public class FooCommand11Handler : IMessageHandler<FooCommand11>
        {
            public Task<object> Handle(FooCommand11 message)
            {
                return Task.FromResult<object>(null);
            }
        }

        public class FooCommand12
        {
        }

        public class FooCommand12Handler : IMessageHandler<FooCommand12>
        {
            public FooCommand12Handler()
            {
                throw new Exception();
            }

            public Task<object> Handle(FooCommand12 message)
            {
                return Task.FromResult<object>(null);
            }
        }

        public class FooCommand13
        {
        }

        public class FooCommand13Handler : IMessageHandler<FooCommand13>
        {
            public Task<object> Handle(FooCommand13 message)
            {
                throw new Exception();
            }
        }

        public class FooCommand14
        {
            public string Bar { get; set; }
        }

        public class FooCommand14Handler : IMessageHandler<FooCommand14>
        {
            public Task<object> Handle(FooCommand14 message)
            {
                return Task.FromResult<object>(null);
            }
        }

        public class FooCommand15
        {
            public string Who { get; set; }
        }

        public class FooCommand15Handler : IMessageHandler<FooCommand15>
        {
            public Task<object> Handle(FooCommand15 message)
            {
                return Task.FromResult<object>(string.Format("Hello, {0}!", message.Who));
            }
        }
    }
}