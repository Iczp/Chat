using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionRoles;

public class SessionRoleAdminListResult
{
    public long TotalCount { get; set; }

    public List<SessionRole> Items { get; set; } = new();
}

public interface ISessionRoleAdminManager
{
    Task<SessionRoleAdminListResult> GetAdminListAsync(
        Guid? sessionId,
        string keyword,
        bool? isDefault,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default);

    Task<SessionRole> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
