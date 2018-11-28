using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace RockLib.Messaging.Http
{
    /// <summary>
    /// An implementation of <see cref="IReceiver"/> that receives http
    /// messages with an <see cref="HttpListener"/>. See
    /// https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistener for
    /// more information.
    /// </summary>
    public class HttpListenerReceiver : Receiver
    {
        /// <summary>The default status code for messages that are acknowledged.</summary>
        public const int DefaultAcknowledgeStatusCode = 200;
        /// <summary>The default status code for messages that are rolled back.</summary>
        public const int DefaultRollbackStatusCode = 500;
        /// <summary>The default status code for messages that are rejected.</summary>
        public const int DefaultRejectStatusCode = 400;
        /// <summary>The default http method to listen for.</summary>
        public const string DefaultMethod = "POST";

        private readonly HttpListener _listener = new HttpListener();

        private readonly MediaTypeHeaderValue _contentType;

        private readonly Regex _pathRegex;
        private readonly IReadOnlyCollection<string> _pathTokens;

        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListenerReceiver"/> class.
        /// </summary>
        /// <param name="name">The name of the receiver.</param>
        /// /// <param name="url">
        /// The url that the <see cref="HttpListener"/> should listen to. See
        /// https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistener for
        /// more information.
        /// </param>
        /// <param name="acknowledgeStatusCode">
        /// The status code to be returned to the client when a message is acknowledged.
        /// </param>
        /// <param name="rollbackStatusCode">
        /// The status code to be returned to the client when a message is rolled back.
        /// </param>
        /// <param name="rejectStatusCode">
        /// The status code to be returned to the client when a message is acknowledged.
        /// </param>
        /// <param name="method">
        /// The http method that requests must have in order to be handled. Any request
        /// that does not have this method will receive a 405 Method Not Allowed response.
        /// </param>
        /// <param name="contentType">
        /// The content type that requests must match in order to be handled. If not null, any request whose
        /// content type does not match this value will receive a 415 Unsupported Media Type response.
        /// </param>
        public HttpListenerReceiver(string name, string url,
            int acknowledgeStatusCode = DefaultAcknowledgeStatusCode,
            int rollbackStatusCode = DefaultRollbackStatusCode,
            int rejectStatusCode = DefaultRejectStatusCode,
            string method = DefaultMethod, string contentType = null)
            : this(name, url,
                  new DefaultHttpResponseGenerator(acknowledgeStatusCode, rollbackStatusCode, rejectStatusCode),
                  method, contentType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListenerReceiver"/> class.
        /// </summary>
        /// <param name="name">The name of the receiver.</param>
        /// <param name="prefixes">
        /// The URI prefixes handled by the <see cref="HttpListener"/>. See
        /// https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistener for
        /// more information.
        /// </param>
        /// <param name="path">
        /// The path that requests must match in order to be handled. Any request whose
        /// path does not match this value will receive a 404 Not Found response.
        /// </param>
        /// <param name="acknowledgeStatusCode">
        /// The status code to be returned to the client when a message is acknowledged.
        /// </param>
        /// <param name="rollbackStatusCode">
        /// The status code to be returned to the client when a message is rolled back.
        /// </param>
        /// <param name="rejectStatusCode">
        /// The status code to be returned to the client when a message is acknowledged.
        /// </param>
        /// <param name="method">
        /// The http method that requests must have in order to be handled. Any request
        /// that does not have this method will receive a 405 Method Not Allowed response.
        /// </param>
        /// <param name="contentType">
        /// The content type that requests must match in order to be handled. If not null, any request whose
        /// content type does not match this value will receive a 415 Unsupported Media Type response.
        /// </param>
        public HttpListenerReceiver(string name, IReadOnlyList<string> prefixes, string path,
            int acknowledgeStatusCode = DefaultAcknowledgeStatusCode,
            int rollbackStatusCode = DefaultRollbackStatusCode,
            int rejectStatusCode = DefaultRejectStatusCode,
            string method = DefaultMethod, string contentType = null)
            : this(name, prefixes, path,
                new DefaultHttpResponseGenerator(acknowledgeStatusCode, rollbackStatusCode, rejectStatusCode),
                method, contentType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListenerReceiver"/> class.
        /// </summary>
        /// <param name="name">The name of the receiver.</param>
        /// <param name="url">
        /// The url that the <see cref="HttpListener"/> should listen to. See
        /// https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistener for
        /// more information.
        /// </param>
        /// <param name="httpResponseGenerator">
        /// An object that determines the http response that is returned to clients,
        /// depending on whether the message is acknowledged, rejected, or rolled back.
        /// </param>
        /// <param name="method">
        /// The http method that requests must have in order to be handled. Any request
        /// that does not have this method will receive a 405 Method Not Allowed response.
        /// </param>
        /// <param name="contentType">
        /// The content type that requests must match in order to be handled. If not null, any request whose
        /// content type does not match this value will receive a 415 Unsupported Media Type response.
        /// </param>
        public HttpListenerReceiver(string name, string url,
            IHttpResponseGenerator httpResponseGenerator, string method = DefaultMethod, string contentType = null)
            : this(name, GetPrefixes(url), GetPath(url), httpResponseGenerator, method, contentType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListenerReceiver"/> class.
        /// </summary>
        /// <param name="name">The name of the receiver.</param>
        /// <param name="prefixes">
        /// The URI prefixes handled by the <see cref="HttpListener"/>. See
        /// https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistener for
        /// more information.
        /// </param>
        /// <param name="path">
        /// The path that requests must match in order to be handled. Any request whose
        /// path does not match this value will receive a 404 Not Found response.
        /// </param>
        /// <param name="httpResponseGenerator">
        /// An object that determines the http response that is returned to clients,
        /// depending on whether the message is acknowledged, rejected, or rolled back.
        /// </param>
        /// <param name="method">
        /// The http method that requests must have in order to be handled. Any request
        /// that does not have this method will receive a 405 Method Not Allowed response.
        /// </param>
        /// <param name="contentType">
        /// The content type that requests must match in order to be handled. If not null, any request whose
        /// content type does not match this value will receive a 415 Unsupported Media Type response.
        /// </param>
        public HttpListenerReceiver(string name, IReadOnlyList<string> prefixes, string path,
            IHttpResponseGenerator httpResponseGenerator, string method = DefaultMethod, string contentType = null)
            : base(name)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            Prefixes = prefixes ?? throw new ArgumentNullException(nameof(prefixes));
            foreach (var prefix in Prefixes)
                _listener.Prefixes.Add(prefix);

            Path = path.Trim('/');
            var pathTokens = new List<string>();
            var pathPattern = "^/?" + Regex.Replace(Path ?? "", "{([^}]+)}", m =>
            {
                var token = m.Groups[1].Value;
                pathTokens.Add(token);
                return $"(?<{token}>.*?)";
            }) + "/?$";
            _pathRegex = new Regex(pathPattern, RegexOptions.IgnoreCase);
            _pathTokens = pathTokens;

            HttpResponseGenerator = httpResponseGenerator ?? throw new ArgumentNullException(nameof(httpResponseGenerator));
            Method = method ?? throw new ArgumentNullException(nameof(method));

            if (contentType != null)
            {
                try
                {
                    _contentType = MediaTypeHeaderValue.Parse(contentType);
                }
                catch (FormatException ex)
                {
                    throw new ArgumentException("Invalid value for 'Content-Type' header.", nameof(contentType), ex);
                }
            }
        }

        /// <summary>
        /// The object that determines the http response that is returned to clients,
        /// depending on whether the message is acknowledged, rejected, or rolled back.
        /// </summary>
        public IHttpResponseGenerator HttpResponseGenerator { get; }

        /// <summary>
        /// Gets the http method that requests must have in order to be handled. Any request
        /// that does not have this method will receive a 405 Method Not Allowed response.
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Gets the URI prefixes handled by the <see cref="HttpListener"/>. See
        /// https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistener for
        /// more information.
        /// </summary>
        public IReadOnlyList<string> Prefixes { get; }

        /// <summary>
        /// Gets the path that requests must match in order to be handled. Any request whose
        /// path does not match this value will receive a 404 Not Found response.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the content type that requests must match in order to be handled. If not null, any
        /// request whose content type does not match this value will receive a 415 Unsupported Media
        /// Type response.
        /// </summary>
        public string ContentType => _contentType?.ToString();

        /// <inheritdoc />
        protected override void Start()
        {
            _listener.Start();
            _listener.BeginGetContext(CompleteGetContext, null);
        }

        private void CompleteGetContext(IAsyncResult result)
        {
            if (disposed)
                return;

            var context = _listener.EndGetContext(result);

            _listener.BeginGetContext(CompleteGetContext, null);

            if (!_pathRegex.IsMatch(context.Request.Url.AbsolutePath))
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
                return;
            }

            if (context.Request.HttpMethod != Method)
            {
                context.Response.StatusCode = 405;
                context.Response.Close();
                return;
            }

            if (_contentType != null)
            {
                MediaTypeHeaderValue requestContentType;

                try
                {
                    requestContentType = MediaTypeHeaderValue.Parse(context.Request.ContentType);
                }
                catch
                {
                    requestContentType = null;
                }

                if (requestContentType == null || requestContentType.MediaType != _contentType.MediaType)
                {
                    context.Response.StatusCode = 415;
                    context.Response.Close();
                    return;
                }
            }

            MessageHandler.OnMessageReceived(this, new HttpListenerReceiverMessage(context, HttpResponseGenerator, _pathRegex, _pathTokens));
        }
        
        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            disposed = true;
            _listener.Stop();
            base.Dispose(disposing);
            _listener.Close();
        }

        private static IReadOnlyList<string> GetPrefixes(string url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            var match = Regex.Match(url, ".*?(?={[^}]+})");

            if (match.Success)
                return new[] { match.Value };
            return new[] { url.Trim('/') + '/' };
        }

        private static string GetPath(string url)
        {
            var uri = new Uri(url.Replace('*', '-').Replace('+', '-'));
            return uri.LocalPath;
        }
    }
}
