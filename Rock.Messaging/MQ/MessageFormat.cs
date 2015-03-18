namespace Rock.Messaging
{
    /// <summary>
    /// Defines various message formats.
    /// </summary>
    public enum MessageFormat
    {
        /// <summary>
        /// The message's string value is unformatted text.
        /// </summary>
        Text,

        /// <summary>
        /// The message's string value is an XML document.
        /// </summary>
        Xml,

        /// <summary>
        /// The message's string value is a JSON object.
        /// </summary>
        Json,

        /// <summary>
        /// The message's binary value is an unformatted byte array.
        /// </summary>
        Binary
    }
}