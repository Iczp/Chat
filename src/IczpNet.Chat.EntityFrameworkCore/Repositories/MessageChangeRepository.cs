using IczpNet.Chat.DeletedRecorders;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.Specifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.Repositories;

public class MessageChangeRepository(
    IDbContextProvider<ChatDbContext> provider,
    IDataFilter<ISoftDelete> softDeleteFilter) : ChatRepositoryBase<MessageChange, long>(provider), IMessageChangeRepository, ITransientDependency
{
    public async Task<MessageChangeEntry> AppendRollbackAsync(Message message, DateTime time, CancellationToken cancellationToken = default)
    {
        var db = await GetDbContextAsync();
        if (db.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Message changes require a transactional unit of work.");

        await AcquireTransactionWriteLockAsync(db, cancellationToken);

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

        // 先软删该 MessageId 现有的未删除记录并立即保存，释放唯一过滤索引 [IsDeleted] = 0
        await SoftDeleteExistingActiveChangesAsync(db, message.Id, time, cancellationToken);

        var sessionId = message.SessionId!.Value;
        var change = new MessageChange(sessionId, message.Id, MessageChangeKind.Rollback, time);
        await InsertAsync(change, autoSave: true, cancellationToken: cancellationToken);

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
        var db = await GetDbContextAsync();
        if (db.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Message changes require a transactional unit of work.");

        await AcquireTransactionWriteLockAsync(db, cancellationToken);

        var exists = await db.Message.AsNoTracking().AnyAsync(x => x.Id == message.Id, cancellationToken);
        if (!exists)
            throw new BusinessException("Chat:MessageUnavailable");

        // 先软删该 MessageId 现有的未删除记录并立即保存，释放唯一过滤索引 [IsDeleted] = 0
        await SoftDeleteExistingActiveChangesAsync(db, message.Id, time, cancellationToken);

        var sessionId = message.SessionId!.Value;
        var change = new MessageChange(sessionId, message.Id, MessageChangeKind.Update, time);
        await InsertAsync(change, autoSave: true, cancellationToken: cancellationToken);

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

    private async Task SoftDeleteExistingActiveChangesAsync(ChatDbContext db, long messageId, DateTime deletionTime, CancellationToken cancellationToken)
    {
        var activeChanges = await db.Set<MessageChange>()
            .Where(x => x.MessageId == messageId && !x.IsDeleted)
            .ToListAsync(cancellationToken);

        if (activeChanges.Count == 0)
            return;

        await DeleteManyAsync(activeChanges, autoSave: true, cancellationToken: cancellationToken);
        var updatedDeletionTime = false;
        foreach (var item in activeChanges)
        {
            if (!item.DeletionTime.HasValue)
            {
                db.Entry(item).Property(x => x.DeletionTime).CurrentValue = deletionTime;
                updatedDeletionTime = true;
            }
        }

        if (updatedDeletionTime)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task AcquireTransactionWriteLockAsync(ChatDbContext db, CancellationToken token)
    {
        if (!db.Database.IsSqlServer())
            return;

        // Serialize shared MessageChange IDENTITY commits without an extra gate table.
        await db.Database.ExecuteSqlRawAsync(
            "DECLARE @r int; EXEC @r = sp_getapplock @Resource = N'Chat_MessageChange_WriteGate', @LockMode = N'Exclusive', @LockOwner = N'Transaction', @LockTimeout = 30000; IF @r < 0 THROW 50001, 'Failed to acquire MessageChange write lock.', 1;",
            token);
    }

    public Task<MessageChangeEntry> AppendDeleteAsync(Guid sessionUnitId, long messageId, DateTime time,
        CancellationToken cancellationToken = default)
        => AppendDeleteAsync(sessionUnitId, messageId, time, null, cancellationToken);

    public async Task<MessageChangeEntry> AppendDeleteAsync(Guid sessionUnitId, long messageId, DateTime time,
        string deviceId, CancellationToken cancellationToken)
    {
        var db = await GetDbContextAsync();
        var unit = await db.SessionUnit.AsNoTracking().SingleAsync(x => x.Id == sessionUnitId, cancellationToken);
        if (await db.Set<DeletedRecorder>().AnyAsync(x => x.SessionUnitId == sessionUnitId && x.MessageId == messageId, cancellationToken))
            return new MessageChangeEntry { AlreadyApplied = true, MessageId = messageId, SessionId = unit.SessionId!.Value };

        var message = await db.Message.AsNoTracking()
            .Where(x => x.Id == messageId && x.SessionId == unit.SessionId)
            .Where(new OwnerMessageSpecification(unit.OwnerId, sessionUnitId).ToExpression())
            .SingleOrDefaultAsync(cancellationToken);
        if (message == null) throw new BusinessException("Chat:MessageUnavailable");

        var version = time.Ticks;
        db.Set<DeletedRecorder>().Add(new DeletedRecorder(unit, messageId, deviceId));
        return new MessageChangeEntry
        {
            SessionId = unit.SessionId!.Value,
            MessageId = messageId,
            SessionSeq = message.SessionSeq,
            Version = version,
            ChangeType = MessageChangeKind.Delete,
            ChangedAt = time,
            ReceiverSessionUnitId = sessionUnitId
        };
    }

    public async Task<MessageChangeEntry> AllocateClearVersionAsync(Guid sessionUnitId, DateTime clearTime,
        CancellationToken cancellationToken = default)
    {
        var db = await GetDbContextAsync();
        var sessionId = await db.SessionUnit.Where(x => x.Id == sessionUnitId)
            .Select(x => x.SessionId!.Value).SingleAsync(cancellationToken);
        var previousClearTime = await db.Set<SessionUnitSetting>().Where(x => x.SessionUnitId == sessionUnitId)
            .Select(x => x.ClearTime).SingleAsync(cancellationToken);
        if (previousClearTime >= clearTime)
        {
            return new MessageChangeEntry
            {
                SessionId = sessionId,
                ReceiverSessionUnitId = sessionUnitId,
                ChangeType = MessageChangeKind.Clear,
                ChangedAt = previousClearTime.Value,
                Version = previousClearTime.Value.Ticks,
                AlreadyApplied = true
            };
        }

        return new MessageChangeEntry
        {
            SessionId = sessionId,
            ReceiverSessionUnitId = sessionUnitId,
            ChangeType = MessageChangeKind.Clear,
            ChangedAt = clearTime,
            Version = clearTime.Ticks
        };
    }

    public async Task<List<Guid>> GetRollbackRecipientsAsync(Guid sessionId, long messageId, List<Guid> candidateUnitIds,
        CancellationToken cancellationToken = default)
    {
        if (candidateUnitIds.Count == 0)
            return [];

        var db = await GetDbContextAsync();
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

    public async Task<List<MessageChangeUnitScope>> GetScopesAsync(long ownerId, List<Guid> sessionUnitIds,
        CancellationToken cancellationToken = default)
    {
        var db = await GetDbContextAsync();
        return await db.SessionUnit.AsNoTracking()
            .Where(x => sessionUnitIds.Contains(x.Id) && x.OwnerId == ownerId && x.SessionId.HasValue && !x.Setting.IsKilled)
            .Select(x => new MessageChangeUnitScope(x.Id, x.SessionId.Value, x.OwnerId, x.Setting.ClearTime))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<MessageChangeItemState>> ReadItemsAsync(MessageChangeUnitScope scope,
        List<long> ids, CancellationToken cancellationToken = default)
    {
        var db = await GetDbContextAsync();
        return await db.Message.AsNoTracking()
            .Where(x => x.SessionId == scope.SessionId && ids.Contains(x.Id))
            .Where(new OwnerMessageSpecification(scope.OwnerId, scope.UnitId, includeDeleted: true).ToExpression())
            .Select(x => new MessageChangeItemState(x, x.DeletedList.Any(d => d.SessionUnitId == scope.UnitId)))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<long>> ReadSoftDeletedIdsAsync(MessageChangeUnitScope scope,
        List<long> ids, CancellationToken cancellationToken = default)
    {
        var db = await GetDbContextAsync();
        using var filter = softDeleteFilter.Disable();
        return await db.Message.AsNoTracking()
            .Where(x => x.IsDeleted && x.SessionId == scope.SessionId && ids.Contains(x.Id) && !x.Session.IsDeleted)
            .Where(x => x.Session.UnitList.Any(unit => unit.Id == scope.UnitId && !unit.IsDeleted))
            .Where(new OwnerMessageSpecification(scope.OwnerId, scope.UnitId, includeDeleted: true).ToExpression())
            .Select(x => x.Id).ToListAsync(cancellationToken);
    }

    public async Task<List<Message>> ReadWindowAsync(MessageChangeUnitScope scope,
        long? minId, long? maxId, int take, bool descending, CancellationToken cancellationToken = default)
    {
        var db = await GetDbContextAsync();
        var query = db.Message.AsNoTracking().Where(x => x.SessionId == scope.SessionId)
            .Where(new OwnerMessageSpecification(scope.OwnerId, scope.UnitId).ToExpression());
        if (minId.HasValue) query = query.Where(x => x.Id > minId.Value);
        if (maxId.HasValue) query = query.Where(x => x.Id < maxId.Value);
        var ordered = descending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id);
        return await ordered.Take(take).ToListAsync(cancellationToken);
    }

    public async Task<long> GetHeadChangeIdAsync(CancellationToken cancellationToken = default)
    {
        var db = await GetDbContextAsync();
        return await db.Set<MessageChange>().AsNoTracking()
            .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0L;
    }

    public async Task<MessageChangeCursor> GetCursorAsync(Guid? tenantId, Guid userId, long chatObjectId, string deviceId,
        CancellationToken cancellationToken = default)
    {
        var db = await GetDbContextAsync();
        return await db.Set<MessageChangeCursor>().AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId && x.ChatObjectId == chatObjectId && x.DeviceId == deviceId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<MessageChangeCursor> UpsertCursorAsync(Guid? tenantId, Guid userId, long chatObjectId, string deviceId,
        long lastAppliedMessageChangeId, DateTime syncTime, bool reset = false,
        CancellationToken cancellationToken = default)
    {
        var db = await GetDbContextAsync();
        var existing = await db.Set<MessageChangeCursor>()
            .Where(x => x.TenantId == tenantId && x.UserId == userId && x.ChatObjectId == chatObjectId && x.DeviceId == deviceId)
            .SingleOrDefaultAsync(cancellationToken);
        if (existing == null)
        {
            var created = new MessageChangeCursor(
                Guid.NewGuid(),
                tenantId,
                userId,
                chatObjectId,
                deviceId,
                lastAppliedMessageChangeId,
                isInitialized: true,
                lastSyncTime: syncTime);
            db.Set<MessageChangeCursor>().Add(created);
            await db.SaveChangesAsync(cancellationToken);
            return created;
        }

        if (reset)
            existing.Reset(lastAppliedMessageChangeId, syncTime);
        else
            existing.Advance(lastAppliedMessageChangeId, syncTime);
        await db.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<MessageChangePullResult> PullChangesAsync(long chatObjectId, long afterMessageChangeId,
        long headChangeId, int maxResultCount, CancellationToken cancellationToken = default)
    {
        if (afterMessageChangeId >= headChangeId)
            return new MessageChangePullResult(headChangeId, false, []);

        var db = await GetDbContextAsync();
        var scanLimit = Math.Max(maxResultCount * 5, 500);
        var windowIds = await db.Set<MessageChange>().AsNoTracking()
            .Where(c => c.Id > afterMessageChangeId && c.Id <= headChangeId)
            .OrderBy(c => c.Id)
            .Take(scanLimit + 1)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var hasMoreByWindow = windowIds.Count > scanLimit;
        if (hasMoreByWindow)
            windowIds.RemoveAt(windowIds.Count - 1);

        var windowEnd = windowIds.Count == 0
            ? headChangeId
            : (hasMoreByWindow ? windowIds[^1] : headChangeId);

        var rows = await (
            from c in db.Set<MessageChange>().AsNoTracking()
            join m in db.Message.AsNoTracking() on c.MessageId equals m.Id
            join u in db.SessionUnit.AsNoTracking() on c.SessionId equals u.SessionId
            where c.Id > afterMessageChangeId && c.Id <= windowEnd
                  && u.OwnerId == chatObjectId && !u.IsDeleted && !u.Setting.IsKilled
                  && (!m.IsPrivate || m.SenderSessionUnitId == u.Id || m.ReceiverSessionUnitId == u.Id)
                  && (!u.Setting.StartMessageId.HasValue || m.Id > u.Setting.StartMessageId)
                  && (!u.Setting.EndMessageId.HasValue || m.Id <= u.Setting.EndMessageId)
                  && (!u.Setting.HistoryFristTime.HasValue || m.CreationTime >= u.Setting.HistoryFristTime)
                  && (!u.Setting.HistoryLastTime.HasValue || m.CreationTime < u.Setting.HistoryLastTime)
                  && (!u.Setting.ClearTime.HasValue || m.CreationTime > u.Setting.ClearTime)
                  && !m.DeletedList.Any(d => d.SessionUnitId == u.Id)
            orderby c.Id
            select new MessageChangePullRow(c.Id, c.SessionId, u.Id, c.MessageId, c.ChangeType, c.CreationTime))
            .Take(maxResultCount + 1)
            .ToListAsync(cancellationToken);

        var hasMoreByCount = rows.Count > maxResultCount;
        if (hasMoreByCount)
            rows.RemoveAt(rows.Count - 1);

        var nextCursor = hasMoreByCount ? rows[^1].MessageChangeId : windowEnd;
        var hasMore = hasMoreByCount || windowEnd < headChangeId;
        return new MessageChangePullResult(nextCursor, hasMore, rows);
    }
}
