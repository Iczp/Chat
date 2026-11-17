using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages;

public partial interface IMessageChangeRepository
{
    Task<List<MessageChangeUnitScope>> GetV2ScopesAsync(long ownerId, List<Guid> sessionUnitIds,
        CancellationToken cancellationToken = default);

    Task<List<MessageChangeV2State>> ReadV2ItemsAsync(MessageChangeUnitScope scope,
        List<long> ids, CancellationToken cancellationToken = default);

    Task<List<long>> ReadV2SoftDeletedIdsAsync(MessageChangeUnitScope scope, List<long> ids,
        CancellationToken cancellationToken = default);

    Task<List<Message>> ReadV2WindowAsync(MessageChangeUnitScope scope,
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

public record MessageChangeUnitScope(Guid UnitId, Guid SessionId, long OwnerId, DateTime? ClearTime);

public record MessageChangeV2State(Message Message, bool IsDeleted, long? DeleteChangeSeq);

public record MessageChangePullRow(long MessageChangeId, Guid SessionId, Guid SessionUnitId, long MessageId,
    MessageChangeKind ChangeType, DateTime CreationTime);

public record MessageChangePullResult(long NextCursor, bool HasMore, List<MessageChangePullRow> Items);
