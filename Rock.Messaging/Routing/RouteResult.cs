using System;

namespace Rock.Messaging.Routing
{
    /// <summary>
    /// Represents the results of a <see cref="IMessageRouter.Route"/> operation. An instance of this class
    /// indicates whether the operation was successful through its <see cref="Success"/> property. Successful
    /// operations will have a value for <see cref="Message"/>, and may have a value for <see cref="Result"/>,
    /// but will not have a value for <see cref="Exception"/>. Unsuccessful operations will have a value for
    /// <see cref="Exception"/>, but not for <see cref="Message"/> or <see cref="Result"/>.
    /// </summary>
    public class RouteResult
    {
        private readonly IMessage _message;
        private readonly object _result;
        private readonly Exception _exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteResult"/> class with a message and result object.
        /// </summary>
        /// <param name="message">The message that was handled by a Route operation.</param>
        /// <param name="result">An object that was returned by the message handler that handled the message.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="message"/> is null.</exception>
        public RouteResult(IMessage message, object result)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            _message = message;
            _result = result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteResult"/> class with an exception
        /// that was thrown during a Route operation.
        /// </summary>
        /// <param name="exception">The exception that was thrown during a Route operation.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="exception"/> is null.</exception>
        public RouteResult(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            _exception = exception;
        }

        /// <summary>
        /// Gets the message that was handled by a Route operation. If the Route operation was
        /// unsuccessful, the value of this property will be null.
        /// </summary>
        public IMessage Message
        {
            get { return _message; }
        }

        /// <summary>
        /// Gets an object that was returned by the message handler that handled the message.
        /// The value of this property may be null.
        /// </summary>
        public object Result
        {
            get { return _result; }
        }

        /// <summary>
        /// Gets the <see cref="Exception"/> that was thrown during a Route operation. If the
        /// Route operation was successful, the value of this property will be null.
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
        }

        /// <summary>
        /// Gets a value representing whether the Route operation was successful.
        /// </summary>
        public bool Success
        {
            get { return _exception == null; }
        }
    }
}