using Rock.Immutable;

namespace Rock.Messaging
{
    public static class DefaultMessageCompressor
    {
        private static readonly Semimutable<IMessageCompressor> _messageParser = new Semimutable<IMessageCompressor>(GetDefault);

        public static IMessageCompressor Current
        {
            get { return _messageParser.Value; }
        }

        public static void SetCurrent(IMessageCompressor messageParser)
        {
            _messageParser.Value = messageParser;
        }

        private static IMessageCompressor GetDefault()
        {
            return new GZipBase64EncodedMessageCompressor();
        }
    }
}