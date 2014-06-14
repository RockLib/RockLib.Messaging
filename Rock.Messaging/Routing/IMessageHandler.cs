using System.Threading.Tasks;

namespace Rock.Messaging.Routing
{
    public interface IMessageHandler<TMessage>
        where TMessage : IMessage
    {
        Task<IMessage> Handle(TMessage message);
    }
}