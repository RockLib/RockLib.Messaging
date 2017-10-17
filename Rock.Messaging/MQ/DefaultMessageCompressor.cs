#if ROCKLIB
using RockLib.Immutable;
#else
using Rock.Immutable;
#endif

#if ROCKLIB
namespace RockLib.Messaging
#else
namespace Rock.Messaging
#endif
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