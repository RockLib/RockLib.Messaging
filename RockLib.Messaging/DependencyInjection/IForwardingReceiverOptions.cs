using System;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// Defines the editable settings for creating an instance of <see cref="ForwardingReceiver"/>.
    /// </summary>
    public interface IForwardingReceiverOptions
    {
        /// <summary>
        /// Gets or sets the name of the receiver - as obtained from an <see cref="IServiceProvider"/>
        /// - that is used as the forwarder for messages that are acknowledged.
        /// </summary>
        string? AcknowledgeForwarderName { get; set; }

        /// <summary>
        /// Gets or sets the outcome for received messages that have been forwarded to to the
        /// acknowledge forwarder.
        /// </summary>
        ForwardingOutcome AcknowledgeOutcome { get; set; }

        /// <summary>
        /// Gets or sets the name of the receiver - as obtained from an <see cref="IServiceProvider"/>
        /// - that is used as the forwarder for messages that are rolled back.
        /// </summary>
        string? RollbackForwarderName { get; set; }

        /// <summary>
        /// Gets or sets the outcome for received messages that have been forwarded to to the
        /// rollback forwarder.
        /// </summary>
        ForwardingOutcome RollbackOutcome { get; set; }

        /// <summary>
        /// Gets or sets the name of the receiver - as obtained from an <see cref="IServiceProvider"/>
        /// - that is used as the forwarder for messages that are rejected.
        /// </summary>
        string? RejectForwarderName { get; set; }

        /// <summary>
        /// Gets or sets the outcome for received messages that have been forwarded to to the
        /// reject forwarder.
        /// </summary>
        ForwardingOutcome RejectOutcome { get; set; }
    }
}