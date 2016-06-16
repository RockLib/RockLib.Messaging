namespace Rock.Messaging.Internal
{
    /// <summary>
    /// Provides the names of headers for internal use.
    /// </summary>
    public static class HeaderName
    {
        /// <summary>
        /// The header name to mark a message as having its payload compressed.
        /// </summary>
        public static string CompressedPayload
        {
            get { return "core_compressed_payload"; }
        }

        /// <summary>
        /// The header name to indicate which messaging system generated the message.
        /// </summary>
        public static string OriginatingSystem
        {
            get { return "core_originating_system"; }
        }

        /// <summary>
        /// The header name to indicate which message format a message was created with.
        /// </summary>
        public static string MessageFormat
        {
            get { return "core_message_format"; }
        }
    }
}