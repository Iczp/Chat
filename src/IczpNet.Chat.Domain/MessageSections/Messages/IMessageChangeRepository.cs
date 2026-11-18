using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.MessageSections.Messages;

public interface IMessageChangeRepository : IRepository<MessageChange, long>
{
    Task<MessageChangeEntry> AppendRollbackAsync(Message message, DateTime time, CancellationToken cancellationToken = default);
    Task<MessageChangeEntry> AppendUpdateAsync(Message message, DateTime time, CancellationToken cancellationToken = default);
    Task<MessageChangeEntry> AppendDeleteAsync(Guid sessionUnitId, long messageId, DateTime time, CancellationToken cancellationToken = default);
    Task<MessageChangeEntry> AppendDeleteAsync(Guid sessionUnitId, long messageId, DateTime time, string deviceId,
        CancellationToken cancellationToken);
    Task<MessageChangeEntry> AllocateClearVersionAsync(Guid sessionUnitId, DateTime clearTime,
        CancellationToken cancellationToken = default);
    Task<List<Guid>> GetRollbackRecipientsAsync(Guid sessionId, long messageId, List<Guid> candidateUnitIds,
        CancellationToken cancellationToken = default);
    Task<List<MessageChangeUnitScope>> GetScopesAsync(long ownerId, List<Guid> sessionUnitIds,
        CancellationToken cancellationToken = default);
    Task<List<MessageChangeItemState>> ReadItemsAsync(MessageChangeUnitScope scope,
        List<long> ids, CancellationToken cancellationToken = default);
    Task<List<long>> ReadSoftDeletedIdsAsync(MessageChangeUnitScope scope, List<long> ids,
        CancellationToken cancellationToken = default);
    Task<List<Message>> ReadWindowAsync(MessageChangeUnitScope scope,
        long? minId, long? maxId, int take, bool descending, CancellationToken cancellationToken = default);
    Task<long> GetHeadChangeIdAsync(CancellationToken cancellationToken = default);
    Task<MessageChangeCursor> GetCursorAsync(Guid? tenantId, Guid userId, long chatObjectId, string deviceId,
        CancellationToken cancellationToken = default);
    Task<MessageChangeCursor> UpsertCursorAsync(Guid? tenantId, Guid userId, long chatObjectId, string deviceId,
        long lastAppliedMessageChangeId, DateTime syncTime, bool reset = false,
        CancellationToken cancellationToken = default);
    Task<MessageChangePullResult> PullChangesAsync(long chatObjectId, long afterMessageChangeId, long headChangeId,
        int maxResultCount, CancellationToken cancellationToken = default);
}
