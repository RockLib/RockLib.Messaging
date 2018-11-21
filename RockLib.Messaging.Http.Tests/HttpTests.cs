using System;
using System.Net.Http;
using Xunit;

namespace RockLib.Messaging.Http.Tests
{
    public class HttpTests
    {
        [Fact]
        public void HttpMessagesAreSentAndReceived()
        {
            using (var receiver = new HttpListenerReceiver("foo", new[] { "http://localhost:5000/" }))
            {
                string payload = null;

                receiver.Start(m =>
                {
                    payload = m.StringPayload;
                    m.Acknowledge();
                });

                using (var sender = new HttpClientSender("foo", "http://localhost:5000/"))
                {
                    sender.Send("Hello, world!");
                }

                Assert.Equal("Hello, world!", payload);
            }
        }

        [Fact]
        public void HttpMessagesAreSendAndReceivedWhenReceiverDoesRollback()
        {
            using (var receiver = new HttpListenerReceiver("foo", new[] { "http://localhost:5000/" }))
            {
                string payload = null;

                receiver.Start(m =>
                {
                    payload = m.StringPayload;
                    m.Rollback();
                });

                using (var sender = new HttpClientSender("foo", "http://localhost:5000/"))
                {
                    Assert.ThrowsAny<HttpRequestException>(() => sender.Send("Hello, world!"));
                }

                Assert.Equal("Hello, world!", payload);
            }
        }

        [Fact]
        public void TokensInHttpClientSenderUrlAreReplacedByMatchingHeaders()
        {
            using (var receiver = new HttpListenerReceiver("foo", new[] { "http://localhost:5000/" }))
            {
                string payload = null;

                receiver.Start(m =>
                {
                    payload = m.StringPayload;
                    m.Acknowledge();
                });

                using (var sender = new HttpClientSender("foo", "http://{server}:5000/"))
                {
                    var message = new SenderMessage("Hello, world!") { Headers = { ["server"] = "localhost" } };
                    sender.Send(message);
                }

                Assert.Equal("Hello, world!", payload);
            }
        }

        [Fact]
        public void TokensInHttpClientSenderUrlWithoutACorrespondingHeaderThrowsInvalidOperationException()
        {
            using (var receiver = new HttpListenerReceiver("foo", new[] { "http://localhost:5000/" }))
            {
                string payload = null;

                receiver.Start(m =>
                {
                    payload = m.StringPayload;
                    m.Acknowledge();
                });

                using (var sender = new HttpClientSender("foo", "http://{server}:5000/"))
                {
                    var message = new SenderMessage("Hello, world!");
                    Assert.Throws<InvalidOperationException>(() => sender.Send(message));
                }

                Assert.Null(payload);
            }
        }

        [Fact]
        public void MismatchedMethodsResultsIn405()
        {
            using (var receiver = new HttpListenerReceiver("foo", new[] { "http://localhost:5000/" }, "POST"))
            {
                string payload = null;

                receiver.Start(m =>
                {
                    payload = m.StringPayload;
                    m.Acknowledge();
                });

                using (var sender = new HttpClientSender("foo", "http://localhost:5000/", "PUT"))
                {
                    var exception = Assert.Throws<HttpRequestException>(() => sender.Send("Hello, world!"));
                    Assert.Contains("405 (Method Not Allowed)", exception.Message);
                }

                Assert.Null(payload);
            }
        }
    }
}
