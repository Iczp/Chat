using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ConnectionPools.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ConnectionPools;

public class ConnectionPoolAppService(
    IConnectionPoolManager connectionPoolManager) : ChatAppService, IConnectionPoolAppService
{
    public IConnectionPoolManager ConnectionPoolManager { get; } = connectionPoolManager;

    /// <inheritdoc />
    public Task<int> GetTotalCountAsync(string host)
    {
        return ConnectionPoolManager.CountAsync(host);
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<ConnectionPoolCacheItem>> GetListAsync(ConnectionPoolGetListInput input)
    {
        var query = (await ConnectionPoolManager.GetAllAsync())
            .AsQueryable()
            .WhereIf(!string.IsNullOrWhiteSpace(input.Host), x => x.Host == input.Host)
            .WhereIf(!string.IsNullOrWhiteSpace(input.ConnectionId), x => x.ConnectionId == input.ConnectionId)
            .WhereIf(!string.IsNullOrWhiteSpace(input.QueryId), x => x.QueryId == input.QueryId)
            .WhereIf(input.AppUserId.HasValue, x => x.AppUserId == input.AppUserId)
            .WhereIf(input.ChatObjectId.HasValue, x => x.ChatObjectIdList.Contains(input.ChatObjectId.Value))
            ;
        ;

        return await GetPagedListAsync(query, input, q => q.OrderByDescending(x => x.CreationTime));
    }

    /// <inheritdoc />
    public Task ClearAllAsync(string host)
    {
        return ConnectionPoolManager.ClearAllAsync(host);
    }

    /// <inheritdoc />
    public Task RemoveAsync(string connectionId)
    {
        return ConnectionPoolManager.RemoveAsync(connectionId);
    }
}
