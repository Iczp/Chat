using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageRepository : IRepository<Message, long>
    {
    }
}
