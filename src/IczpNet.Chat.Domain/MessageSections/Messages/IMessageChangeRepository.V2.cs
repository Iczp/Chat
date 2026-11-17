using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages;

public partial interface IMessageChangeRepository
{
    Task<List<MessageChangeUnitScope>> GetV2ScopesAsync(long ownerId, List<Guid> sessionUnitIds,
        CancellationToken cancellationToken = default);

    Task<List<MessageChangeV2Entry>> ReadV2ChangesAsync(MessageChangeUnitScope scope,
        long after, long through, int take, CancellationToken cancellationToken = default);

    Task<List<MessageChangeV2State>> ReadV2ItemsAsync(MessageChangeUnitScope scope,
        List<long> ids, CancellationToken cancellationToken = default);

    Task<List<long>> ReadV2SoftDeletedIdsAsync(MessageChangeUnitScope scope, List<long> ids,
        CancellationToken cancellationToken = default);

    Task<List<Message>> ReadV2WindowAsync(MessageChangeUnitScope scope,
        long? minId, long? maxId, int take, bool descending, CancellationToken cancellationToken = default);
}

public record MessageChangeUnitScope(Guid UnitId, Guid SessionId, long OwnerId, DateTime? ClearTime);

public record MessageChangeV2Entry(long Version, long? MessageId, MessageChangeKind Kind, DateTime? Time);

public record MessageChangeV2State(Message Message, bool IsDeleted, long? DeleteChangeSeq);
