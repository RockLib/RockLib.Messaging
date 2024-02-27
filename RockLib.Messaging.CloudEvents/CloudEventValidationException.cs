using System;

namespace RockLib.Messaging.CloudEvents
{
    /// <summary>
    /// The exception that is thrown when validation for a CloudEvent fails.
    /// </summary>
    [Serializable]
    public class CloudEventValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CloudEventValidationException"/> class.
        /// </summary>
        public CloudEventValidationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudEventValidationException"/> class with a
        /// specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CloudEventValidationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudEventValidationException"/> class with a specified
        /// error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null reference if no inner
        /// exception is specified.</param>
        public CloudEventValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
