using System;

namespace RockLib.Messaging
{
    /// <summary>
    /// A decorator for the <see cref="IReceiver"/> interface that can forward messages using an
    /// <see cref="ISender"/> instance when acknowledged, rolled back, or rejected.
    /// </summary>
    public sealed class ForwardingReceiver : IReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardingReceiver"/> class. This constructor
        /// uses <see cref="MessagingScenarioFactory"/> to create the main <see cref="IReceiver"/> and
        /// <see cref="ISender"/> forwarders.
        /// </summary>
        /// <param name="name">The name of the forwarding receiver.</param>
        /// <param name="receiverName">
        /// The name of the <see cref="IReceiver"/> that is the actual source of messages.
        /// </param>
        /// <param name="acknowledgeForwarderName">
        /// The name of the <see cref="ISender"/> that received messages are forwarded to when
        /// their <see cref="IReceiverMessage.AcknowledgeAsync"/> method is called.
        /// </param>
        /// <param name="acknowledgeOutcome">
        /// The outcome for received messages that have been forwarded to
        /// <see cref="AcknowledgeForwarder"/>.
        /// </param>
        /// <param name="rollbackForwarderName">
        /// The name of the <see cref="ISender"/> that received messages are forwarded to when
        /// their <see cref="IReceiverMessage.RollbackAsync"/> method is called.
        /// </param>
        /// <param name="rollbackOutcome">
        /// The outcome for received messages that have been forwarded to
        /// <see cref="RollbackForwarder"/>.
        /// </param>
        /// <param name="rejectForwarderName">
        /// The name of the <see cref="ISender"/> that received messages are forwarded to when
        /// their <see cref="IReceiverMessage.RejectAsync"/> method is called.
        /// </param>
        /// <param name="rejectOutcome">
        /// The outcome for received messages that have been forwarded to
        /// <see cref="RejectForwarder"/>.
        /// </param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardingReceiver"/> class.
        /// </summary>
        /// <param name="name">The name of the forwarding receiver.</param>
        /// <param name="receiver">
        /// The <see cref="IReceiver"/> that is the actual source of messages.
        /// </param>
        /// <param name="acknowledgeForwarder">
        /// The <see cref="ISender"/> that received messages are forwarded to when
        /// their <see cref="IReceiverMessage.AcknowledgeAsync"/> method is called.
        /// </param>
        /// <param name="acknowledgeOutcome">
        /// The outcome for received messages that have been forwarded to
        /// <see cref="AcknowledgeForwarder"/>.
        /// </param>
        /// <param name="rollbackForwarder">
        /// The <see cref="ISender"/> that received messages are forwarded to when
        /// their <see cref="IReceiverMessage.RollbackAsync"/> method is called.
        /// </param>
        /// <param name="rollbackOutcome">
        /// The outcome for received messages that have been forwarded to
        /// <see cref="RollbackForwarder"/>.
        /// </param>
        /// <param name="rejectForwarder">
        /// The <see cref="ISender"/> that received messages are forwarded to when
        /// their <see cref="IReceiverMessage.RejectAsync"/> method is called.
        /// </param>
        /// <param name="rejectOutcome">
        /// The outcome for received messages that have been forwarded to
        /// <see cref="RejectForwarder"/>.
        /// </param>
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

        /// <summary>
        /// Gets the name of the forwarding receiver.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the <see cref="IReceiver"/> that is the actual source of messages.
        /// </summary>
        public IReceiver Receiver { get; }

        /// <summary>
        /// Gets the <see cref="ISender"/> that received messages are forwarded to when
        /// their <see cref="IReceiverMessage.AcknowledgeAsync"/> method is called.
        /// </summary>
        public ISender AcknowledgeForwarder { get; }

        /// <summary>
        /// Gets the outcome for received messages that have been forwarded to
        /// <see cref="AcknowledgeForwarder"/>.
        /// </summary>
        public ForwardingOutcome AcknowledgeOutcome { get; }

        /// <summary>
        /// Gets the <see cref="ISender"/> that received messages are forwarded to when
        /// their <see cref="IReceiverMessage.RollbackAsync"/> method is called.
        /// </summary>
        public ISender RollbackForwarder { get; }

        /// <summary>
        /// Gets the outcome for received messages that have been forwarded to
        /// <see cref="RollbackForwarder"/>.
        /// </summary>
        public ForwardingOutcome RollbackOutcome { get; }

        /// <summary>
        /// Gets the <see cref="ISender"/> that received messages are forwarded to when
        /// their <see cref="IReceiverMessage.RejectAsync"/> method is called.
        /// </summary>
        public ISender RejectForwarder { get; }

        /// <summary>
        /// Gets the outcome for received messages that have been forwarded to
        /// <see cref="RejectForwarder"/>.
        /// </summary>
        public ForwardingOutcome RejectOutcome { get; }

        /// <summary>
        /// Gets or sets the message handler for this receiver. When set, the receiver is started
        /// and will invoke the value's <see cref="IMessageHandler.OnMessageReceived"/> method
        /// when messages are received.
        /// <para>
        /// When set, this property sets the <see cref="IReceiver.MessageHandler"/> of the
        /// <see cref="Receiver"/> property to a new instace of the <see cref="ForwardingMessageHandler"/>
        /// class. The getter returns the <see cref="ForwardingMessageHandler.MessageHandler"/> property
        /// of that object.
        /// </para>
        /// </summary>
        public IMessageHandler MessageHandler
        {
            get => ((ForwardingMessageHandler)Receiver.MessageHandler)?.MessageHandler;
            set => Receiver.MessageHandler = new ForwardingMessageHandler(this, value);
        }

        /// <summary>
        /// Occurs when a connection is established.
        /// <para>
        /// This event passes through to the <see cref="IReceiver.Connected"/> event of
        /// the <see cref="Receiver"/> property.
        /// </para>
        /// </summary>
        public event EventHandler Connected
        {
            add => Receiver.Connected += value;
            remove => Receiver.Connected -= value;
        }

        /// <summary>
        /// Occurs when a connection is lost.
        /// <para>
        /// This event passes through to the <see cref="IReceiver.Disconnected"/> event of
        /// the <see cref="Receiver"/> property.
        /// </para>
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> Disconnected
        {
            add => Receiver.Disconnected += value;
            remove => Receiver.Disconnected -= value;
        }

        /// <summary>
        /// Occurs when an error happens.
        /// <para>
        /// This event passes through to the <see cref="IReceiver.Error"/> event of
        /// the <see cref="Receiver"/> property.
        /// </para>
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error
        {
            add => Receiver.Error += value;
            remove => Receiver.Error -= value;
        }

        /// <summary>
        /// Disposes the <see cref="Receiver"/> property and, if they are not null,
        /// the <see cref="AcknowledgeForwarder"/>, <see cref="RollbackForwarder"/>,
        /// and <see cref="RejectForwarder"/> properties.
        /// </summary>
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
