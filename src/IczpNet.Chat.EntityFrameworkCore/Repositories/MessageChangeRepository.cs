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

public partial class MessageChangeRepository(IDbContextProvider<ChatDbContext> provider,
    IDataFilter<ISoftDelete> softDeleteFilter) : IMessageChangeRepository, ITransientDependency
{
    public async Task<MessageChangeEntry> AppendRollbackAsync(Message message, DateTime time, CancellationToken cancellationToken = default)
    {
        var db = await provider.GetDbContextAsync();
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

        var sessionId = message.SessionId!.Value;
        var change = new MessageChange(sessionId, message.Id, MessageChangeKind.Rollback, time);
        db.Set<MessageChange>().Add(change);
        await db.SaveChangesAsync(cancellationToken);

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

        await AcquireTransactionWriteLockAsync(db, cancellationToken);

        var exists = await db.Message.AsNoTracking().AnyAsync(x => x.Id == message.Id, cancellationToken);
        if (!exists)
            throw new BusinessException("Chat:MessageUnavailable");

        var sessionId = message.SessionId!.Value;
        var change = new MessageChange(sessionId, message.Id, MessageChangeKind.Update, time);
        db.Set<MessageChange>().Add(change);
        await db.SaveChangesAsync(cancellationToken);

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
        var db = await provider.GetDbContextAsync();
        var unit = await db.SessionUnit.AsNoTracking().SingleAsync(x => x.Id == sessionUnitId, cancellationToken);
        if (await db.Set<DeletedRecorder>().AnyAsync(x => x.SessionUnitId == sessionUnitId && x.MessageId == messageId, cancellationToken))
            return new MessageChangeEntry { AlreadyApplied = true, MessageId = messageId, SessionId = unit.SessionId!.Value };

        var message = await db.Message.AsNoTracking()
            .Where(x => x.Id == messageId && x.SessionId == unit.SessionId)
            .Where(new OwnerMessageSpecification(unit.OwnerId, sessionUnitId).ToExpression())
            .SingleOrDefaultAsync(cancellationToken);
        if (message == null) throw new BusinessException("Chat:MessageUnavailable");

        var version = time.Ticks;
        db.Set<DeletedRecorder>().Add(new DeletedRecorder(unit, messageId, deviceId, version));
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
        var db = await provider.GetDbContextAsync();
        var sessionId = await db.SessionUnit.Where(x => x.Id == sessionUnitId)
            .Select(x => x.SessionId!.Value).SingleAsync(cancellationToken);
        var previous = await db.Set<SessionUnitSetting>().Where(x => x.SessionUnitId == sessionUnitId)
            .Select(x => new { x.ClearTime, x.ClearChangeSeq }).SingleAsync(cancellationToken);
        if (previous.ClearTime >= clearTime)
        {
            return new MessageChangeEntry
            {
                SessionId = sessionId,
                ReceiverSessionUnitId = sessionUnitId,
                ChangeType = MessageChangeKind.Clear,
                ChangedAt = previous.ClearTime.Value,
                Version = previous.ClearChangeSeq.GetValueOrDefault(),
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
}
