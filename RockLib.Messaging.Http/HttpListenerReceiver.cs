using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace RockLib.Messaging.Http
{
    public class HttpListenerReceiver : Receiver
    {
        private readonly Regex _pathRegex;
        private readonly IReadOnlyCollection<string> _pathTokens;
        private readonly HttpListener _listener;

        private bool disposed;

        public HttpListenerReceiver(string name, IEnumerable<string> prefixes, string path, string method = "POST",
            int acknowledgeStatusCode = 200, string acknowledgeStatusDescription = "OK",
            int rollbackStatusCode = 500, string rollbackStatusDescription = "Internal Server Error",
            int rejectStatusCode = 400, string rejectStatusDescription = "Bad Request")
            : this(name, prefixes, path,
                  new DefaultHttpResponseGenerator(acknowledgeStatusCode, acknowledgeStatusDescription, rollbackStatusCode, rollbackStatusDescription, rejectStatusCode, rejectStatusDescription),
                  method)
        {
        }

        public HttpListenerReceiver(string name, IEnumerable<string> prefixes, string path, IHttpResponseGenerator httpResponseGenerator, string method = "POST")
            : base(name)
        {
            if (prefixes == null)
                throw new ArgumentNullException(nameof(prefixes));

            HttpResponseGenerator = httpResponseGenerator ?? throw new ArgumentNullException(nameof(httpResponseGenerator));
            Method = method ?? throw new ArgumentNullException(nameof(method));

            Path = path?.ToLowerInvariant().Trim('/') ?? throw new ArgumentNullException(nameof(path));
            var pathTokens = new List<string>();
            var pathPattern = "^" + Regex.Replace(Path ?? "", "{([^}]+)}", m =>
            {
                var token = m.Groups[1].Value;
                pathTokens.Add(token);
                return $"(?<{token}>.*?)";
            }) + "$";
            _pathRegex = new Regex(pathPattern);
            _pathTokens = pathTokens;

            _listener = new HttpListener();
            foreach (var prefix in prefixes)
                _listener.Prefixes.Add(prefix);
        }

        public IHttpResponseGenerator HttpResponseGenerator { get; }
        public string Method { get; }
        public string Path { get; }

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

            if (!_pathRegex.IsMatch(context.Request.Url.AbsolutePath.ToLowerInvariant().Trim('/')))
            {
                context.Response.StatusCode = 404;
                context.Response.StatusDescription = "Not Found";
                context.Response.Close();
                return;
            }

            if (context.Request.HttpMethod != Method)
            {
                context.Response.StatusCode = 405;
                context.Response.StatusDescription = "Method Not Allowed";
                context.Response.Close();
                return;
            }

            MessageHandler.OnMessageReceived(this, new HttpListenerReceiverMessage(context, HttpResponseGenerator, _pathRegex, _pathTokens));
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            disposed = true;
            _listener.Stop();
            base.Dispose(disposing);
            _listener.Close();
        }
    }
}
