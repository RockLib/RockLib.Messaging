﻿using System;
using System.Collections.Generic;
using System.Net;
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

        private readonly Regex _pathRegex;
        private readonly IReadOnlyCollection<string> _pathTokens;

        private bool _disposed;

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
        /// <param name="requiredHeaders">
        /// The HTTP headers that incoming requests are required to match in order to be handled.
        /// Any request that does not have match the required headers will receive a 4xx response.
        /// </param>
        public HttpListenerReceiver(string name, Uri url,
            int acknowledgeStatusCode = DefaultAcknowledgeStatusCode,
            int rollbackStatusCode = DefaultRollbackStatusCode,
            int rejectStatusCode = DefaultRejectStatusCode,
            string method = DefaultMethod, RequiredHttpRequestHeaders? requiredHeaders = null) : base(name)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(url);
#else
            if (url is null) { throw new ArgumentNullException(nameof(url)); }
#endif

            Prefixes = GetPrefixes(url.OriginalString);
            foreach (var prefix in Prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }

            Path = url.AbsolutePath.Trim('/');

            var pathTokens = new List<string>();
            var pathPattern = "^/?" + Regex.Replace(Path ?? "", "{([^}]+)}", m =>
            {
                var token = m.Groups[1].Value;
                pathTokens.Add(token);
                return $"(?<{token}>[^/]+)";
            }) + "/?$";
            _pathRegex = new Regex(pathPattern, RegexOptions.IgnoreCase);
            _pathTokens = pathTokens;

            HttpResponseGenerator = new DefaultHttpResponseGenerator(acknowledgeStatusCode, rollbackStatusCode, rejectStatusCode);
            Method = method ?? throw new ArgumentNullException(nameof(method));
            RequiredHeaders = requiredHeaders;
        }

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
        /// <param name="requiredHeaders">
        /// The HTTP headers that incoming requests are required to match in order to be handled.
        /// Any request that does not have match the required headers will receive a 4xx response.
        /// </param>
#pragma warning disable CA1054 // URI-like parameters should not be strings
        public HttpListenerReceiver(string name, string url,
#pragma warning restore CA1054 // URI-like parameters should not be strings
            int acknowledgeStatusCode = DefaultAcknowledgeStatusCode,
            int rollbackStatusCode = DefaultRollbackStatusCode,
            int rejectStatusCode = DefaultRejectStatusCode,
            string method = DefaultMethod, RequiredHttpRequestHeaders? requiredHeaders = null)
            : this(name, url,
                  new DefaultHttpResponseGenerator(acknowledgeStatusCode, rollbackStatusCode, rejectStatusCode),
                  method, requiredHeaders)
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
        /// <param name="requiredHeaders">
        /// The HTTP headers that incoming requests are required to match in order to be handled.
        /// Any request that does not have match the required headers will receive a 4xx response.
        /// </param>
        public HttpListenerReceiver(string name, IReadOnlyList<string> prefixes, string path,
            int acknowledgeStatusCode = DefaultAcknowledgeStatusCode,
            int rollbackStatusCode = DefaultRollbackStatusCode,
            int rejectStatusCode = DefaultRejectStatusCode,
            string method = DefaultMethod, RequiredHttpRequestHeaders? requiredHeaders = null)
            : this(name, prefixes, path,
                new DefaultHttpResponseGenerator(acknowledgeStatusCode, rollbackStatusCode, rejectStatusCode),
                method, requiredHeaders)
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
        /// <param name="requiredHeaders">
        /// The HTTP headers that incoming requests are required to match in order to be handled.
        /// Any request that does not have match the required headers will receive a 4xx response.
        /// </param>
#pragma warning disable CA1054 // URI-like parameters should not be strings
        public HttpListenerReceiver(string name, string url,
#pragma warning restore CA1054 // URI-like parameters should not be strings
            IHttpResponseGenerator httpResponseGenerator, string method = DefaultMethod, RequiredHttpRequestHeaders? requiredHeaders = null)
            : this(name, GetPrefixes(url), GetPath(url), httpResponseGenerator, method, requiredHeaders)
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
        /// <param name="requiredHeaders">
        /// The HTTP headers that incoming requests are required to match in order to be handled.
        /// Any request that does not have match the required headers will receive a 4xx response.
        /// </param>
        public HttpListenerReceiver(string name, IReadOnlyList<string> prefixes, string path,
            IHttpResponseGenerator httpResponseGenerator, string method = DefaultMethod, RequiredHttpRequestHeaders? requiredHeaders = null)
            : base(name)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(path);
#else
            if (path is null) { throw new ArgumentNullException(nameof(path)); }
#endif

            Prefixes = prefixes ?? throw new ArgumentNullException(nameof(prefixes));
            foreach (var prefix in Prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }

            Path = path.Trim('/');
            var pathTokens = new List<string>();
            var pathPattern = "^/?" + Regex.Replace(Path ?? "", "{([^}]+)}", m =>
            {
                var token = m.Groups[1].Value;
                pathTokens.Add(token);
                return $"(?<{token}>[^/]+)";
            }) + "/?$";
            _pathRegex = new Regex(pathPattern, RegexOptions.IgnoreCase);
            _pathTokens = pathTokens;

            HttpResponseGenerator = httpResponseGenerator ?? throw new ArgumentNullException(nameof(httpResponseGenerator));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            RequiredHeaders = requiredHeaders;
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
        /// Gets the HTTP headers that incoming requests are required to match in order to be handled.
        /// Any request that does not have match the required headers will receive a 4xx response.
        /// </summary>
        public RequiredHttpRequestHeaders? RequiredHeaders { get; }

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
        public string? Path { get; }

        /// <inheritdoc />
        protected override void Start()
        {
            _listener.Start();
            _listener.BeginGetContext(CompleteGetContext, null);
        }

        private void CompleteGetContext(IAsyncResult result)
        {
            if (_disposed)
            {
                return;
            }

            var context = _listener.EndGetContext(result);

            _listener.BeginGetContext(CompleteGetContext, null);

            if (!_pathRegex.IsMatch(context.Request.Url!.AbsolutePath))
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

            if (RequiredHeaders != null)
            {
                if (!RequiredHeaders.AllowsContentType(context.Request.ContentType))
                {
                    context.Response.StatusCode = 415;
                    context.Response.Close();
                    return;
                }

                if (!RequiredHeaders.AllowsAccept(context.Request.AcceptTypes))
                {
                    context.Response.StatusCode = 406;
                    context.Response.Close();
                    return;
                }
            }

            try
            {
                using var receiverMessage = new HttpListenerReceiverMessage(context, HttpResponseGenerator, _pathRegex, _pathTokens);
                MessageHandler?.OnMessageReceivedAsync(this, receiverMessage).GetAwaiter().GetResult();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                OnError("Error in MessageHandler.OnMessageReceivedAsync.", ex);
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _listener.Stop();
            base.Dispose(disposing);
            _listener.Close();
        }

        private static string[] GetPrefixes(string url)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(url);
#else
            if (url is null) { throw new ArgumentNullException(nameof(url)); }
#endif

            var match = Regex.Match(url, ".*?(?={[^}]+})");

            return match.Success
                ? new[] { match.Value }
                : new[] { url.Trim('/') + '/' };
        }

        private static string GetPath(string url)
        {
            var uri = new Uri(url.Replace('*', '-').Replace('+', '-'));
            return uri.LocalPath;
        }
    }
}
