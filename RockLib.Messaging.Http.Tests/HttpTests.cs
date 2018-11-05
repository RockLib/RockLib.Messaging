using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
    }
}
