using IczpNet.Chat.MessageSections.Messages;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionUnitMessages;

public interface ISessionUnitMessageRepository : IRepository<SessionUnitMessage>
{
    Task<int> InsertMessagesForAllAsync(Message message);
}
