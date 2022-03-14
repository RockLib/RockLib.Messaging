using System;
using System.Runtime.Serialization;

namespace RockLib.Messaging.Kafka.Exceptions
{
    /// <summary>
    /// Represents an error that occured when validating a message
    /// </summary>
    [Serializable]
    public class InvalidMessageException : Exception
    {
        /// <summary>
        /// Construct a new instance of <see cref="InvalidMessageException"/> with a specified message
        /// </summary>
        /// <param name="message">Message describing the error</param>
        public InvalidMessageException(string message) : base(message)
        {
        }

        /// <summary>
        /// Construct a new instance of <see cref="InvalidMessageException"/>
        /// </summary>
        public InvalidMessageException()
        {
        }

        /// <summary>
        /// Construct a new instance of <see cref="InvalidMessageException"/> with a specified message
        /// and inner exception
        /// </summary>
        /// <param name="message">Message describing the error</param>
        /// <param name="innerException">The inner exception</param>
        public InvalidMessageException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructs a new instance of <see cref="InvalidMessageException"/> via serialization
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The stream context</param>
        protected InvalidMessageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}