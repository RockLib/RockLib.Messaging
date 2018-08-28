namespace RockLib.Messaging.Internal
{
    /// <summary>
    /// Provides extension methods for <see cref="ISenderMessage"/>.
    /// </summary>
    public static class SenderMessageExtensions
    {
        /// <summary>
        /// Gets a value determining whether a sender message should be compressed.
        /// </summary>
        /// <param name="source">The sender message.</param>
        /// <param name="compressedInConfiguration">
        /// A value indicating whether the target <see cref="ISender"/> has been configured
        /// to compress messages.
        /// </param>
        /// <returns>
        /// True, if the <see cref="ISenderMessage.Compressed"/> property of
        /// <paramref name="source"/> is null and <paramref name="compressedInConfiguration"/>
        /// is true, or if the <see cref="ISenderMessage.Compressed"/> property of
        /// <paramref name="source"/> is null. Otherwise, return false.</returns>
        public static bool ShouldCompress(this ISenderMessage source, bool compressedInConfiguration)
        {
            return ((source.Compressed == null && compressedInConfiguration)
                    || source.Compressed == true);
        }
    }
}