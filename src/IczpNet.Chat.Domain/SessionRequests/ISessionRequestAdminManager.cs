using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionRequests;

public class SessionRequestAdminListResult
{
    public long TotalCount { get; set; }

    public List<SessionRequest> Items { get; set; } = new();
}

public interface ISessionRequestAdminManager
{
    Task<SessionRequestAdminListResult> GetAdminListAsync(
        string keyword,
        long? ownerId,
        long? destinationId,
        bool? isHandled,
        bool? isAgreed,
        bool? isExpired,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default);

    Task<SessionRequest> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SessionRequest> HandleAdminAsync(
        Guid id,
        bool isAgreed,
        string handleMessage,
        CancellationToken cancellationToken = default);
}
