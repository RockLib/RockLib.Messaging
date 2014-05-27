using System.Threading.Tasks;

namespace Rock.Messaging.Routing
{
    public interface IMessageHandler<TMessage>
        where TMessage : IMessage
    {
        Task<TMessage> Handle(TMessage message);
    }
}