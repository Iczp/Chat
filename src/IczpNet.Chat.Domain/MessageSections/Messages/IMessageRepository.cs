using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageRepository : IRepository<Message, long>
    {

        Task<int> BatchUpdateReadedCountAsync(List<long> messageIdList);

        Task<int> BatchUpdateOpenedCountAsync(List<long> messageIdList);

        Task<int> BatchUpdateFavoritedCountAsync(List<long> messageIdList);

        Task<int> BatchUpdateRecorderAsync(List<long> messageIdList);

    }
}
