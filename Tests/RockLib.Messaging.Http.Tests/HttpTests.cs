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
            using (var receiver = new HttpListenerReceiver("foo", new Uri("http://localhost:5000/"), method: "PUT", requiredHeaders: new RequiredHttpRequestHeaders(contentType: "application/json", accept: "application/json")))
            {
                string? payload = null;

                receiver.Start(async m =>
                {
                    payload = m.StringPayload;
                    await m.AcknowledgeAsync().ConfigureAwait(false);
                });

                using (var sender = new HttpClientSender("foo", new Uri("http://localhost:5000/"), method: "PUT", defaultHeaders: new Dictionary<string, string> { { "Content-Type", "application/json" }, { "Accept", "application/json" } }))
                {
                    sender.SendAsync("Hello, world!");
                }

                Assert.Equal("Hello, world!", payload);
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task HttpMessagesAreSentAndReceivedWhenReceiverDoesRollback()
        {
            using (var receiver = new HttpListenerReceiver("foo", new Uri("http://localhost:5000/")))
            {
                string? payload = null;

                receiver.Start(async m =>
                {
                    payload = m.StringPayload;
                    await m.RollbackAsync().ConfigureAwait(false);
                });

                using (var sender = new HttpClientSender("foo", new Uri("http://localhost:5000/")))
                {
                    await Assert.ThrowsAnyAsync<HttpRequestException>(() => sender.SendAsync("Hello, world!")).ConfigureAwait(false);
                }

                Assert.Equal("Hello, world!", payload);
            }
        }

        [Fact]
        public void TokensInHttpClientSenderUrlAreReplacedByMatchingHeaders()
        {
            using (var receiver = new HttpListenerReceiver("foo", new Uri("http://localhost:5000/")))
            {
                string? payload = null;

                receiver.Start(async m =>
                {
                    payload = m.StringPayload;
                    await m.AcknowledgeAsync().ConfigureAwait(false);
                });

                using (var sender = new HttpClientSender("foo", new Uri("http://{server}:5000/")))
                {
                    var message = new SenderMessage("Hello, world!") { Headers = { ["server"] = "localhost" } };
                    sender.SendAsync(message);
                }

                Assert.Equal("Hello, world!", payload);
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task TokensInHttpClientSenderUrlWithoutACorrespondingHeaderThrowsInvalidOperationException()
        {
            using (var receiver = new HttpListenerReceiver("foo", new Uri("http://localhost:5000/")))
            {
                string? payload = null;

                receiver.Start(async m =>
                {
                    payload = m.StringPayload;
                    await m.AcknowledgeAsync().ConfigureAwait(false);
                });

                using (var sender = new HttpClientSender("foo", new Uri("http://{server}:5000/")))
                {
                    var message = new SenderMessage("Hello, world!");
                    await Assert.ThrowsAsync<InvalidOperationException>(() => sender.SendAsync(message)).ConfigureAwait(false);
                }

                Assert.Null(payload);
            }
        }

        [Fact]
        public void TokensInHttpListenerReceiverPathAreExtractedIntoHeaders()
        {
            using (var receiver = new HttpListenerReceiver("foo", new Uri("http://localhost:5000/api/{api_version}")))
            {
                string? payload = null;
                string? apiVersion = null;

                receiver.Start(async m =>
                {
                    payload = m.StringPayload;
                    apiVersion = m.Headers.GetValue<string>("api_version");
                    await m.AcknowledgeAsync().ConfigureAwait(false);
                });

                using (var sender = new HttpClientSender("foo", new Uri("http://localhost:5000/API/v2/")))
                {
                    sender.SendAsync("Hello, world!");
                }

                Assert.Equal("Hello, world!", payload);
                Assert.Equal("v2", apiVersion);
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task ExtraPathAfterTokenResultIn404()
        {
            using (var receiver = new HttpListenerReceiver("foo", new Uri("http://localhost:5000/api/{api_version}")))
            {
                string? payload = null;
                string? apiVersion = null;

                receiver.Start(async m =>
                {
                    payload = m.StringPayload;
                    apiVersion = m.Headers.GetValue<string>("api_version");
                    await m.AcknowledgeAsync().ConfigureAwait(false);
                });

                using (var sender = new HttpClientSender("foo", new Uri("http://localhost:5000/API/v2/extra")))
                {
                    var exception = await Assert.ThrowsAsync<HttpRequestException>(() => sender.SendAsync("Hello, world!")).ConfigureAwait(false);
                    Assert.Contains("404 (Not Found)", exception.Message, StringComparison.InvariantCultureIgnoreCase);
                }

                Assert.Null(payload);
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task MismatchedMethodsResultsIn405()
        {
            using (var receiver = new HttpListenerReceiver("foo", new Uri("http://localhost:5000/"), method: "POST"))
            {
                string? payload = null;

                receiver.Start(async m =>
                {
                    payload = m.StringPayload;
                    await m.AcknowledgeAsync().ConfigureAwait(false);
                });

                using (var sender = new HttpClientSender("foo", new Uri("http://localhost:5000/"), "PUT"))
                {
                    var exception = await Assert.ThrowsAsync<HttpRequestException>(() => sender.SendAsync("Hello, world!")).ConfigureAwait(false);
                    Assert.Contains("405 (Method Not Allowed)", exception.Message, StringComparison.InvariantCultureIgnoreCase);
                }

                Assert.Null(payload);
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task MismatchedContentTypeResultsIn415()
        {
            using (var receiver = new HttpListenerReceiver("foo", new Uri("http://localhost:5000/"), requiredHeaders: new RequiredHttpRequestHeaders(contentType: "application/json")))
            {
                string? payload = null;

                receiver.Start(async m =>
                {
                    payload = m.StringPayload;
                    await m.AcknowledgeAsync().ConfigureAwait(false);
                });

                using (var sender = new HttpClientSender("foo", new Uri("http://localhost:5000/"), defaultHeaders: new Dictionary<string, string> { { "Content-Type", "application/xml" } }))
                {
                    var exception = await Assert.ThrowsAsync<HttpRequestException>(() => sender.SendAsync("Hello, world!")).ConfigureAwait(false);
                    Assert.Contains("415 (Unsupported Media Type)", exception.Message, StringComparison.InvariantCultureIgnoreCase);
                }

                Assert.Null(payload);
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task MismatchedAcceptResultsIn406()
        {
            using (var receiver = new HttpListenerReceiver("foo", new Uri("http://localhost:5000/"), requiredHeaders: new RequiredHttpRequestHeaders(accept: "application/json")))
            {
                string? payload = null;

                receiver.Start(async m =>
                {
                    payload = m.StringPayload;
                    await m.AcknowledgeAsync().ConfigureAwait(false);
                });

                using (var sender = new HttpClientSender("foo", new Uri("http://localhost:5000/"), defaultHeaders: new Dictionary<string, string> { { "Accept", "application/xml" } }))
                {
                    var exception = await Assert.ThrowsAsync<HttpRequestException>(() => sender.SendAsync("Hello, world!")).ConfigureAwait(false);
                    Assert.Contains("406 (Not Acceptable)", exception.Message, StringComparison.InvariantCultureIgnoreCase);
                }

                Assert.Null(payload);
            }
        }
    }
}
