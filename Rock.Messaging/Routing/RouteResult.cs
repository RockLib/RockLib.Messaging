using System;

namespace Rock.Messaging.Routing
{
    public class RouteResult
    {
        private readonly IMessage _message;
        private readonly Exception _exception;

        public RouteResult(IMessage message)
        {
            _message = message;
        }

        public RouteResult(Exception exception)
        {
            _exception = exception;
        }

        public IMessage Message
        {
            get { return _message; }
        }

        public Exception Exception
        {
            get { return _exception; }
        }
    }
}