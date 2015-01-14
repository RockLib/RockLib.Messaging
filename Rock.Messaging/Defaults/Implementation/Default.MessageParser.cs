//using System;
//using Rock.Defaults;
//using Rock.Messaging.Routing;

//namespace Rock.Messaging.Defaults.Implementation
//{
//    public static partial class Default
//    {
//        private static readonly DefaultHelper<IMessageParser> _messageParser = new DefaultHelper<IMessageParser>(() => new XmlMessageParser());

//        public static IMessageParser MessageParser
//        {
//            get { return _messageParser.Current; }
//        }

//        public static IMessageParser DefaultMessageParser
//        {
//            get { return _messageParser.DefaultInstance; }
//        }

//        public static void SetMessageParser(Func<IMessageParser> getMessageParserInstance)
//        {
//            _messageParser.SetCurrent(getMessageParserInstance);
//        }

//        public static void RestoreDefaultMessageParser()
//        {
//            _messageParser.RestoreDefault();
//        }
//    }
//}
