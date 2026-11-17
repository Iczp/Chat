using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages;

public partial interface IMessageChangeRepository
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
}
