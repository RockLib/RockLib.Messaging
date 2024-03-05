using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static RockLib.Messaging.HttpUtils;

namespace RockLib.Messaging.Http
{
    /// <summary>
    /// An implementation of <see cref="ISender" /> that sends messages with an
    /// <see cref="HttpClient"/>.
    /// </summary>
    public class HttpClientSender : ISender
    {
        private readonly HttpContentHeaders _defaultContentHeaders = new ByteArrayContent(Array.Empty<byte>()).Headers;
        private readonly HttpRequestHeaders _defaultRequestHeaders = new HttpRequestMessage().Headers;

        private readonly HttpClient _client;

        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientSender"/> class.
        /// </summary>
        /// <param name="name">The name of the sender.</param>
        /// <param name="url">The url to send messages to.</param>
        /// <param name="method">The http method to use when sending messages.</param>
        /// <param name="defaultHeaders">Default headers that are added to each http request.</param>
        public HttpClientSender(string name, Uri url, string method = "POST", IReadOnlyDictionary<string, string>? defaultHeaders = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Url = url is not null ? url.OriginalString : throw new ArgumentNullException(nameof(url));
            Method = new HttpMethod(method ?? throw new ArgumentNullException(nameof(method)));

            _client = new HttpClient();

            if (defaultHeaders is not null)
            {
                foreach (var header in defaultHeaders)
                {
                    if (IsContentHeader(header.Key))
                    {
                        AddHeader(_defaultContentHeaders, header.Key, header.Value);
                    }
                    else
                    {
                        AddHeader(_defaultRequestHeaders, header.Key, header.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientSender"/> class.
        /// </summary>
        /// <param name="name">The name of the sender.</param>
        /// <param name="url">The url to send messages to.</param>
        /// <param name="method">The http method to use when sending messages.</param>
        /// <param name="defaultHeaders">Default headers that are added to each http request.</param>
        public HttpClientSender(string name, string url, string method = "POST", IReadOnlyDictionary<string, string>? defaultHeaders = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            Method = new HttpMethod(method ?? throw new ArgumentNullException(nameof(method)));

            _client = new HttpClient();

            if (defaultHeaders is not null)
            {
                foreach (var header in defaultHeaders)
                {
                    if (IsContentHeader(header.Key))
                    {
                        AddHeader(_defaultContentHeaders, header.Key, header.Value);
                    }
                    else
                    {
                        AddHeader(_defaultRequestHeaders, header.Key, header.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the name of this instance of <see cref="HttpClientSender"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the url that messages are sent to.
        /// </summary>
#pragma warning disable CA1056 // URI-like properties should not be strings
        public string Url { get; }
#pragma warning restore CA1056 // URI-like properties should not be strings

        /// <summary>
        /// Gets the http method that is used when sending messages.
        /// </summary>
        public HttpMethod Method { get; }

        /// <summary>
        /// Disposes managed resources
        /// </summary>
        /// <param name="disposing">Is this being disposed from <see cref="Dispose()"/> or the finalizer?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _client.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the <see cref="HttpClient"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously sends the specified message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public async Task SendAsync(SenderMessage message, CancellationToken cancellationToken)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(message);
#else
            if (message is null) { throw new ArgumentNullException(nameof(message)); }
#endif

            if (message.OriginatingSystem is null)
            {
                message.OriginatingSystem = "HTTP";
            }

            var headers = new Dictionary<string, object>(message.Headers);

            var url = GetUrl(headers);

            using var request = new HttpRequestMessage(Method, url)
            {
                Content = new ByteArrayContent(message.BinaryPayload)
            };

            foreach (var defaultContentHeader in _defaultContentHeaders)
            {
                if (!message.Headers.ContainsKey(defaultContentHeader.Key))
                {
                    request.Content.Headers.Add(defaultContentHeader.Key, defaultContentHeader.Value);
                }
            }

            foreach (var defaultRequestHeader in _defaultRequestHeaders)
            {
                if (!message.Headers.ContainsKey(defaultRequestHeader.Key))
                {
                    request.Headers.Add(defaultRequestHeader.Key, defaultRequestHeader.Value);
                }
            }

            foreach (var header in message.Headers)
            {
                if (IsContentHeader(header.Key))
                {
                    AddHeader(request.Content.Headers, header.Key, header.Value?.ToString()!);
                }
                else
                {
                    AddHeader(request.Headers, header.Key, header.Value?.ToString()!);
                }
            }

            var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Get the url for the specified message, replacing any tokens with corresponding header
        /// values. If a header is used to replace a token in the url, it is removed from the
        /// <paramref name="headers"/> dictionary.
        /// </summary>
        private string GetUrl(Dictionary<string, object> headers)
        {
            return Regex.Replace(Url, "{([^}]+)}", match =>
            {
                var token = match.Groups[1].Value;

#if NET8_0_OR_GREATER
                if (headers.TryGetValue(token, out var value))
                {
                    headers.Remove(token);
                    return value?.ToString()!;
                }
#else
                if (headers.ContainsKey(token))
                {
                    var value = headers[token];
                    headers.Remove(token);
                    return value?.ToString()!;
                }
#endif

                throw new InvalidOperationException($"The url for this {nameof(HttpClientSender)} contains a token, '{token}', that is not present in the headers of the sender message.");
            });
        }
    }
}
