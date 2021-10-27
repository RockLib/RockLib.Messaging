using System;

namespace RockLib.Messaging.Kafka.Exceptions
{
    /// <summary>
    /// Represents an error that occured when validating a message
    /// </summary>
    public class InvalidMessageException : Exception
    {
        /// <summary>
        /// Construct a new instance of <see cref="InvalidMessageException"/> with a specified message
        /// </summary>
        /// <param name="message">Message describing the error</param>
        public InvalidMessageException(string message) : base(message)
        {
        }
    }
}