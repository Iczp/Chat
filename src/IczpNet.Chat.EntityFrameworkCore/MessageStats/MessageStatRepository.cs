using IczpNet.Chat.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;
using IczpNet.Chat.MessageStats;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.Repositories;

public class MessageStatRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : ChatRepositoryBase<MessageStat, long>(dbContextProvider), IMessageStatRepository
{

    public virtual async Task<long> StatAsync(Guid sessionId, MessageTypes messageType)
    {
        var context = await GetDbContextAsync();

        var id = long.Parse(Clock.Now.ToString("yyyyMMdd"));

        var result = await context.MessageStat
           .Where(x => x.SessionId == sessionId && x.MessageType == messageType && x.Id == id)
           .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.Count, b => b.Count + 1)
                .SetProperty(b => b.LastModificationTime, b => Clock.Now)
            );

        if (result > 0)
        {
            return result;
        }

        var messageStat = await context.MessageStat.AddAsync(new MessageStat(id)
        {
            SessionId = sessionId,
            MessageType = messageType,
            Count = 1,
            CreationTime = Clock.Now,
        });
        return 1;
    }
}
