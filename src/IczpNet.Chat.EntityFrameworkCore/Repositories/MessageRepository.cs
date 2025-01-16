using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.MessageSections.Messages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.Repositories;

public class MessageRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : ChatRepositoryBase<Message, long>(dbContextProvider), IMessageRepository
{
    public virtual async Task<int> IncrementReadedCountAsync(List<long> messageIdList)
    {
        var context = await GetDbContextAsync();

        return await context.ReadedCounter
            .Where(x => messageIdList.Contains(x.MessageId))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.Count, b => b.Count + 1)
            );
    }

    public virtual async Task<int> IncrementOpenedCountAsync(List<long> messageIdList)
    {
        var context = await GetDbContextAsync();

        return await context.OpenedCounter
            .Where(x => messageIdList.Contains(x.MessageId))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.Count, b => b.Count + 1)
            );
    }

    public virtual async Task<int> IncrementFavoritedCountAsync(List<long> messageIdList)
    {
        var context = await GetDbContextAsync();

        return await context.FavoritedCounter
            .Where(x => messageIdList.Contains(x.MessageId))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.Count, b => b.Count + 1)
            );
    }

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
