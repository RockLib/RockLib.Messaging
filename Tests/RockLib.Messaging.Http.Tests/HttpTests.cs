using System;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;

namespace RockLib.Messaging.Http.Tests
{
    public class HttpTests
    {
        [Fact]
        public void HttpMessagesAreSentAndReceived()
        {
            using (var receiver = new HttpListenerReceiver("foo", "http://localhost:5000/", method: "PUT", requiredHeaders: new RequiredHttpRequestHeaders(contentType: "application/json", accept: "application/json")))
            {
                string payload = null;

                receiver.Start(m =>
                {
                    payload = m.StringPayload;
                    m.Acknowledge();
                });

                using (var sender = new HttpClientSender("foo", "http://localhost:5000/", method: "PUT", defaultHeaders: new Dictionary<string, string> { { "Content-Type", "application/json" }, { "Accept", "application/json" } }))
                {
                    sender.Send("Hello, world!");
                }

                Assert.Equal("Hello, world!", payload);
            }
        }

        [Fact]
        public void HttpMessagesAreSentAndReceivedWhenReceiverDoesRollback()
        {
            using (var receiver = new HttpListenerReceiver("foo", "http://localhost:5000/"))
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
            using (var receiver = new HttpListenerReceiver("foo", "http://localhost:5000/"))
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
            using (var receiver = new HttpListenerReceiver("foo", "http://localhost:5000/"))
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
        public void TokensInHttpListenerReceiverPathAreExtractedIntoHeaders()
        {
            using (var receiver = new HttpListenerReceiver("foo", "http://localhost:5000/api/{api_version}"))
            {
                string payload = null;
                string apiVersion = null;

                receiver.Start(m =>
                {
                    payload = m.StringPayload;
                    apiVersion = m.Headers.GetValue<string>("api_version");
                    m.Acknowledge();
                });

                using (var sender = new HttpClientSender("foo", "http://localhost:5000/API/v2/"))
                {
                    sender.Send("Hello, world!");
                }

                Assert.Equal("Hello, world!", payload);
                Assert.Equal("v2", apiVersion);
            }
        }

        [Fact]
        public void ExtraPathAfterTokenResultIn404()
        {
            using (var receiver = new HttpListenerReceiver("foo", "http://localhost:5000/api/{api_version}"))
            {
                string payload = null;
                string apiVersion = null;

                receiver.Start(m =>
                {
                    payload = m.StringPayload;
                    apiVersion = m.Headers.GetValue<string>("api_version");
                    m.Acknowledge();
                });

                using (var sender = new HttpClientSender("foo", "http://localhost:5000/API/v2/extra"))
                {
                    var exception = Assert.Throws<HttpRequestException>(() => sender.Send("Hello, world!"));
                    Assert.Contains("404 (Not Found)", exception.Message);
                }

                Assert.Null(payload);
            }
        }

        [Fact]
        public void MismatchedMethodsResultsIn405()
        {
            using (var receiver = new HttpListenerReceiver("foo", "http://localhost:5000/", method: "POST"))
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

        [Fact]
        public void MismatchedContentTypeResultsIn415()
        {
            using (var receiver = new HttpListenerReceiver("foo", "http://localhost:5000/", requiredHeaders: new RequiredHttpRequestHeaders(contentType: "application/json")))
            {
                string payload = null;

                receiver.Start(m =>
                {
                    payload = m.StringPayload;
                    m.Acknowledge();
                });

                using (var sender = new HttpClientSender("foo", "http://localhost:5000/", defaultHeaders: new Dictionary<string, string> { { "Content-Type", "application/xml" } }))
                {
                    var exception = Assert.Throws<HttpRequestException>(() => sender.Send("Hello, world!"));
                    Assert.Contains("415 (Unsupported Media Type)", exception.Message);
                }

                Assert.Null(payload);
            }
        }

        [Fact]
        public void MismatchedAcceptResultsIn406()
        {
            using (var receiver = new HttpListenerReceiver("foo", "http://localhost:5000/", requiredHeaders: new RequiredHttpRequestHeaders(accept: "application/json")))
            {
                string payload = null;

                receiver.Start(m =>
                {
                    payload = m.StringPayload;
                    m.Acknowledge();
                });

                using (var sender = new HttpClientSender("foo", "http://localhost:5000/", defaultHeaders: new Dictionary<string, string> { { "Accept", "application/xml" } }))
                {
                    var exception = Assert.Throws<HttpRequestException>(() => sender.Send("Hello, world!"));
                    Assert.Contains("406 (Not Acceptable)", exception.Message);
                }

                Assert.Null(payload);
            }
        }
    }
}
