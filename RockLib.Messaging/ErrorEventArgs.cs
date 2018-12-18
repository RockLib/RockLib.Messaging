using System;

namespace RockLib.Messaging
{
    /// <summary>
    /// Provides data for the <see cref="IReceiver.Error"/> event.
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorEventArgs"/> class.
        /// </summary>
        /// <param name="message">A message the describes the error.</param>
        /// <param name="exception">The exception responsible for the error.</param>
        public ErrorEventArgs(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// Gets the message that describes the error.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the exception responsible for the error.
        /// </summary>
        public Exception Exception { get; }
    }
}
