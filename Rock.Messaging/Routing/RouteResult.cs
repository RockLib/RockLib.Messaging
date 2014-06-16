using System;

namespace Rock.Messaging.Routing
{
    public class RouteResult
    {
        private readonly IMessage _message;
        private readonly Exception _exception;

        public RouteResult(IMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            _message = message;
        }

        public RouteResult(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

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

        public bool Success
        {
            get { return _exception != null; }
        }
    }
}