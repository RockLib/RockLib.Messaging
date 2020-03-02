#if !NET451
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// Defines the settings for creating an instance of <see cref="ForwardingReceiver"/>.
    /// </summary>
    public class ForwardingReceiverOptions : IForwardingReceiverOptions
    {
        /// <summary>
        /// Gets or sets the name of the receiver - as obtained from the <see cref="IServiceProvider"/>
        /// parameter of the <see cref="GetAcknowledgeForwarder"/> method - that is used as the
        /// forwarder for messages that are acknowledged.
        /// </summary>
        public string AcknowledgeForwarderName { get; set; }

        /// <summary>
        /// Gets or sets the outcome for received messages that have been forwarded to to the
        /// acknowledge forwarder.
        /// </summary>
        public ForwardingOutcome AcknowledgeOutcome { get; set; } = ForwardingOutcome.Acknowledge;

        /// <summary>
        /// Gets or sets the name of the receiver - as obtained from the <see cref="IServiceProvider"/>
        /// parameter of the <see cref="GetRollbackForwarder"/> method - that is used as the
        /// forwarder for messages that are rolled back.
        /// </summary>
        public string RollbackForwarderName { get; set; }

        /// <summary>
        /// Gets or sets the outcome for received messages that have been forwarded to to the
        /// rollback forwarder.
        /// </summary>
        public ForwardingOutcome RollbackOutcome { get; set; } = ForwardingOutcome.Rollback;

        /// <summary>
        /// Gets or sets the name of the receiver - as obtained from the <see cref="IServiceProvider"/>
        /// parameter of the <see cref="GetRejectForwarder"/> method - that is used as the
        /// forwarder for messages that are rejected.
        /// </summary>
        public string RejectForwarderName { get; set; }

        /// <summary>
        /// Gets or sets the outcome for received messages that have been forwarded to to the
        /// reject forwarder.
        /// </summary>
        public ForwardingOutcome RejectOutcome { get; set; } = ForwardingOutcome.Reject;

        /// <summary>
        /// Gets the <see cref="ISender"/> to be used as the <see cref="ForwardingReceiver.AcknowledgeForwarder"/>.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider that has a registered <see cref="ISender"/> with a name matching
        /// <see cref="AcknowledgeForwarderName"/>.
        /// </param>
        /// <returns>The acknowledge forwarder.</returns>
        public ISender GetAcknowledgeForwarder(IServiceProvider serviceProvider) => GetForwarder(serviceProvider, AcknowledgeForwarderName);

        /// <summary>
        /// Gets the <see cref="ISender"/> to be used as the <see cref="ForwardingReceiver.RollbackForwarder"/>.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider that has a registered <see cref="ISender"/> with a name matching
        /// <see cref="RollbackForwarderName"/>.
        /// </param>
        /// <returns>The rollback forwarder.</returns>
        public ISender GetRollbackForwarder(IServiceProvider serviceProvider) => GetForwarder(serviceProvider, RollbackForwarderName);

        /// <summary>
        /// Gets the <see cref="ISender"/> to be used as the <see cref="ForwardingReceiver.RejectForwarder"/>.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider that has a registered <see cref="ISender"/> with a name matching
        /// <see cref="RejectForwarderName"/>.
        /// </param>
        /// <returns>The reject forwarder.</returns>
        public ISender GetRejectForwarder(IServiceProvider serviceProvider) => GetForwarder(serviceProvider, RejectForwarderName);

        private static ISender GetForwarder(IServiceProvider serviceProvider, string name) =>
            name == null
                ? null
                : serviceProvider.GetService<IEnumerable<ISender>>()?.FirstOrDefault(s => s.Name == name)
                    ?? serviceProvider.GetService<IEnumerable<ITransactionalSender>>()?.FirstOrDefault(s => s.Name == name)
                    ?? throw new InvalidOperationException($"No senders found matching name '{name}'.");
    }
}
#endif
