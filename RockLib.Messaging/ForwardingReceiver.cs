using System;

namespace RockLib.Messaging
{
    public sealed class ForwardingReceiver : IReceiver
    {
        public ForwardingReceiver(string name, string receiverName,
            string acknowledgeForwarderName = null, ForwardingOutcome acknowledgeOutcome = ForwardingOutcome.Acknowledge,
            string rollbackForwarderName = null, ForwardingOutcome rollbackOutcome = ForwardingOutcome.Rollback,
            string rejectForwarderName = null, ForwardingOutcome rejectOutcome = ForwardingOutcome.Reject)
            : this(name ?? throw new ArgumentNullException(nameof(name)),
                  MessagingScenarioFactory.CreateReceiver(receiverName ?? throw new ArgumentNullException(nameof(receiverName))),
                  CreateForwarder(acknowledgeForwarderName), acknowledgeOutcome,
                  CreateForwarder(rollbackForwarderName), rollbackOutcome,
                  CreateForwarder(rejectForwarderName), rejectOutcome)
        {
        }

        public ForwardingReceiver(string name, IReceiver receiver,
            ISender acknowledgeForwarder = null, ForwardingOutcome acknowledgeOutcome = ForwardingOutcome.Acknowledge,
            ISender rollbackForwarder = null, ForwardingOutcome rollbackOutcome = ForwardingOutcome.Rollback,
            ISender rejectForwarder = null, ForwardingOutcome rejectOutcome = ForwardingOutcome.Reject)
        {
            if (receiver == null)
                throw new ArgumentNullException(nameof(receiver));
            if (receiver.MessageHandler != null)
                throw new ArgumentException("Cannot create a ForwardingReceiver with a receiver that is already started.", nameof(receiver));

            Name = name ?? throw new ArgumentNullException(nameof(name));
            Receiver = receiver;
            AcknowledgeForwarder = acknowledgeForwarder;
            AcknowledgeOutcome = acknowledgeOutcome;
            RollbackForwarder = rollbackForwarder;
            RollbackOutcome = rollbackOutcome;
            RejectForwarder = rejectForwarder;
            RejectOutcome = rejectOutcome;
        }

        public string Name { get; }

        public IReceiver Receiver { get; }

        public ISender AcknowledgeForwarder { get; }

        public ForwardingOutcome AcknowledgeOutcome { get; }

        public ISender RollbackForwarder { get; }

        public ForwardingOutcome RollbackOutcome { get; }

        public ISender RejectForwarder { get; }

        public ForwardingOutcome RejectOutcome { get; }

        public IMessageHandler MessageHandler
        {
            get => ((ForwardingMessageHandler)Receiver.MessageHandler).MessageHandler;
            set => Receiver.MessageHandler = new ForwardingMessageHandler(this, value);
        }

        public event EventHandler Connected
        {
            add { Receiver.Connected += value; }
            remove { Receiver.Connected -= value; }
        }

        public event EventHandler<DisconnectedEventArgs> Disconnected
        {
            add { Receiver.Disconnected += value; }
            remove { Receiver.Disconnected -= value; }
        }

        public void Dispose()
        {
            Receiver.Dispose();
            AcknowledgeForwarder?.Dispose();
            RollbackForwarder?.Dispose();
            RejectForwarder?.Dispose();
        }

        private static ISender CreateForwarder(string senderName) =>
            senderName == null ? null : MessagingScenarioFactory.CreateSender(senderName);
    }
}
