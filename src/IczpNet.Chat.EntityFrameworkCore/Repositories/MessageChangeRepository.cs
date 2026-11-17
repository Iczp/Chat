using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Specifications;
using IczpNet.Chat.DeletedRecorders;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp;

namespace IczpNet.Chat.Repositories;

public partial class MessageChangeRepository(IDbContextProvider<ChatDbContext> provider,
    IDataFilter<ISoftDelete> softDeleteFilter) : IMessageChangeRepository, ITransientDependency
{
    public async Task<MessageChangeEntry> AppendRollbackAsync(Message message, DateTime time, CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
        if (db.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Message changes require a transactional unit of work.");

        // Acquire the global write gate lock via EF Core ExecuteUpdateAsync so
        // MessageChange IDENTITY ids are allocated strictly in commit order.
        await AcquireWriteGateLockAsync(db, cancellationToken);

        var existing = await db.Message.AsNoTracking().SingleAsync(x => x.Id == message.Id, cancellationToken);
        if (existing.IsRollbacked || existing.RollbackTime.HasValue)
        {
            return new MessageChangeEntry
            {
                AlreadyApplied = true,
                MessageId = message.Id,
                Version = existing.MessageChangeId ?? existing.SyncVersion,
                ChangeType = MessageChangeKind.Rollback,
                ChangedAt = existing.RollbackTime ?? time
            };
        }

        var sessionId = message.SessionId!.Value;
        var change = new MessageChange(sessionId, message.Id, MessageChangeKind.Rollback, time);
        db.Set<MessageChange>().Add(change);
        await db.SaveChangesAsync(cancellationToken);

        await CommitWriteGatePositionAsync(db, change.Id, cancellationToken);
        return new MessageChangeEntry
        {
            SessionId = sessionId,
            MessageId = message.Id,
            SessionSeq = message.SessionSeq,
            Version = change.Id,
            ChangeType = MessageChangeKind.Rollback,
            ChangedAt = time
        };
    }

    public async Task<MessageChangeEntry> AppendUpdateAsync(Message message, DateTime time, CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
        if (db.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Message changes require a transactional unit of work.");

        await AcquireWriteGateLockAsync(db, cancellationToken);

        var exists = await db.Message.AsNoTracking().AnyAsync(x => x.Id == message.Id, cancellationToken);
        if (!exists)
            throw new BusinessException("Chat:MessageUnavailable");

        var sessionId = message.SessionId!.Value;
        var change = new MessageChange(sessionId, message.Id, MessageChangeKind.Update, time);
        db.Set<MessageChange>().Add(change);
        await db.SaveChangesAsync(cancellationToken);

        await CommitWriteGatePositionAsync(db, change.Id, cancellationToken);
        return new MessageChangeEntry
        {
            SessionId = sessionId,
            MessageId = message.Id,
            SessionSeq = message.SessionSeq,
            Version = change.Id,
            ChangeType = MessageChangeKind.Update,
            ChangedAt = time
        };
    }

    private static async Task AcquireWriteGateLockAsync(ChatDbContext db, CancellationToken token)
    {
        var affected = await db.Set<MessageChangeWriteGate>()
            .Where(x => x.Id == MessageChangeWriteGate.DefaultId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Revision, x => x.Revision + 1), token);
        if (affected == 0)
        {
            db.Set<MessageChangeWriteGate>().Add(new MessageChangeWriteGate(
                MessageChangeWriteGate.DefaultId,
                MessageChangeWriteGate.InitialEpoch,
                revision: 1));
            await db.SaveChangesAsync(token);
        }
    }

    private static Task CommitWriteGatePositionAsync(ChatDbContext db, long changeId, CancellationToken token)
        => db.Set<MessageChangeWriteGate>()
            .Where(x => x.Id == MessageChangeWriteGate.DefaultId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.LastCommittedChangeId, changeId), token);

    private static async Task LockStateAsync(ChatDbContext db, Guid sessionId, CancellationToken token)
    {
        if (db.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Message changes require a transactional unit of work.");
        var table = db.Model.FindEntityType(typeof(SessionMessageChangeState))!;
        var name = db.GetService<ISqlGenerationHelper>().DelimitIdentifier(table.GetTableName()!, table.GetSchema());
        var command = $"IF NOT EXISTS (SELECT 1 FROM {name} WITH (UPDLOCK,HOLDLOCK) WHERE [Id] = {{0}}) INSERT INTO {name} ([Id],[Epoch],[Version],[MinVersion]) VALUES ({{0}},{{1}},0,0)";
        await db.Database.ExecuteSqlRawAsync(command, new object[] { sessionId, sessionId }, token);
    }

    private static async Task<long> AllocateVersionAsync(ChatDbContext db, Guid sessionId, CancellationToken token)
    {
        var state = db.Set<SessionMessageChangeState>().Where(x => x.Id == sessionId);
        await state.ExecuteUpdateAsync(s => s.SetProperty(x => x.Version, x => x.Version + 1), token);
        return await state.Select(x => x.Version).SingleAsync(token);
    }

    public Task<MessageChangeEntry> AppendDeleteAsync(Guid sessionUnitId, long messageId, DateTime time,
        CancellationToken cancellationToken = default)
        => AppendDeleteAsync(sessionUnitId, messageId, time, null, cancellationToken);

    public async Task<MessageChangeEntry> AppendDeleteAsync(Guid sessionUnitId, long messageId, DateTime time,
        string deviceId, CancellationToken cancellationToken)
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
        var version = await AllocateVersionAsync(db, unit.SessionId.Value, cancellationToken);
        // Both rows are saved by the same outer transaction. The existing
        // DeletedRecorder entity event invalidates member visibility caches.
        db.Set<DeletedRecorder>().Add(new DeletedRecorder(unit, messageId, deviceId, version));
        return new MessageChangeEntry { SessionId = unit.SessionId.Value, MessageId = messageId, SessionSeq = message.SessionSeq,
            Version = version, ChangeType = MessageChangeKind.Delete, ChangedAt = time, ReceiverSessionUnitId = sessionUnitId };
    }

    public async Task<MessageChangeEntry> AllocateClearVersionAsync(Guid sessionUnitId, DateTime clearTime,
        CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
        var sessionId = await db.SessionUnit.Where(x => x.Id == sessionUnitId)
            .Select(x => x.SessionId!.Value).SingleAsync(cancellationToken);
        await LockStateAsync(db, sessionId, cancellationToken);
        var previous = await db.Set<SessionUnitSetting>().Where(x => x.SessionUnitId == sessionUnitId)
            .Select(x => new { x.ClearTime, x.ClearChangeSeq }).SingleAsync(cancellationToken);
        if (previous.ClearTime >= clearTime)
        {
            return new MessageChangeEntry
            {
                SessionId = sessionId, ReceiverSessionUnitId = sessionUnitId, ChangeType = MessageChangeKind.Clear,
                ChangedAt = previous.ClearTime.Value, Version = previous.ClearChangeSeq.GetValueOrDefault(), AlreadyApplied = true
            };
        }

        return new MessageChangeEntry
        {
            SessionId = sessionId, ReceiverSessionUnitId = sessionUnitId, ChangeType = MessageChangeKind.Clear,
            ChangedAt = clearTime, Version = await AllocateVersionAsync(db, sessionId, cancellationToken)
        };
    }

    public async Task<MessageChangeCheckpoint> GetCheckpointAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
        return await db.Set<SessionMessageChangeState>().AsNoTracking().Where(x => x.Id == sessionId)
            .Select(x => new MessageChangeCheckpoint(x.Epoch, x.Version, x.MinVersion))
            .SingleOrDefaultAsync(cancellationToken) ?? new MessageChangeCheckpoint(sessionId, 0, 0);
    }

    public async Task<List<Guid>> GetRollbackRecipientsAsync(Guid sessionId, long messageId, List<Guid> candidateUnitIds,
        CancellationToken cancellationToken = default)
    {
        if (candidateUnitIds.Count == 0)
            return [];

        var db = await provider.GetDbContextAsync();
        var eligible = from message in db.Message
                      where message.Id == messageId && message.SessionId == sessionId
                      from unit in message.Session.UnitList
                      where !unit.Setting.IsKilled &&
                            (!message.IsPrivate || message.SenderSessionUnitId == unit.Id || message.ReceiverSessionUnitId == unit.Id) &&
                            (!unit.Setting.StartMessageId.HasValue || message.Id > unit.Setting.StartMessageId) &&
                            (!unit.Setting.EndMessageId.HasValue || message.Id <= unit.Setting.EndMessageId) &&
                            (!unit.Setting.HistoryFristTime.HasValue || message.CreationTime >= unit.Setting.HistoryFristTime) &&
                            (!unit.Setting.HistoryLastTime.HasValue || message.CreationTime < unit.Setting.HistoryLastTime) &&
                            (!unit.Setting.ClearTime.HasValue || message.CreationTime > unit.Setting.ClearTime) &&
                            !message.DeletedList.Any(recorder => recorder.SessionUnitId == unit.Id)
                      select unit.Id;
        var recipients = new List<Guid>();
        foreach (var batch in candidateUnitIds.Distinct().Chunk(500))
            recipients.AddRange(await eligible.Where(id => batch.Contains(id)).ToListAsync(cancellationToken));
        return recipients;
    }

    public Task<List<MessageChangeEntry>> ReadChangesAsync(Guid sessionId, long after, long through, int take, CancellationToken cancellationToken = default)
        => Task.FromResult<List<MessageChangeEntry>>([]);

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
