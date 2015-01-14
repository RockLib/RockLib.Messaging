using System;
using NUnit.Framework;
using Rock.Messaging.Routing;

namespace RouteResultTests
{
    public class TheMessageConstructor
    {
        [Test]
        public void ThrowsAnArgumentNullExceptionIfTheMessageParameterIsNull()
        {
            Assert.That(() => new RouteResult(null, new object()), Throws.InstanceOf<ArgumentNullException>());
        }
    
        [Test]
        public void DoesNotThrowAnExceptionIfTheResultParameterIsNull()
        {
            Assert.That(() => new RouteResult(new TestMessage(), null), Throws.Nothing);
        }

        [Test]
        public void SetsTheValueOfTheMessageProperty()
        {
            var message = new TestMessage();

            var routeResult = new RouteResult(message, null);

            Assert.That(routeResult.Message, Is.SameAs(message));
        }

        [Test]
        public void SetsTheValueOfTheResultProperty()
        {
            var result = new object();

            var routeResult = new RouteResult(new TestMessage(), result);

            Assert.That(routeResult.Result, Is.SameAs(result));
        }

        private class TestMessage
        {
        }
    }

    public class TheExceptionConstructor
    {
        [Test]
        public void ThrowsAnArgumentNullExceptionIfTheExceptionParameterIsNull()
        {
            Assert.That(() => new RouteResult((Exception)null), Throws.InstanceOf<ArgumentNullException>());
        }

        [Test]
        public void SetsTheValueOfTheExceptionProperty()
        {
            var exception = new Exception();

            var routeResult = new RouteResult(exception);

            Assert.That(routeResult.Exception, Is.SameAs(exception));
        }
    }

    public class TheSuccessProperty
    {
        [Test]
        public void ReturnsTrueWhenTheMessagePropertyIsNotNull()
        {
            var result = new RouteResult(new TestMessage(), null);

            Assert.That(result.Success, Is.True);
        }

        [Test]
        public void ReturnsFalseWhenTheExceptionPropertyIsNotNull()
        {
            var result = new RouteResult(new Exception());

            Assert.That(result.Success, Is.False);
        }

        private class TestMessage
        {
        }
    }
}