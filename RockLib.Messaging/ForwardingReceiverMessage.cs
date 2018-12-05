using System;
using System.Runtime.CompilerServices;

namespace RockLib.Messaging
{
    public class ForwardingReceiverMessage : IReceiverMessage
    {
        private string _handledBy;

        internal ForwardingReceiverMessage(ForwardingReceiver forwardingReceiver, IReceiverMessage message)
        {
            ForwardingReceiver = forwardingReceiver;
            Message = message;
        }

        public ForwardingReceiver ForwardingReceiver { get; }

        public IReceiverMessage Message { get; }

        public string StringPayload => Message.StringPayload;

        public byte[] BinaryPayload => Message.BinaryPayload;

        public HeaderDictionary Headers => Message.Headers;

        public bool Handled => _handledBy != null;

        public void Acknowledge()
        {
            lock (this)
            {
                ThrowIfHandled();
                if (ForwardingReceiver.AcknowledgeForwarder != null)
                {
                    ForwardingReceiver.AcknowledgeForwarder.Send(Message.ToSenderMessage());
                    HandleForwardedMessage(ForwardingReceiver.AcknowledgeOutcome);
                }
                else
                {
                    Message.Acknowledge();
                }
                SetHandled();
            }
        }

        public void Rollback()
        {
            lock (this)
            {
                ThrowIfHandled();
                if (ForwardingReceiver.RollbackForwarder != null)
                {
                    ForwardingReceiver.RollbackForwarder.Send(Message.ToSenderMessage());
                    HandleForwardedMessage(ForwardingReceiver.RollbackOutcome);
                }
                else
                {
                    Message.Rollback();
                }
                SetHandled();
            }
        }

        public void Reject()
        {
            lock (this)
            {
                ThrowIfHandled();
                if (ForwardingReceiver.RejectForwarder != null)
                {
                    ForwardingReceiver.RejectForwarder.Send(Message.ToSenderMessage());
                    HandleForwardedMessage(ForwardingReceiver.RejectOutcome);
                }
                else
                {
                    Message.Reject();
                }
                SetHandled();
            }
        }

        private void HandleForwardedMessage(ForwardingOutcome outcome)
        {
            switch (outcome)
            {
                case ForwardingOutcome.Acknowledge:
                    Message.Acknowledge();
                    break;
                case ForwardingOutcome.Rollback:
                    Message.Rollback();
                    break;
                case ForwardingOutcome.Reject:
                    Message.Reject();
                    break;
            }
        }

        private void ThrowIfHandled([CallerMemberName] string callerMemberName = null)
        {
            if (Handled)
                throw new InvalidOperationException($"Cannot {callerMemberName} message: the message has already been handled by {_handledBy}.");
        }

        private void SetHandled([CallerMemberName] string callerMemberName = null)
        {
            _handledBy = callerMemberName;
        }
    }
}
