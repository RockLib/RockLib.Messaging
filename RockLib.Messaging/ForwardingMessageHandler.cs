namespace RockLib.Messaging
{
    public class ForwardingMessageHandler : IMessageHandler
    {
        internal ForwardingMessageHandler(ForwardingReceiver forwardingReceiver, IMessageHandler messageHandler)
        {
            ForwardingReceiver = forwardingReceiver;
            MessageHandler = messageHandler;
        }

        public ForwardingReceiver ForwardingReceiver { get; }

        public IMessageHandler MessageHandler { get; }

        public void OnMessageReceived(IReceiver receiver, IReceiverMessage message) =>
            MessageHandler.OnMessageReceived(ForwardingReceiver, new ForwardingReceiverMessage(ForwardingReceiver, message));
    }
}
