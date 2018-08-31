namespace RockLib.Messaging
{
    /// <summary>
    /// Defines the interface for a received message.
    /// </summary>
    public interface IReceiverMessage
    {
        string StringPayload { get; }

        byte[] BinaryPayload { get; }

        HeaderDictionary Headers { get; }

        byte? Priority { get; }

        bool IsTransactional { get; }

        void Acknowledge();

        void Rollback();

        SenderMessage ToSenderMessage();
    }
}