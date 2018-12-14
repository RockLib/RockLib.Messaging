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
        public const string MessageId = "core_internal_id";

        /// <summary>
        /// The name of the header used to mark a message as having its payload compressed.
        /// </summary>
        public const string IsCompressedPayload = "core_compressed_payload";

        /// <summary>
        /// The name of the header used to indicate which messaging system generated the message.
        /// </summary>
        public const string OriginatingSystem = "core_originating_system";

        /// <summary>
        /// The name of the header used to indicate that a message was originally constructed with
        /// a binary message.
        /// </summary>
        public const string IsBinaryPayload = "core_binary_payload";
    }
}