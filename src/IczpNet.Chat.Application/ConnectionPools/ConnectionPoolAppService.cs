using IczpNet.AbpCommons.Dtos;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ConnectionPools.Dtos;
using IczpNet.Chat.Permissions;
using Minio.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.ConnectionPools;

/// <summary>
/// 连接池
/// </summary>
/// <param name="abortService"></param>
/// <param name="connectionPoolManager"></param>
public class ConnectionPoolAppService(
    IAbortService abortService,
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
    public IAbortService AbortService { get; } = abortService;
    public IConnectionPoolManager ConnectionPoolManager { get; } = connectionPoolManager;


    /// <inheritdoc />
    public async Task<int> GetTotalCountAsync(string host)
    {
        return await ConnectionPoolManager.GetTotalCountAsync(host);
    }

    protected virtual async Task<PagedResultDto<ConnectionPoolDto>> QueryPagedListAsync(IQueryable<ConnectionPoolCacheItem> connectionPools, ConnectionPoolGetListInput input)
    {
        var query = connectionPools.Where(x => x != null)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Host), x => x.Host == input.Host)
            .WhereIf(!string.IsNullOrWhiteSpace(input.ConnectionId), x => x.ConnectionId == input.ConnectionId)
            .WhereIf(!string.IsNullOrWhiteSpace(input.ClientId), x => x.ClientId == input.ClientId)
            .WhereIf(input.UserId.HasValue, x => x.UserId == input.UserId)
            .WhereIf(input.ChatObjectId.HasValue, x => x.ChatObjectIdList != null && x.ChatObjectIdList.Contains(input.ChatObjectId.Value))
            .WhereIf(input.ChatObjectIdList.IsAny(), x => x.ChatObjectIdList != null && x.ChatObjectIdList.Any(d => input.ChatObjectIdList.Contains(d)))
            //ActiveTime
            .WhereIf(input.StartActiveTime.HasValue, x => x.ActiveTime >= input.StartActiveTime)
            .WhereIf(input.EndActiveTime.HasValue, x => x.ActiveTime < input.EndActiveTime)
            //CreationTime
            .WhereIf(input.StartCreationTime.HasValue, x => x.CreationTime >= input.StartCreationTime)
            .WhereIf(input.EndCreationTime.HasValue, x => x.CreationTime < input.EndCreationTime)
            ;

        return await GetPagedListAsync<ConnectionPoolCacheItem, ConnectionPoolDto>(query, input, q => q.OrderByDescending(x => x.CreationTime));
    }

    protected virtual async Task<PagedResultDto<ConnectionPoolDto>> FetchPagedListAsync(ConnectionPoolGetListInput input)
    {
        var queryable = (await ConnectionPoolManager.CreateQueryableAsync());
        return await QueryPagedListAsync(queryable, input);
    }

    /// <summary>
    /// 获取在线人数列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedResultDto<ConnectionPoolDto>> GetListAsync(ConnectionPoolGetListInput input)
    {
        await CheckGetListPolicyAsync();
        return await FetchPagedListAsync(input);
    }

    /// <summary>
    /// 获取设备类型列表
    /// </summary>
    /// <param name="chatObjectId"></param>
    /// <returns></returns>
    public async Task<PagedResultDto<ConnectionPoolDto>> GetListByChatObjectAsync(long chatObjectId)
    {
        //await CheckGetItemPolicyAsync();
        var queryable = (await ConnectionPoolManager.GetListByChatObjectAsync(chatObjectId)).AsQueryable();

        return await QueryPagedListAsync(queryable, new ConnectionPoolGetListInput() { MaxResultCount = 999 });
    }

    /// <summary>
    /// 获取在线人数列表(用户)
    /// </summary>
    /// <returns></returns>
    public async Task<PagedResultDto<ConnectionPoolDto>> GetListByUserAsync(ConnectionPoolGetListInput input)
    {
        await CheckPolicyAsync(GetListByCurrentUserPolicyName);

        var userConnList = await ConnectionPoolManager.GetListByUserAsync(input.UserId.Value);

        var queryable = (userConnList.AsQueryable());

        return await QueryPagedListAsync(queryable, input);
    }

    /// <summary>
    /// 获取在线人数列表(当前用户)
    /// </summary>
    /// <returns></returns>
    public async Task<PagedResultDto<ConnectionPoolDto>> GetListByCurrentUserAsync(ConnectionPoolGetListInput input)
    {
        await CheckPolicyAsync(GetListByCurrentUserPolicyName);
        input.UserId = CurrentUser.Id;
        return await GetListByUserAsync(input);
    }

    /// <summary>
    /// 获取连接
    /// </summary>
    /// <param name="id">ConnectionId</param>
    /// <returns></returns>
    public async Task<ConnectionPoolDto> GetAsync(string id)
    {
        await CheckGetItemPolicyAsync();

        var item = await ConnectionPoolManager.GetAsync(id);

        return ObjectMapper.Map<ConnectionPoolCacheItem, ConnectionPoolDto>(item);
    }


    /// <summary>
    /// 清空所有连接
    /// </summary>
    /// <param name="host"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public async Task ClearAllAsync(string host, string reason)
    {
        await CheckPolicyAsync(ClearAllPolicyName);
        await ConnectionPoolManager.ClearAllAsync(host, reason);
    }

    /// <summary>
    /// 移除连接
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    public async Task RemoveAsync(string connectionId)
    {
        await CheckDeletePolicyAsync();
        await ConnectionPoolManager.DisconnectedAsync(connectionId);
    }

    /// <summary>
    /// 更新连接数量
    /// </summary>
    /// <returns></returns>
    public async Task<int> UpdateConnectionIdsAsync()
    {
        await CheckPolicyAsync(UpdateConnectionIdsPolicyName);
        return await ConnectionPoolManager.UpdateAllConnectionIdsAsync();
    }

    /// <summary>
    /// 获取连接数量(用户)
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<int> GetCountByUserAsync(Guid userId)
    {
        await CheckPolicyAsync(GetCountByUserIdPolicyName);
        return await ConnectionPoolManager.GetCountByUserAsync(userId);
    }
    public async Task<int> GetCountByChatObjectAsync(long chatObjectId)
    {
        await CheckPolicyAsync(GetCountByUserIdPolicyName);
        return await ConnectionPoolManager.GetCountByChatObjectAsync(chatObjectId);
    }

    /// <summary>
    /// 更新用户连接数量
    /// </summary>
    /// <returns></returns>
    public async Task<int> UpdateUserAsync(Guid userId)
    {
        await CheckPolicyAsync(UpdateUserConnectionIdsPolicyName);
        return await ConnectionPoolManager.UpdateIndexByUserAsync(userId);
    }

    /// <summary>
    /// 更新聊天对象连接数量
    /// </summary>
    /// <returns></returns>
    public async Task<int> UpdateChatObjectAsync(long chatObjectId)
    {
        await CheckPolicyAsync(UpdateUserConnectionIdsPolicyName);
        return await ConnectionPoolManager.UpdateIndexByChatObjectAsync(chatObjectId);
    }

    /// <summary>
    /// 强制断开连接
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task AbortAsync(AbortInput input)
    {
        await AbortService.AbortAsync(input.ConnectionIdList, input.Reason);
    }


}
