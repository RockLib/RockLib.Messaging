namespace RockLib.Messaging
{
    /// <summary>
    /// Provides the names of standard headers.
    /// </summary>
    public static class HeaderNames
    {
        /// <summary>
        /// The name of the header used to identify a message.
        /// </summary>
        public static string MessageId
        {
            get { return "core_internal_id"; }
        }

        /// <summary>
        /// The name of the header used to mark a message as having its payload compressed.
        /// </summary>
        public static string CompressedPayload
        {
            get { return "core_compressed_payload"; }
        }

        /// <summary>
        /// The name of the header used to indicate which messaging system generated the message.
        /// </summary>
        public static string OriginatingSystem
        {
            get { return "core_originating_system"; }
        }

        /// <summary>
        /// The name of the header used to indicate that a message was originally constructed with
        /// a binary message.
        /// </summary>
        public static string IsBinaryMessage
        {
            get { return "core_is_binary_message"; }
        }
    }
}