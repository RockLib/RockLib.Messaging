namespace RockLib.Messaging
{
    /// <summary>
    /// Defines the outcomes for a forwarded message.
    /// </summary>
    public enum ForwardingOutcome
    {
        /// <summary>
        /// A forwarded message should have its <see cref="IReceiverMessage.AcknowledgeAsync"/> method called.
        /// </summary>
        Acknowledge,

        /// <summary>
        /// A forwarded message should have its <see cref="IReceiverMessage.RollbackAsync"/> method called.
        /// </summary>
        Rollback,

        /// <summary>
        /// A forwarded message should have its <see cref="IReceiverMessage.RejectAsync"/> method called.
        /// </summary>
        Reject
    }
}
