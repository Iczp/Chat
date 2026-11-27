using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.ChatObjects;

public interface IChatObjectRepository : IRepository<ChatObject, long>
{
    Task<ChatObjectAdminListResult> GetAdminPagedListAsync(
        ChatObjectAdminQueryFilter filter,
        List<Guid> resolvedCategoryIds,
        CancellationToken cancellationToken = default);

    Task<ChatObject> FindAdminDetailAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<int> GetCountByAppUserIdAsync(
        Guid appUserId,
        CancellationToken cancellationToken = default);
}
