using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RockLib.Messaging.Http
{
    public class HttpListenerReceiver : Receiver
    {
        private readonly HttpListener _listener;

        public HttpListenerReceiver(string name, IEnumerable<string> prefixes)
            : base(name)
        {
            if (prefixes == null)
                throw new ArgumentNullException(nameof(prefixes));

            _listener = new HttpListener();
            foreach (var prefix in prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }
        }

        protected override void Start()
        {
            _listener.Start();
            _listener.BeginGetContext(CompleteGetContext, null);
        }

        private void CompleteGetContext(IAsyncResult result)
        {
            // TODO: add exception handling (when disposed this line throws)
            var context = _listener.EndGetContext(result);

            _listener.BeginGetContext(CompleteGetContext, null);

            MessageHandler.OnMessageReceived(this, new HttpListenerReceiverMessage(context));
        }

        protected override void Dispose(bool disposing)
        {
            _listener.Stop();
            base.Dispose(disposing);
            _listener.Close();
        }
    }
}
