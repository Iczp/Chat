using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Sessions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.Repositories;

public class SessionRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : ChatRepositoryBase<Session, Guid>(dbContextProvider), ISessionRepository
{
    public virtual async Task<int> UpdateLastMessageIdAsync(Guid sessionId, long lastMessageId)
    {
        var context = await GetDbContextAsync();

        return await context.Session
            .Where(x => x.Id == sessionId)
            .Where(x => x.LastMessageId == null || x.LastMessageId.Value < lastMessageId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.LastModificationTime, b => Clock.Now)
                .SetProperty(b => b.LastMessageId, b => lastMessageId)
            );
    }

    public async Task<long> UpdateLastMessageSeqAsync(Guid sessionId,  CancellationToken cancellationToken = default)
    {
        var _dbContext = await GetDbContextAsync();

        if (_dbContext.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException("Allocating a session message sequence requires a transactional unit of work.");
        }

        var trackedSessions = _dbContext.ChangeTracker.Entries<Session>()
            .Where(x => x.Entity.Id == sessionId)
            .ToArray();
        if (trackedSessions.Any(x => x.Property(s => s.LastMessageSeq).IsModified))
        {
            throw new InvalidOperationException("A tracked session has an uncommitted sequence change.");
        }

        // 1. 原子递增当前 Session 的消息序号
        var affected = await _dbContext.Session
            .Where(x => x.Id == sessionId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    x => x.LastMessageSeq,
                    x => x.LastMessageSeq + 1),
                cancellationToken);

        if (affected != 1)
        {
            throw new Exception($"不存在Session Id:{sessionId}");
        }

        // 2. 读取刚才分配到的序号
        var sessionSeq = await _dbContext.Session
            .AsNoTracking()
            .Where(x => x.Id == sessionId)
            .Select(x => x.LastMessageSeq)
            .SingleAsync(cancellationToken);

        // ExecuteUpdate bypasses the tracker. Keep a tracked Session from writing an old value later.
        foreach (var entry in trackedSessions)
        {
            var property = entry.Property(x => x.LastMessageSeq);
            property.CurrentValue = sessionSeq;
            property.OriginalValue = sessionSeq;
            property.IsModified = false;
        }
        
        return sessionSeq;
    }
}
