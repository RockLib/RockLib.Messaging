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
            Assert.That(() => new RouteResult((IMessage)null), Throws.InstanceOf<ArgumentNullException>());
        }
    }

    public class TheExceptionConstructor
    {
        [Test]
        public void ThrowsAnArgumentNullExceptionIfTheExceptionParameterIsNull()
        {
            Assert.That(() => new RouteResult((Exception)null), Throws.InstanceOf<ArgumentNullException>());
        }
    }

    public class TheSuccessProperty
    {
        [Test]
        public void ReturnsTrueWhenTheMessagePropertyIsNotNull()
        {
            var result = new RouteResult(new TestMessage());

            Assert.That(result.Success, Is.True);
        }

        [Test]
        public void ReturnsFalseWhenTheExceptionPropertyIsNotNull()
        {
            var result = new RouteResult(new Exception());

            Assert.That(result.Success, Is.False);
        }

        private class TestMessage : IMessage
        {
        }
    }
}