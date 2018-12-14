namespace RockLib.Messaging
{
    /// <summary>
    /// Defines the outcomes for a forwarded message.
    /// </summary>
    public enum ForwardingOutcome
    {
        /// <summary>
        /// A forwarded message should have its <see cref="IReceiverMessage.Acknowledge"/> method called.
        /// </summary>
        Acknowledge,

        /// <summary>
        /// A forwarded message should have its <see cref="IReceiverMessage.Rollback"/> method called.
        /// </summary>
        Rollback,

        /// <summary>
        /// A forwarded message should have its <see cref="IReceiverMessage.Reject"/> method called.
        /// </summary>
        Reject
    }
}
