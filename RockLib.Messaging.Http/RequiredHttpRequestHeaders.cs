using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace RockLib.Messaging.Http
{
    /// <summary>
    /// Defines the set of HTTP headers that an instance of <see cref="HttpListenerReceiver"/>
    /// supports. If a header is specified, all incoming HTTP requests must have a header that
    /// matches the 
    /// </summary>
    public class RequiredHttpRequestHeaders
    {
        private readonly IReadOnlyCollection<string> _contentTypeMediaTypes;
        private readonly IReadOnlyCollection<string> _acceptMediaTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredHttpRequestHeaders"/> class.
        /// </summary>
        /// <param name="contentType">
        /// The <c>Content-Type</c> header value(s) that incoming HTTP requests must match in order
        /// to be handled. When specified, any request that does not have a <c>Content-Type</c> header
        /// that matches this value will immediately receive a <c>415 Unsupported Media Type</c>
        /// response. When this value is null, HTTP requests are not filtered according to their
        /// <c>Content-Type</c> header.
        /// </param>
        /// <param name="accept">
        /// The <c>Accept</c> header value(s) that incoming HTTP requests must match in order to be
        /// handled. When specified, any request that does not have a <c>Accept</c> header that
        /// matches this value will immediately receive a <c>406 Not Acceptable</c> response. When
        /// this value is null, HTTP requests are not filtered according to their <c>Accept</c> header.
        /// </param>
        public RequiredHttpRequestHeaders(string contentType = null, string accept = null)
        {
            _contentTypeMediaTypes = contentType?.Split(',').Select(ct => MediaTypeHeaderValue.Parse(ct).MediaType).ToList();
            _acceptMediaTypes = accept?.Split(',').Select(ct => MediaTypeWithQualityHeaderValue.Parse(ct).MediaType).ToList();

            ContentType = contentType;
            Accept = accept;
        }

        /// <summary>
        /// Gets the <c>Content-Type</c> header value(s) that incoming HTTP requests must match in order
        /// to be handled. When specified, any request that does not have a <c>Content-Type</c> header
        /// that matches this value will immediately receive a <c>415 Unsupported Media Type</c>
        /// response. When this value is null, HTTP requests are not filtered according to their
        /// <c>Content-Type</c> header.
        /// </summary>
        public string ContentType { get; }

        /// <summary>
        /// Gets the <c>Accept</c> header value(s) that incoming HTTP requests must match in order to
        /// be handled. When specified, any request that does not have a <c>Accept</c> header that
        /// matches this value will immediately receive a <c>406 Not Acceptable</c> response. When this
        /// value is null, HTTP requests are not filtered according to their <c>Accept</c> header.
        /// </summary>
        public string Accept { get; }

        internal bool AllowsContentType(string requestContentType)
        {
            if (_contentTypeMediaTypes == null)
                return true;

            try
            {
                var contentType = MediaTypeHeaderValue.Parse(requestContentType);
                return _contentTypeMediaTypes.Any(mediaType => contentType.MediaType == mediaType);
            }
            catch
            {
            }

            return false;
        }

        internal bool AllowsAccept(string[] requestAcceptTypes)
        {
            if (_acceptMediaTypes == null)
                return true;

            try
            {
                foreach (var requestAcceptType in requestAcceptTypes)
                {
                    var accept = MediaTypeWithQualityHeaderValue.Parse(requestAcceptType);
                    if (_acceptMediaTypes.Any(mediaType => accept.MediaType == mediaType))
                        return true;
                }
            }
            catch
            {
            }

            return false;
        }
    }
}
