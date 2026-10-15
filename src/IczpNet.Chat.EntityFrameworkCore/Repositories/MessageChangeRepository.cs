using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Specifications;
using IczpNet.Chat.DeletedRecorders;
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
using Volo.Abp;

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
        await LockStateAsync(db, sessionId, cancellationToken);
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

    private static async Task LockStateAsync(ChatDbContext db, Guid sessionId, CancellationToken token)
    {
        if (db.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Message changes require a transactional unit of work.");
        var table = db.Model.FindEntityType(typeof(SessionMessageChangeState))!;
        var name = db.GetService<ISqlGenerationHelper>().DelimitIdentifier(table.GetTableName()!, table.GetSchema());
        var command = $"IF NOT EXISTS (SELECT 1 FROM {name} WITH (UPDLOCK,HOLDLOCK) WHERE [Id] = {{0}}) INSERT INTO {name} ([Id],[Epoch],[Version],[MinVersion]) VALUES ({{0}},{{1}},0,0)";
        await db.Database.ExecuteSqlRawAsync(command, new object[] { sessionId, sessionId }, token);
    }

    public async Task<MessageChangeEntry> AppendDeleteAsync(Guid sessionUnitId, long messageId, DateTime time,
        CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
        var unit = await db.SessionUnit.AsNoTracking().SingleAsync(x => x.Id == sessionUnitId, cancellationToken);
        await LockStateAsync(db, unit.SessionId!.Value, cancellationToken);
        if (await db.Set<DeletedRecorder>().AnyAsync(x => x.SessionUnitId == sessionUnitId && x.MessageId == messageId, cancellationToken))
            return new MessageChangeEntry { AlreadyApplied = true, MessageId = messageId, SessionId = unit.SessionId.Value };
        var message = await db.Message.AsNoTracking()
            .Where(x => x.Id == messageId && x.SessionId == unit.SessionId)
            .Where(new OwnerMessageSpecification(unit.OwnerId, sessionUnitId).ToExpression())
            .SingleOrDefaultAsync(cancellationToken);
        if (message == null) throw new BusinessException("Chat:MessageUnavailable");
        await db.Set<SessionMessageChangeState>().Where(x => x.Id == unit.SessionId.Value)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Version, x => x.Version + 1), cancellationToken);
        var version = await db.Set<SessionMessageChangeState>().Where(x => x.Id == unit.SessionId.Value)
            .Select(x => x.Version).SingleAsync(cancellationToken);
        // Both rows are saved by the same outer transaction. The existing
        // DeletedRecorder entity event invalidates member visibility caches.
        db.Set<DeletedRecorder>().Add(new DeletedRecorder(unit, messageId, null));
        db.Set<MessageChange>().Add(new MessageChange(Guid.NewGuid(), message, version, time, sessionUnitId));
        return new MessageChangeEntry { SessionId = unit.SessionId.Value, MessageId = messageId,
            Version = version, ChangeType = MessageChangeKind.Delete, ChangedAt = time, ReceiverSessionUnitId = sessionUnitId };
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
                SessionId = x.SessionId, MessageId = x.MessageId, Version = x.Version, ChangeType = x.ChangeType,
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
            .Where(new OwnerMessageSpecification(unit.OwnerId, sessionUnitId, includeDeleted: true).ToExpression())
            .ToListAsync(cancellationToken);
        var deletedIds = (await db.Set<DeletedRecorder>().AsNoTracking()
            .Where(x => x.SessionUnitId == sessionUnitId && ids.Contains(x.MessageId))
            .Select(x => x.MessageId).ToListAsync(cancellationToken)).ToHashSet();
        // Calibration is a state snapshot. Deletion is terminal for this
        // member; the committed head certifies it without scanning an entire
        // session journal for each batch of old IDs.
        var deletionVersion = deletedIds.Count == 0 ? 0 : (await GetCheckpointAsync(unit.SessionId!.Value, cancellationToken)).Version;
        return messages.Select(x => new MessageSyncItem { MessageId = x.Id,
            SyncVersion = deletedIds.Contains(x.Id) ? Math.Max(x.SyncVersion, deletionVersion) : x.SyncVersion, IsDeleted = deletedIds.Contains(x.Id),
            IsRollbacked = x.IsRollbacked, RollbackTime = x.RollbackTime }).ToList();
    }
}
