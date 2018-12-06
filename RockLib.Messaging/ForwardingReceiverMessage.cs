using System;
using System.Runtime.CompilerServices;

namespace RockLib.Messaging
{
    /// <summary>
    /// A decorator for the <see cref="IReceiverMessage"/> interface that forwards
    /// received messages to configured <see cref="ISender"/> instances when
    /// messages are acknowledged, rolled back, or rejected.
    /// </summary>
    public class ForwardingReceiverMessage : IReceiverMessage
    {
        private string _handledBy;

        internal ForwardingReceiverMessage(ForwardingReceiver forwardingReceiver, IReceiverMessage message)
        {
            ForwardingReceiver = forwardingReceiver;
            Message = message;
        }

        /// <summary>
        /// Gets the <see cref="ForwardingReceiver"/> whose properties determine
        /// what happens to messages when they are acknowledged, rolled back, or
        /// rejected. The <see cref="IReceiver"/> that this decorates is the actual
        /// source of messages.
        /// </summary>
        public ForwardingReceiver ForwardingReceiver { get; }

        /// <summary>
        /// Gets the actual <see cref="IReceiverMessage"/> that was received.
        /// </summary>
        public IReceiverMessage Message { get; }

        /// <summary>
        /// Gets the <see cref="IReceiverMessage.StringPayload"/> of the actual
        /// <see cref="IReceiverMessage"/> that was received.
        /// </summary>
        public string StringPayload => Message.StringPayload;

        /// <summary>
        /// Gets the <see cref="IReceiverMessage.BinaryPayload"/> of the actual
        /// <see cref="IReceiverMessage"/> that was received.
        /// </summary>
        public byte[] BinaryPayload => Message.BinaryPayload;

        /// <summary>
        /// Gets the <see cref="IReceiverMessage.Headers"/> of the actual
        /// <see cref="IReceiverMessage"/> that was received.
        /// </summary>
        public HeaderDictionary Headers => Message.Headers;

        /// <summary>
        /// Gets a value indicating whether this message has been handled by one of the
        /// <see cref="Acknowledge"/>, <see cref="Rollback"/> or <see cref="Reject"/>
        /// methods.
        /// </summary>
        public bool Handled => _handledBy != null;

        /// <summary>
        /// Indicates that the message was successfully processed and should not
        /// be redelivered.
        /// <para>
        /// If the <see cref="ForwardingReceiver"/> property has a non-null
        /// <see cref="ForwardingReceiver.AcknowledgeForwarder"/> value, then <see cref="Message"/>
        /// is forwarded to it and handled according to the <see cref="ForwardingReceiver.AcknowledgeOutcome"/>
        /// value. Otherwise, <see cref="Message"/> is acknowledged.
        /// </para>
        /// </summary>
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

        /// <summary>
        /// Indicates that the message was not successfully processed but should be
        /// (or should be allowed to be) redelivered.
        /// <para>
        /// If the <see cref="ForwardingReceiver"/> property has a non-null
        /// <see cref="ForwardingReceiver.RollbackForwarder"/> value, then <see cref="Message"/>
        /// is forwarded to it and handled according to the <see cref="ForwardingReceiver.RollbackOutcome"/>
        /// value. Otherwise, <see cref="Message"/> is rolled back.
        /// </para>
        /// </summary>
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

        /// <summary>
        /// Indicates that the message could not be successfully processed and should
        /// not be redelivered.
        /// <para>
        /// If the <see cref="ForwardingReceiver"/> property has a non-null
        /// <see cref="ForwardingReceiver.RejectForwarder"/> value, then <see cref="Message"/>
        /// is forwarded to it and handled according to the <see cref="ForwardingReceiver.RejectOutcome"/>
        /// value. Otherwise, <see cref="Message"/> is rejected.
        /// </para>
        /// </summary>
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
