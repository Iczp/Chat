using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ConnectionPools.Dtos;
using IczpNet.Chat.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ConnectionPools;

/// <summary>
/// 连接池
/// </summary>
/// <param name="connectionPoolManager"></param>
public class ConnectionPoolAppService(
    IConnectionPoolManager connectionPoolManager) : ChatAppService, IConnectionPoolAppService
{

    protected override string CreatePolicyName { get; set; } = ChatPermissions.ConnectionPoolPermission.Create;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.ConnectionPoolPermission.GetList;
    protected virtual string GetListByChatObjectPolicyName { get; set; } = ChatPermissions.ConnectionPoolPermission.GetListByChatObject;
    protected virtual string GetListByCurrentUserPolicyName { get; set; } = ChatPermissions.ConnectionPoolPermission.GetListByCurrentUser;
    protected override string GetPolicyName { get; set; } = ChatPermissions.ConnectionPoolPermission.GetItem;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.ConnectionPoolPermission.Delete;
    protected virtual string ClearAllPolicyName { get; set; } = ChatPermissions.ConnectionPoolPermission.ClearAll;
    protected virtual string UpdateConnectionIdsPolicyName { get; set; } = ChatPermissions.ConnectionPoolPermission.UpdateConnectionIds;
    protected virtual string GetConnectionIdsByUserIdPolicyName { get; set; } = ChatPermissions.ConnectionPoolPermission.GetConnectionIdsByUserId;
    protected virtual string UpdateUserConnectionIdsPolicyName { get; set; } = ChatPermissions.ConnectionPoolPermission.UpdateUserConnectionIds;
    protected virtual string GetCountByUserIdPolicyName { get; set; } = ChatPermissions.ConnectionPoolPermission.GetCountByUserId;


    public IConnectionPoolManager ConnectionPoolManager { get; } = connectionPoolManager;


    /// <inheritdoc />
    public async Task<int> GetTotalCountAsync(string host)
    {
        return await ConnectionPoolManager.GetTotalCountAsync(host);
    }

    protected virtual async Task<PagedResultDto<ConnectionPoolCacheItem>> FetchListAsync(ConnectionPoolGetListInput input)
    {

        var query = (await ConnectionPoolManager.CreateQueryableAsync())
            .WhereIf(!string.IsNullOrWhiteSpace(input.Host), x => x.Host == input.Host)
            .WhereIf(!string.IsNullOrWhiteSpace(input.ConnectionId), x => x.ConnectionId == input.ConnectionId)
            .WhereIf(!string.IsNullOrWhiteSpace(input.ClientId), x => x.ClientId == input.ClientId)
            .WhereIf(input.UserId.HasValue, x => x.UserId == input.UserId)
            .WhereIf(input.ChatObjectId.HasValue, x => x.ChatObjectIdList.Contains(input.ChatObjectId.Value))
            .WhereIf(input.ChatObjectIdList.IsAny(), x => x.ChatObjectIdList.Any(d => input.ChatObjectIdList.Contains(d)))
            //ActiveTime
            .WhereIf(input.StartActiveTime.HasValue, x => x.ActiveTime >= input.StartActiveTime)
            .WhereIf(input.EndActiveTime.HasValue, x => x.ActiveTime < input.EndActiveTime)
            //CreationTime
            .WhereIf(input.StartCreationTime.HasValue, x => x.CreationTime >= input.StartCreationTime)
            .WhereIf(input.EndCreationTime.HasValue, x => x.CreationTime < input.EndCreationTime)
            ;

        return await GetPagedListAsync(query, input, q => q.OrderByDescending(x => x.CreationTime));
    }

    /// <summary>
    /// 获取在线人数列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedResultDto<ConnectionPoolCacheItem>> GetListAsync(ConnectionPoolGetListInput input)
    {
        await CheckGetListPolicyAsync();
        return await FetchListAsync(input);
    }

    /// <summary>
    /// 获取在线人数列表
    /// </summary>
    /// <returns></returns>
    public async Task<PagedResultDto<ConnectionPoolCacheItem>> GetListByChatObjectAsync(List<long> chatObjectIdList)
    {
        await CheckPolicyAsync(GetListByChatObjectPolicyName);
        return await FetchListAsync(new ConnectionPoolGetListInput()
        {
            ChatObjectIdList = chatObjectIdList
        });
    }
    /// <summary>
    /// 获取在线人数列表(当前用户)
    /// </summary>
    /// <returns></returns>
    public async Task<PagedResultDto<ConnectionPoolCacheItem>> GetListByCurrentUserAsync()
    {
        await CheckPolicyAsync(GetListByCurrentUserPolicyName);
        return await FetchListAsync(new ConnectionPoolGetListInput()
        {
            UserId = CurrentUser.Id
        });
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

    /// <summary>
    /// 清空所有连接
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public async Task ClearAllAsync(string host)
    {
        await CheckPolicyAsync(ClearAllPolicyName);
        await ConnectionPoolManager.ClearAllAsync(host);
    }

    /// <summary>
    /// 移除连接
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    public async Task RemoveAsync(string connectionId)
    {
        await CheckDeletePolicyAsync();
        await ConnectionPoolManager.RemoveAsync(connectionId);
    }

    /// <summary>
    /// 更新连接数量
    /// </summary>
    /// <returns></returns>
    public async Task<int> UpdateConnectionIdsAsync()
    {
        await CheckPolicyAsync(UpdateConnectionIdsPolicyName);
        return await ConnectionPoolManager.UpdateConnectionIdsAsync();
    }

    /// <summary>
    /// 获取用户连接
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<string>> GetConnectionIdsByUserIdAsync(Guid userId)
    {
        await CheckPolicyAsync(GetConnectionIdsByUserIdPolicyName);
        return await ConnectionPoolManager.GetConnectionIdsByUserIdAsync(userId);
    }

    /// <summary>
    /// 获取用户连接
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<int> GetCountByUserIdAsync(Guid userId)
    {
        await CheckPolicyAsync(GetCountByUserIdPolicyName);
        return (await ConnectionPoolManager.GetConnectionIdsByUserIdAsync(userId)).Count;
    }

    /// <summary>
    /// 更新用户连接数量
    /// </summary>
    /// <returns></returns>
    public async Task<int> UpdateUserConnectionIdsAsync(Guid userId)
    {
        await CheckPolicyAsync(UpdateUserConnectionIdsPolicyName);
        return await ConnectionPoolManager.UpdateUserConnectionIdsAsync(userId);
    }


}
