using System;
using System.Collections.Generic;
using System.Net;

namespace RockLib.Messaging.Http
{
    public class HttpListenerReceiver : Receiver
    {
        private readonly HttpListener _listener;

        private bool disposed;

        public HttpListenerReceiver(string name, IEnumerable<string> prefixes,
            int acknowledgeStatusCode = 200, string acknowledgeStatusDescription = "OK",
            int rollbackStatusCode = 500, string rollbackStatusDescription = "Internal Server Error")
            : this(name, prefixes, new DefaultResponseGenerator(acknowledgeStatusCode, acknowledgeStatusDescription, rollbackStatusCode, rollbackStatusDescription))
        {
        }

        public HttpListenerReceiver(string name, IEnumerable<string> prefixes, IResponseGenerator responseGenerator)
            : base(name)
        {
            if (prefixes == null)
                throw new ArgumentNullException(nameof(prefixes));

            ResponseGenerator = responseGenerator ?? throw new ArgumentNullException(nameof(responseGenerator));

            _listener = new HttpListener();
            foreach (var prefix in prefixes)
                _listener.Prefixes.Add(prefix);
        }

        public IResponseGenerator ResponseGenerator { get; }

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

            MessageHandler.OnMessageReceived(this, new HttpListenerReceiverMessage(context, ResponseGenerator));
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
