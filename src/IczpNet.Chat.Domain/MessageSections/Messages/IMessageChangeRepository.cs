using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages;

public interface IMessageChangeRepository
{
    Task<MessageChangeEntry> AppendRollbackAsync(Message message, DateTime time, CancellationToken cancellationToken = default);
    Task<MessageChangeCheckpoint> GetCheckpointAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, MessageChangeCheckpoint>> GetCheckpointsAsync(List<Guid> sessionIds, CancellationToken cancellationToken = default);
    Task<List<MessageChangeEntry>> ReadChangesAsync(Guid sessionId, long after, long through, int take, CancellationToken cancellationToken = default);
    Task<List<MessageSyncItem>> ReadSyncItemsAsync(Guid sessionUnitId, List<long> ids, CancellationToken cancellationToken = default);
}
