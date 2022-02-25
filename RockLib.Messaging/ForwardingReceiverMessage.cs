using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging
{
    /// <summary>
    /// A decorator for the <see cref="IReceiverMessage"/> interface that forwards
    /// received messages to configured <see cref="ISender"/> instances when
    /// messages are acknowledged, rolled back, or rejected.
    /// </summary>
    public class ForwardingReceiverMessage : IReceiverMessage
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private string? _handledBy;

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
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] BinaryPayload => Message.BinaryPayload;
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Gets the <see cref="IReceiverMessage.Headers"/> of the actual
        /// <see cref="IReceiverMessage"/> that was received.
        /// </summary>
        public HeaderDictionary Headers => Message.Headers;

        /// <summary>
        /// Gets a value indicating whether this message has been handled by one of the
        /// <see cref="AcknowledgeAsync"/>, <see cref="RollbackAsync"/> or <see cref="RejectAsync"/>
        /// methods.
        /// </summary>
        public bool Handled => _handledBy is not null;

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
        public async Task AcknowledgeAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                ThrowIfHandled();
                if (ForwardingReceiver.AcknowledgeForwarder is not null)
                {
                    await ForwardingReceiver.AcknowledgeForwarder.SendAsync(Message.ToSenderMessage(), cancellationToken).ConfigureAwait(false);
                    await HandleForwardedMessageAsync(ForwardingReceiver.AcknowledgeOutcome, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await Message.AcknowledgeAsync(cancellationToken).ConfigureAwait(false);
                }
                SetHandled();
            }
            finally
            {
                _semaphore.Release();
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
        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                ThrowIfHandled();
                if (ForwardingReceiver.RollbackForwarder is not null)
                {
                    await ForwardingReceiver.RollbackForwarder.SendAsync(Message.ToSenderMessage(), cancellationToken).ConfigureAwait(false);
                    await HandleForwardedMessageAsync(ForwardingReceiver.RollbackOutcome, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await Message.RollbackAsync(cancellationToken).ConfigureAwait(false);
                }
                SetHandled();
            }
            finally
            {
                _semaphore.Release();
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
        public async Task RejectAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                ThrowIfHandled();
                if (ForwardingReceiver.RejectForwarder is not null)
                {
                    await ForwardingReceiver.RejectForwarder.SendAsync(Message.ToSenderMessage(), cancellationToken).ConfigureAwait(false);
                    await HandleForwardedMessageAsync(ForwardingReceiver.RejectOutcome, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await Message.RejectAsync(cancellationToken).ConfigureAwait(false);
                }
                SetHandled();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private Task HandleForwardedMessageAsync(ForwardingOutcome outcome, CancellationToken cancellationToken)
        {
            return outcome switch
            {
                ForwardingOutcome.Acknowledge => Message.AcknowledgeAsync(cancellationToken),
                ForwardingOutcome.Rollback => Message.RollbackAsync(cancellationToken),
                ForwardingOutcome.Reject => Message.RejectAsync(cancellationToken),
                _ => throw new InvalidOperationException("Invalid ForwardingOutcome value."),
            };
        }

        private void ThrowIfHandled([CallerMemberName] string? callerMemberName = null)
        {
            if (Handled)
            {
                throw new InvalidOperationException($"Cannot {callerMemberName} message: the message has already been handled by {_handledBy}.");
            }
        }

        private void SetHandled([CallerMemberName] string? callerMemberName = null)
        {
            _handledBy = callerMemberName;
        }
    }
}
