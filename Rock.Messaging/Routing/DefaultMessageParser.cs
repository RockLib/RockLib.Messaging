using Rock.Immutable;

namespace Rock.Messaging.Routing
{
    public static class DefaultMessageParser
    {
        private static readonly Semimutable<IMessageParser> _messageParser = new Semimutable<IMessageParser>(GetDefault);

        public static IMessageParser Current
        {
            get { return _messageParser.Value; }
        }

        public static void SetCurrent(IMessageParser messageParser)
        {
            _messageParser.Value = messageParser;
        }

        private static IMessageParser GetDefault()
        {
            return new XmlMessageParser();
        }
    }
}