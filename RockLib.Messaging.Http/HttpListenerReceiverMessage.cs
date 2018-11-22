using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RockLib.Messaging.Http
{
    public class HttpListenerReceiverMessage : ReceiverMessage
    {
        public HttpListenerReceiverMessage(HttpListenerContext context, IHttpResponseGenerator httpResponseGenerator)
            : base(() => GetPayload(context))
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            HttpResponseGenerator = httpResponseGenerator ?? throw new ArgumentNullException(nameof(httpResponseGenerator));
        }

        public override byte? Priority => null;

        public HttpListenerContext Context { get; }

        public IHttpResponseGenerator HttpResponseGenerator { get; }

        public override void Acknowledge()
        {
            var response = HttpResponseGenerator.GetAcknowledgeResponse(this);
            WriteResponse(response);
        }

        public override void Rollback()
        {
            var response = HttpResponseGenerator.GetRollbackResponse(this);
            WriteResponse(response);
        }

        private void WriteResponse(HttpResponse response)
        {
            Context.Response.StatusCode = response.StatusCode;
            Context.Response.StatusDescription = response.StatusDescription;

            switch (response.Content)
            {
                case string stringContent:
                    Context.Response.ContentEncoding = Encoding.UTF8;
                    var buffer = Encoding.UTF8.GetBytes(stringContent);
                    Context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    break;
                case byte[] binaryContent:
                    Context.Response.OutputStream.Write(binaryContent, 0, binaryContent.Length);
                    break;
            }

            foreach (var header in response.Headers)
                Context.Response.Headers.Add(header.Key, header.Value);

            Context.Response.Close();
        }

        protected override void InitializeHeaders(IDictionary<string, object> headers)
        {
            foreach (var key in Context.Request.Headers.AllKeys)
                headers.Add(key, Context.Request.Headers[key]);

            foreach (var key in Context.Request.QueryString.AllKeys)
                headers.Add(key, Context.Request.QueryString[key]);
        }

        private static byte[] GetPayload(HttpListenerContext context)
        {
            var buffer = new byte[context.Request.ContentLength64];
            context.Request.InputStream.Read(buffer, 0, buffer.Length);
            return buffer;
        }
    }
}
