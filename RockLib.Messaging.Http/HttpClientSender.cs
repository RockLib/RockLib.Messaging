using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.Http
{
    public class HttpClientSender : ISender
    {
        private readonly HttpClient _client;

        public HttpClientSender(string name, string url, string method = "POST")
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            Method = new HttpMethod(method ?? throw new ArgumentNullException(nameof(method)));
            _client = new HttpClient();
        }

        public string Name { get; }
        public string Url { get; }
        public HttpMethod Method { get; }

        public void Dispose()
        {
            _client.Dispose();
        }

        public async Task SendAsync(SenderMessage message, CancellationToken cancellationToken)
        {
            if (message.OriginatingSystem == null)
                message.OriginatingSystem = "HTTP";

            var headers = new Dictionary<string, object>(message.Headers);

            var url = GetUrl(headers);

            var request = new HttpRequestMessage(Method, url)
            {
                Content = message.IsBinary || message.IsCompressed
                    ? new ByteArrayContent(message.BinaryPayload)
                    : new StringContent(message.StringPayload)
            };

            // TODO: if the message is compressed, add the correct http compression header

            foreach (var header in headers)
            {
                if (header.Value != null)
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToString());
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

                if (headers.ContainsKey(token))
                {
                    var value = headers[token];
                    headers.Remove(token);
                    return value?.ToString();
                }

                throw new InvalidOperationException($"The url for this {nameof(HttpClientSender)} contains a token, '{token}', that is not present in the headers of the sender message.");
            });
        }
    }
}
