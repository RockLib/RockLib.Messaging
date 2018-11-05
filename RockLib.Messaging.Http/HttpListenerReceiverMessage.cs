using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RockLib.Messaging.Http
{
    public class HttpListenerReceiverMessage : ReceiverMessage
    {
        public HttpListenerReceiverMessage(HttpListenerContext context)
            : base(() => GetPayload(context))
        {
            Context = context;
        }

        public override byte? Priority => null;

        public HttpListenerContext Context { get; }

        public override void Acknowledge()
        {
            Context.Response.StatusCode = 200;
            Context.Response.StatusDescription = "OK";
            Context.Response.Close();
        }

        public override void Rollback()
        {
            Context.Response.StatusCode = 500;
            Context.Response.StatusDescription = "Internal Server Error";
            Context.Response.Close();
        }

        protected override void InitializeHeaders(IDictionary<string, object> headers)
        {
            foreach (var key in Context.Request.Headers.AllKeys)
            {
                headers.Add(key, Context.Request.Headers[key]);
            }
        }

        private static byte[] GetPayload(HttpListenerContext context)
        {
            var buffer = new byte[context.Request.ContentLength64];
            context.Request.InputStream.Read(buffer, 0, buffer.Length);
            return buffer;
        }
    }
}
