using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageRepository : IRepository<Message, long>
    {

        Task<int> IncrementReadedCountAsync(List<long> messageIdList);

        Task<int> IncrementOpenedCountAsync(List<long> messageIdList);

        Task<int> IncrementFavoritedCountAsync(List<long> messageIdList);

        //Task<int> IncrementRecorderAsync(List<long> messageIdList);

    }
}
