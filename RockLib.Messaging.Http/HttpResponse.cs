using System;
using System.Collections.Generic;

namespace RockLib.Messaging.Http
{
    /// <summary>
    /// Defines an http response.
    /// </summary>
    public class HttpResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponse"/> class.
        /// </summary>
        /// <param name="statusCode">The http status code of the response.</param>
        /// <param name="statusDescription">The http status description of the response.</param>
        /// <param name="content">The string content of the response.</param>
        public HttpResponse(int statusCode, string content = null, string statusDescription = null)
            : this(statusCode, statusDescription, (object)content)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponse"/> class.
        /// </summary>
        /// <param name="statusCode">The http status code of the response.</param>
        /// <param name="statusDescription">The http status description of the response.</param>
        /// <param name="content">The binary content of the response.</param>
        public HttpResponse(int statusCode, byte[] content, string statusDescription = null)
            : this(statusCode, statusDescription, (object)content)
        {
        }

        private HttpResponse(int statusCode, string statusDescription, object content)
        {
            if (statusCode < 100 || statusCode > 999)
                throw new ArgumentException("statusCode cannot be less than 100 or greater than 999.", nameof(statusCode));
            StatusCode = statusCode;
            StatusDescription = statusDescription;
            Content = content;
        }

        /// <summary>
        /// Gets the status code of the response.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// Gets the status description of the response.
        /// </summary>
        public string StatusDescription { get; }

        /// <summary>
        /// Gets the content of the response.
        /// </summary>
        public object Content { get; }

        /// <summary>
        /// Gets a dictionary representing the headers of the response.
        /// </summary>
        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
    }
}
