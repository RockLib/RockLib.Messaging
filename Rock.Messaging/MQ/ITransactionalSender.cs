#if ROCKLIB
namespace RockLib.Messaging
#else
namespace Rock.Messaging
#endif
{
    /// <summary>
    /// Defines an interface for sending messages transactionally.
    /// </summary>
    public interface ITransactionalSender : ISender
    {
        /// <summary>
        /// Starts a message-sending transaction.
        /// </summary>
        /// <returns>An object representing the new transaction.</returns>
        ISenderTransaction BeginTransaction();
    }
}