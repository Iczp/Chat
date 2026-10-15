using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.Repositories;

public class MessageChangeRepository(IDbContextProvider<ChatDbContext> provider) : IMessageChangeRepository, ITransientDependency
{
    public async Task<MessageChangeEntry> AppendRollbackAsync(Message message, DateTime time, CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
        if (db.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Message changes require a transactional unit of work.");
        var sessionId = message.SessionId!.Value;
        // HOLDLOCK protects the absent-key case as well. Every change writer takes
        // this lock before changing a Message, and holds it until the outer commit.
        var table = db.Model.FindEntityType(typeof(SessionMessageChangeState))!;
        var name = db.GetService<ISqlGenerationHelper>().DelimitIdentifier(table.GetTableName()!, table.GetSchema());
        // Only escaped EF model identifiers are composed; all data are parameters.
        var command = $"IF NOT EXISTS (SELECT 1 FROM {name} WITH (UPDLOCK,HOLDLOCK) WHERE [Id] = {{0}}) INSERT INTO {name} ([Id],[Epoch],[Version],[MinVersion]) VALUES ({{0}},{{1}},0,0)";
        await db.Database.ExecuteSqlRawAsync(command,
            new object[] { sessionId, sessionId }, cancellationToken);
        var existing = await db.Message.AsNoTracking().SingleAsync(x => x.Id == message.Id, cancellationToken);
        if (existing.IsRollbacked || existing.RollbackTime.HasValue)
            return new MessageChangeEntry { AlreadyApplied = true, MessageId = message.Id, Version = existing.SyncVersion,
                ChangeType = MessageChangeKind.Rollback, ChangedAt = existing.RollbackTime ?? time };
        await db.Set<SessionMessageChangeState>().Where(x => x.Id == sessionId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Version, x => x.Version + 1), cancellationToken);
        var version = await db.Set<SessionMessageChangeState>().Where(x => x.Id == sessionId)
            .Select(x => x.Version).SingleAsync(cancellationToken);
        db.Set<MessageChange>().Add(new MessageChange(Guid.NewGuid(), message, version, time));
        return new MessageChangeEntry { MessageId = message.Id, Version = version,
            ChangeType = MessageChangeKind.Rollback, ChangedAt = time };
    }

    public async Task<MessageChangeCheckpoint> GetCheckpointAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
        return await db.Set<SessionMessageChangeState>().AsNoTracking().Where(x => x.Id == sessionId)
            .Select(x => new MessageChangeCheckpoint(x.Epoch, x.Version, x.MinVersion))
            .SingleOrDefaultAsync(cancellationToken) ?? new MessageChangeCheckpoint(sessionId, 0, 0);
    }

    public async Task<List<MessageChangeEntry>> ReadChangesAsync(Guid sessionId, long after, long through, int take, CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
        return await db.Set<MessageChange>().AsNoTracking()
            .Where(x => x.SessionId == sessionId && x.Version > after && x.Version <= through)
            .OrderBy(x => x.Version).Take(take).Select(x => new MessageChangeEntry
            {
                MessageId = x.MessageId, Version = x.Version, ChangeType = x.ChangeType,
                ChangedAt = x.ChangedAt, CreationTime = x.MessageCreationTime, IsPrivate = x.IsPrivate,
                SenderSessionUnitId = x.SenderSessionUnitId, ReceiverSessionUnitId = x.ReceiverSessionUnitId
            }).ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, MessageChangeCheckpoint>> GetCheckpointsAsync(List<Guid> sessionIds, CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
        var heads = await db.Set<SessionMessageChangeState>().AsNoTracking().Where(x => sessionIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Epoch, x.Version, x.MinVersion }).ToListAsync(cancellationToken);
        // Keep an empty session's initial epoch stable across its first change;
        // otherwise every device would unnecessarily recalibrate its old ids.
        // Explicit log resets must rotate the persisted epoch, never delete it.
        var result = sessionIds.Distinct().ToDictionary(id => id, id => new MessageChangeCheckpoint(id, 0, 0));
        foreach (var head in heads) result[head.Id] = new(head.Epoch, head.Version, head.MinVersion);
        return result;
    }

    public async Task<List<MessageSyncItem>> ReadSyncItemsAsync(Guid sessionUnitId, List<long> ids, CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
        var unit = await db.SessionUnit.AsNoTracking().SingleAsync(x => x.Id == sessionUnitId, cancellationToken);
        var messages = await db.Message.AsNoTracking().Where(x => ids.Contains(x.Id) && x.SessionId == unit.SessionId)
            .Where(new OwnerMessageSpecification(unit.OwnerId, sessionUnitId).ToExpression())
            .ToListAsync(cancellationToken);
        return messages.Select(x => new MessageSyncItem { MessageId = x.Id, SyncVersion = x.SyncVersion,
            IsRollbacked = x.IsRollbacked, RollbackTime = x.RollbackTime }).ToList();
    }
}
