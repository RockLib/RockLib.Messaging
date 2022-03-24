using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Messaging.Http.Tests
{
    public class HttpTests
    {
        [Fact]
        public async Task HttpMessagesAreSentAndReceivedUsingUriUrl()
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
                    await sender.SendAsync("Hello, world!").ConfigureAwait(false);
                }

                Assert.Equal("Hello, world!", payload);
            }
        }

        [Fact]
        public async Task HttpMessagesAreSentAndReceivedUsingStringUrlAsync()
        {
            using (var receiver = new HttpListenerReceiver("foo", new Uri("http://localhost:5001/"), method: "PUT", requiredHeaders: new RequiredHttpRequestHeaders(contentType: "application/json", accept: "application/json")))
            {
                string? payload = null;

                receiver.Start(async m =>
                {
                    payload = m.StringPayload;
                    await m.AcknowledgeAsync().ConfigureAwait(false);
                });

                using (var sender = new HttpClientSender("foo", "http://localhost:5001/", method: "PUT", defaultHeaders: new Dictionary<string, string> { { "Content-Type", "application/json" }, { "Accept", "application/json" } }))
                {
                    await sender.SendAsync("Hello, world!").ConfigureAwait(false);
                }

                Assert.Equal("Hello, world!", payload);
            }
        }

        [Fact]
        public async Task HttpMessagesAreSentAndReceivedWhenReceiverDoesRollback()
        {
            using (var receiver = new HttpListenerReceiver("foo", new Uri("http://localhost:5002/")))
            {
                string? payload = null;

                receiver.Start(async m =>
                {
                    payload = m.StringPayload;
                    await m.RollbackAsync().ConfigureAwait(false);
                });

                using (var sender = new HttpClientSender("foo", new Uri("http://localhost:5002/")))
                {
                    await Assert.ThrowsAnyAsync<HttpRequestException>(() => sender.SendAsync("Hello, world!")).ConfigureAwait(false);
                }

                Assert.Equal("Hello, world!", payload);
            }
        }

        [Fact]
        public async Task TokensInHttpClientSenderUrlAreReplacedByMatchingHeaders()
        {
            using (var receiver = new HttpListenerReceiver("foo", new Uri("http://localhost:5003/")))
            {
                string? payload = null;

                receiver.Start(async m =>
                {
                    payload = m.StringPayload;
                    await m.AcknowledgeAsync().ConfigureAwait(false);
                });

                using (var sender = new HttpClientSender("foo", "http://{server}:5003/"))
                {
                    var message = new SenderMessage("Hello, world!") { Headers = { ["server"] = "localhost" } };
                    await sender.SendAsync(message).ConfigureAwait(false);
                }

                Assert.Equal("Hello, world!", payload);
            }
        }

        [Fact]
        public async Task TokensInHttpClientSenderUrlWithoutACorrespondingHeaderThrowsInvalidOperationException()
        {
            using (var receiver = new HttpListenerReceiver("foo", new Uri("http://localhost:5004/")))
            {
                string? payload = null;

                receiver.Start(async m =>
                {
                    payload = m.StringPayload;
                    await m.AcknowledgeAsync().ConfigureAwait(false);
                });

                using (var sender = new HttpClientSender("foo", "http://{server}:5004/"))
                {
                    var message = new SenderMessage("Hello, world!");
                    await Assert.ThrowsAsync<InvalidOperationException>(() => sender.SendAsync(message)).ConfigureAwait(false);
                }

                Assert.Null(payload);
            }
        }

        [Fact]
        public async Task TokensInHttpListenerReceiverPathAreExtractedIntoHeaders()
        {
            using (var receiver = new HttpListenerReceiver("foo", "http://localhost:5005/api/{api_version}"))
            {
                string? payload = null;
                string? apiVersion = null;

                receiver.Start(async m =>
                {
                    payload = m.StringPayload;
                    apiVersion = m.Headers.GetValue<string>("api_version");
                    await m.AcknowledgeAsync().ConfigureAwait(false);
                });

                using (var sender = new HttpClientSender("foo", new Uri("http://localhost:5005/API/v2/")))
                {
                    await sender.SendAsync("Hello, world!").ConfigureAwait(false);
                }

                Assert.Equal("Hello, world!", payload);
                Assert.Equal("v2", apiVersion);
            }
        }

        [Fact]
        public async Task ExtraPathAfterTokenResultIn404()
        {
            using (var receiver = new HttpListenerReceiver("foo", "http://localhost:5006/api/{api_version}"))
            {
                string? payload = null;
                string? apiVersion = null;

                receiver.Start(async m =>
                {
                    payload = m.StringPayload;
                    apiVersion = m.Headers.GetValue<string>("api_version");
                    await m.AcknowledgeAsync().ConfigureAwait(false);
                });

                using (var sender = new HttpClientSender("foo", new Uri("http://localhost:5006/API/v2/extra")))
                {
                    var exception = await Assert.ThrowsAsync<HttpRequestException>(() => sender.SendAsync("Hello, world!")).ConfigureAwait(false);
                    Assert.Contains("404 (Not Found)", exception.Message, StringComparison.InvariantCultureIgnoreCase);
                }

                Assert.Null(payload);
            }
        }

        [Fact]
        public async Task MismatchedMethodsResultsIn405()
        {
            using (var receiver = new HttpListenerReceiver("foo", new Uri("http://localhost:5007/"), method: "POST"))
            {
                string? payload = null;

                receiver.Start(async m =>
                {
                    payload = m.StringPayload;
                    await m.AcknowledgeAsync().ConfigureAwait(false);
                });

                using (var sender = new HttpClientSender("foo", new Uri("http://localhost:5007/"), "PUT"))
                {
                    var exception = await Assert.ThrowsAsync<HttpRequestException>(() => sender.SendAsync("Hello, world!")).ConfigureAwait(false);
                    Assert.Contains("405 (Method Not Allowed)", exception.Message, StringComparison.InvariantCultureIgnoreCase);
                }

                Assert.Null(payload);
            }
        }

        [Fact]
        public async Task MismatchedContentTypeResultsIn415()
        {
            using (var receiver = new HttpListenerReceiver("foo", new Uri("http://localhost:5008/"), requiredHeaders: new RequiredHttpRequestHeaders(contentType: "application/json")))
            {
                string? payload = null;

                receiver.Start(async m =>
                {
                    payload = m.StringPayload;
                    await m.AcknowledgeAsync().ConfigureAwait(false);
                });

                using (var sender = new HttpClientSender("foo", new Uri("http://localhost:5008/"), defaultHeaders: new Dictionary<string, string> { { "Content-Type", "application/xml" } }))
                {
                    var exception = await Assert.ThrowsAsync<HttpRequestException>(() => sender.SendAsync("Hello, world!")).ConfigureAwait(false);
                    Assert.Contains("415 (Unsupported Media Type)", exception.Message, StringComparison.InvariantCultureIgnoreCase);
                }

                Assert.Null(payload);
            }
        }

        [Fact]
        public async Task MismatchedAcceptResultsIn406()
        {
            using (var receiver = new HttpListenerReceiver("foo", new Uri("http://localhost:5009/"), requiredHeaders: new RequiredHttpRequestHeaders(accept: "application/json")))
            {
                string? payload = null;

                receiver.Start(async m =>
                {
                    payload = m.StringPayload;
                    await m.AcknowledgeAsync().ConfigureAwait(false);
                });

                using (var sender = new HttpClientSender("foo", new Uri("http://localhost:5009/"), defaultHeaders: new Dictionary<string, string> { { "Accept", "application/xml" } }))
                {
                    var exception = await Assert.ThrowsAsync<HttpRequestException>(() => sender.SendAsync("Hello, world!")).ConfigureAwait(false);
                    Assert.Contains("406 (Not Acceptable)", exception.Message, StringComparison.InvariantCultureIgnoreCase);
                }

                Assert.Null(payload);
            }
        }
    }
}
