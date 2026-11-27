using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitAdminListResult
{
    public long TotalCount { get; set; }

    public List<SessionUnit> Items { get; set; } = new();
}

public interface ISessionUnitAdminManager
{
    Task<SessionUnitAdminListResult> GetAdminListAsync(
        long? ownerId,
        long? destinationId,
        ChatObjectTypeEnums? destinationObjectType,
        bool? isKilled,
        string keyword,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default);

    Task<SessionUnit> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Guid> FindIdAdminAsync(
        long ownerId,
        long destinationId,
        CancellationToken cancellationToken = default);
}
