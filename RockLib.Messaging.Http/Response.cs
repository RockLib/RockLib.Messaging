using System;
using System.Collections.Generic;

namespace RockLib.Messaging.Http
{
    public class Response
    {
        public Response(int statusCode, string statusDescription, string content = null)
            : this(statusCode, statusDescription, (object)content)
        {
        }

        public Response(int statusCode, string statusDescription, byte[] content)
            : this(statusCode, statusDescription, (object)content)
        {
        }

        private Response(int statusCode, string statusDescription, object content)
        {
            if (statusCode < 100 || statusCode > 999)
                throw new ArgumentException("statusCode cannot be less than 100 or greater than 999.", nameof(statusCode));
            StatusCode = statusCode;
            StatusDescription = statusDescription ?? throw new ArgumentNullException(nameof(statusDescription));
            Content = content;
        }

        public int StatusCode { get; }
        public string StatusDescription { get; }
        public object Content { get; }
        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
    }
}
