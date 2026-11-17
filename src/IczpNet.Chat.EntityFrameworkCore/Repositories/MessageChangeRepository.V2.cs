using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Specifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.Repositories;

public partial class MessageChangeRepository
{
    public async Task<List<MessageChangeUnitScope>> GetV2ScopesAsync(long ownerId, List<Guid> sessionUnitIds,
        CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
        return await db.SessionUnit.AsNoTracking()
            .Where(x => sessionUnitIds.Contains(x.Id) && x.OwnerId == ownerId && x.SessionId.HasValue && !x.Setting.IsKilled)
            .Select(x => new MessageChangeUnitScope(x.Id, x.SessionId.Value, x.OwnerId, x.Setting.ClearTime))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<MessageChangeV2State>> ReadV2ItemsAsync(MessageChangeUnitScope scope,
        List<long> ids, CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
        return await db.Message.AsNoTracking()
            .Where(x => x.SessionId == scope.SessionId && ids.Contains(x.Id))
            .Where(new OwnerMessageSpecification(scope.OwnerId, scope.UnitId, includeDeleted: true).ToExpression())
            .Select(x => new MessageChangeV2State(x,
                x.DeletedList.Any(d => d.SessionUnitId == scope.UnitId),
                x.DeletedList.Where(d => d.SessionUnitId == scope.UnitId).Select(d => d.ChangeSeq).FirstOrDefault()))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<long>> ReadV2SoftDeletedIdsAsync(MessageChangeUnitScope scope,
        List<long> ids, CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
        using var filter = softDeleteFilter.Disable();
        return await db.Message.AsNoTracking()
            .Where(x => x.IsDeleted && x.SessionId == scope.SessionId && ids.Contains(x.Id) && !x.Session.IsDeleted)
            .Where(x => x.Session.UnitList.Any(unit => unit.Id == scope.UnitId && !unit.IsDeleted))
            .Where(new OwnerMessageSpecification(scope.OwnerId, scope.UnitId, includeDeleted: true).ToExpression())
            .Select(x => x.Id).ToListAsync(cancellationToken);
    }

    public async Task<List<Message>> ReadV2WindowAsync(MessageChangeUnitScope scope,
        long? minId, long? maxId, int take, bool descending, CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
        var query = db.Message.AsNoTracking().Where(x => x.SessionId == scope.SessionId)
            .Where(new OwnerMessageSpecification(scope.OwnerId, scope.UnitId).ToExpression());
        if (minId.HasValue) query = query.Where(x => x.Id > minId.Value);
        if (maxId.HasValue) query = query.Where(x => x.Id < maxId.Value);
        var ordered = descending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id);
        return await ordered.Take(take).ToListAsync(cancellationToken);
    }

    public async Task<long> GetHeadChangeIdAsync(CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
        return await db.Set<MessageChange>().AsNoTracking()
            .MaxAsync(x => (long?)x.Id, cancellationToken) ?? 0L;
    }

    public async Task<MessageChangeCursor> GetCursorAsync(Guid? tenantId, Guid userId, long chatObjectId, string deviceId,
        CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
        return await db.Set<MessageChangeCursor>().AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId && x.ChatObjectId == chatObjectId && x.DeviceId == deviceId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<MessageChangeCursor> UpsertCursorAsync(Guid? tenantId, Guid userId, long chatObjectId, string deviceId,
        long lastAppliedMessageChangeId, DateTime syncTime, bool reset = false,
        CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
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

        var db = await provider.GetDbContextAsync();
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
                  && m.MessageChangeId == c.Id
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
