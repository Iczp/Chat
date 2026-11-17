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

    public async Task<List<MessageChangeV2Entry>> ReadV2ChangesAsync(MessageChangeUnitScope scope,
        long after, long through, int take, CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
        var messages = db.Message.AsNoTracking().Where(x => x.SessionId == scope.SessionId)
            .Where(x => x.ChangeSeq > after && x.ChangeSeq <= through)
            .Where(new OwnerMessageSpecification(scope.OwnerId, scope.UnitId).ToExpression())
            .Select(x => new
            {
                Version = x.ChangeSeq,
                MessageId = (long?)x.Id,
                Kind = x.IsRollbacked || x.RollbackTime.HasValue ? MessageChangeKind.Rollback : MessageChangeKind.Update,
                Time = x.RollbackTime
            });
        // The current member's own deletion remains actionable even when the
        // message is already hidden by deletion or a later clear.
        var deletions = db.DeletedRecorder.AsNoTracking()
            .Where(x => x.SessionUnitId == scope.UnitId && x.ChangeSeq > after && x.ChangeSeq <= through)
            .Select(x => new
            {
                Version = x.ChangeSeq,
                MessageId = (long?)x.MessageId,
                Kind = MessageChangeKind.Delete,
                Time = (DateTime?)null
            });
        var clear = db.SessionUnitSetting.AsNoTracking()
            .Where(x => x.SessionUnitId == scope.UnitId && x.ClearChangeSeq > after && x.ClearChangeSeq <= through)
            .Select(x => new
            {
                Version = x.ClearChangeSeq,
                MessageId = (long?)null,
                Kind = MessageChangeKind.Clear,
                Time = x.ClearTime
            });
        return await messages.Concat(deletions).Concat(clear)
            .OrderBy(x => x.Version).Take(take)
            .Select(x => new MessageChangeV2Entry(x.Version.Value, x.MessageId, x.Kind, x.Time))
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
        // Disclose only an ID proven to belong to this active member's
        // visible history. Keep all other entity filters, including tenancy.
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
}
