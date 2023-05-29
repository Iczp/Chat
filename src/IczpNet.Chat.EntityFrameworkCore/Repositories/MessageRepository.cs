using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.MessageSections.Messages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.Repositories
{
    public class MessageRepository : ChatRepositoryBase<Message, long>, IMessageRepository
    {
        public MessageRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        //public virtual async Task<int> IncrementReadedCountAsync(List<long> messageIdList)
        //{
        //    var context = await GetDbContextAsync();

        //    return await context.Message
        //        .Where(x => messageIdList.Contains(x.Id))
        //        .ExecuteUpdateAsync(s => s
        //            .SetProperty(b => b.ReadedCount, b => b.ReadedCount + 1)
        //        );
        //}

        //public virtual async Task<int> IncrementOpenedCountAsync(List<long> messageIdList)
        //{
        //    var context = await GetDbContextAsync();

        //    return await context.Message
        //        .Where(x => messageIdList.Contains(x.Id))
        //        .ExecuteUpdateAsync(s => s
        //            .SetProperty(b => b.OpenedCount, b => b.OpenedCount + 1)
        //        );
        //}

        //public virtual async Task<int> IncrementFavoritedCountAsync(List<long> messageIdList)
        //{
        //    var context = await GetDbContextAsync();

        //    return await context.Message
        //        .Where(x => messageIdList.Contains(x.Id))
        //        .ExecuteUpdateAsync(s => s
        //            .SetProperty(b => b.FavoritedCount, b => b.FavoritedCount + 1)
        //        );
        //}

        //public virtual async Task<int> IncrementRecorderAsync(List<long> messageIdList)
        //{
        //    var context = await GetDbContextAsync();

        //    return await context.Message
        //        .Where(x => messageIdList.Contains(x.Id))
        //        .ExecuteUpdateAsync(s => s
        //            .SetProperty(b => b.FavoritedCount, b => context.FavoritedRecorder.Count(f => f.MessageId == b.Id))
        //            .SetProperty(b => b.ReadedCount, b => context.ReadedRecorder.Count(f => f.MessageId == b.Id))
        //            .SetProperty(b => b.OpenedCount, b => context.OpenedRecorder.Count(f => f.MessageId == b.Id))
        //        );
        //}
    }
}
