using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.Sessions;

public class SessionAdminListResult
{
    public long TotalCount { get; set; }

    public List<Session> Items { get; set; } = new();
}

public class SessionAdminScope
{
    public Guid? TenantId { get; set; }

    public bool HasManageAll { get; set; }

    public IReadOnlyCollection<Guid> AllowedSessionIds { get; set; } = Array.Empty<Guid>();
}

public interface ISessionAdminManager
{
    Task<SessionAdminListResult> GetAdminListAsync(
        string keyword,
        long? ownerId,
        Channels? channel,
        bool? isEnabled,
        bool? isAllMuted,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default);

    Task<Session> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Session> SetIsEnabledAdminAsync(
        Guid id,
        bool isEnabled,
        CancellationToken cancellationToken = default);

    Task<Session> SetIsAllMutedAdminAsync(
        Guid id,
        bool isAllMuted,
        SessionAdminScope scope = null,
        CancellationToken cancellationToken = default);
}
