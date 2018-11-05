using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RockLib.Messaging.Http
{
    public class HttpClientSender : ISender
    {
        private readonly HttpClient _client;

        public HttpClientSender(string name, string url)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            _client = new HttpClient();
        }

        public string Name { get; }
        public string Url { get; }

        public void Dispose()
        {
            _client.Dispose();
        }

        public async Task SendAsync(SenderMessage message)
        {
            if (message.OriginatingSystem == null)
                message.OriginatingSystem = "HTTP";

            var request = new HttpRequestMessage(HttpMethod.Post, Url)
            {
                Content = message.IsBinary
                    ? new ByteArrayContent(message.BinaryPayload)
                    : new StringContent(message.StringPayload)
            };

            foreach (var header in message.Headers)
            {
                // TODO: better validation?
                request.Headers.TryAddWithoutValidation(header.Key, header.Value?.ToString()); // TODO: better string conversion
            }

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}
