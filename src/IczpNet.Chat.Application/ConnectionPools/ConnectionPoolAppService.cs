using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ConnectionPools.Dtos;
using IczpNet.Chat.Permissions;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ConnectionPools;

public class ConnectionPoolAppService(
    IConnectionPoolManager connectionPoolManager) : ChatAppService, IConnectionPoolAppService
{

    protected override string CreatePolicyName { get; set; } = ChatPermissions.ConnectionPoolPermission.Create;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.ConnectionPoolPermission.GetList;
    protected override string GetPolicyName { get; set; } = ChatPermissions.ConnectionPoolPermission.GetItem;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.ConnectionPoolPermission.Delete;
    protected virtual string ClearAllPolicyName { get; set; } = ChatPermissions.ConnectionPoolPermission.ClearAll;

    public IConnectionPoolManager ConnectionPoolManager { get; } = connectionPoolManager;

    /// <inheritdoc />
    public async Task<int> GetTotalCountAsync(string host)
    {
        return await ConnectionPoolManager.GetTotalCountAsync(host);
    }

    /// <inheritdoc />
    //[HttpGet]
    public async Task<PagedResultDto<ConnectionPoolCacheItem>> GetListAsync(ConnectionPoolGetListInput input)
    {
        await CheckGetListPolicyAsync();

        var query = (await ConnectionPoolManager.CreateQueryableAsync())
            .WhereIf(!string.IsNullOrWhiteSpace(input.Host), x => x.Host == input.Host)
            .WhereIf(!string.IsNullOrWhiteSpace(input.ConnectionId), x => x.ConnectionId == input.ConnectionId)
            .WhereIf(!string.IsNullOrWhiteSpace(input.ClientId), x => x.ClientId == input.ClientId)
            .WhereIf(input.AppUserId.HasValue, x => x.AppUserId == input.AppUserId)
            .WhereIf(input.ChatObjectId.HasValue, x => x.ChatObjectIdList.Contains(input.ChatObjectId.Value))
            ;
        ;

        return await GetPagedListAsync(query, input, q => q.OrderByDescending(x => x.CreationTime));
    }

    /// <summary>
    /// 获取连接
    /// </summary>
    /// <param name="id">ConnectionId</param>
    /// <returns></returns>
    public async Task<ConnectionPoolCacheItem> GetAsync(string id)
    {
        await CheckGetItemPolicyAsync();

        return await ConnectionPoolManager.GetAsync(id);
    }

    /// <inheritdoc />
    public async Task ClearAllAsync(string host)
    {
        await CheckPolicyAsync(ClearAllPolicyName);
        await ConnectionPoolManager.ClearAllAsync(host);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string connectionId)
    {
        await CheckDeletePolicyAsync();
        await ConnectionPoolManager.RemoveAsync(connectionId);
    }

    /// <inheritdoc />
    public async Task<int> UpdateConnectionIdsAsync()
    {
        return await ConnectionPoolManager.UpdateConnectionIdsAsync();
    }
}
