using IczpNet.AbpCommons.Dtos;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ConnectionPools.Dtos;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionUnits;
using Minio.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ConnectionPools;

/// <summary>
/// 连接池(Cache)
/// </summary>

public class ConnectionCacheAppService(
    IAbortService abortService,
    IConnectionCacheManager connectionCacheManager) : ChatAppService, IConnectionCacheAppService
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
    public IConnectionCacheManager ConnectionPoolManager { get; } = connectionCacheManager;


    /// <summary>
    /// 获取所有主机在线人数
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedResultDto<OnlineHostDto>> GetHostsAsync(ConnectionHostGetListInput input)
    {
        var allHost = await ConnectionPoolManager.GetAllHostsAsync();

        var countMap = await ConnectionPoolManager.OnlineCountByHostAsync(allHost.Select(x => x.Key));

        var queryable = allHost.Select(x => new OnlineHostDto()
        {
            Host = x.Key,
            StartTime = x.Value,
            Count = countMap.GetValueOrDefault(x.Key),
        }).AsQueryable();

        var query = queryable.WhereIf(!string.IsNullOrWhiteSpace(input.Host), x => x.Host == input.Host);

        return await GetPagedListAsync(query, input, q => q.OrderByDescending(x => x.StartTime));
    }

    /// <summary>
    /// 获取所有主机在线人数
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public async Task<long> GetTotalCountAsync(string host)
    {
        var result = await GetHostsAsync(new ConnectionHostGetListInput()
        {
            MaxResultCount = 999
        });
        return result.Items.Sum(x => x.Count);
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    /// <summary>
    /// 获取在线人数列表(用户)
    /// </summary>
    /// <returns></returns>
    public async Task<PagedResultDto<ConnectionPoolDto>> GetListByUserAsync(ConnectionPoolGetListInput input)
    {
        await CheckPolicyAsync(GetListByCurrentUserPolicyName);

        throw new NotImplementedException();
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

        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    /// <summary>
    /// 获取连接数量(用户)
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<int> GetCountByUserAsync(Guid userId)
    {
        await CheckPolicyAsync(GetCountByUserIdPolicyName);
        throw new NotImplementedException();
    }
    public async Task<int> GetCountByChatObjectAsync(long chatObjectId)
    {
        await CheckPolicyAsync(GetCountByUserIdPolicyName);
        throw new NotImplementedException();
    }

    /// <summary>
    /// 更新用户连接数量
    /// </summary>
    /// <returns></returns>
    public async Task<int> UpdateUserAsync(Guid userId)
    {
        await CheckPolicyAsync(UpdateUserConnectionIdsPolicyName);
        throw new NotImplementedException();
    }

    /// <summary>
    /// 更新聊天对象连接数量
    /// </summary>
    /// <returns></returns>
    public async Task<int> UpdateChatObjectAsync(long chatObjectId)
    {
        await CheckPolicyAsync(UpdateUserConnectionIdsPolicyName);
        throw new NotImplementedException();
    }

    public async Task<PagedResultDto<OwnerLatestOnline>> GetLatestOnlineAsync(long ownerId, LatestOnlineGetListInput input)
    {
        var list = await ConnectionPoolManager.GetLatestOnlineAsync(ownerId);
        var queryable = list.AsQueryable();
        var query = queryable
            .WhereIf(!string.IsNullOrWhiteSpace(input.DeviceId), x => x.DeviceId == input.DeviceId)
            .WhereIf(!string.IsNullOrWhiteSpace(input.DeviceType), x => x.DeviceType == input.DeviceType)
            ;
        return await GetPagedListAsync(query, input);
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

    public Task<long> GetOnlineFriendsCountAsync(long ownerId)
    {
        return ConnectionPoolManager.GetOnlineFriendsCountAsync(ownerId);
    }

    public async Task<PagedResultDto<SessionUnitElement>> GetOnlineFriendsAsync(OnlineFriendsGetListInput input)
    {
        var list = await ConnectionPoolManager.GetOnlineFriendsAsync(input.OwnerId);

        var queryable = list.AsQueryable();
        var query = queryable
            .WhereIf(input.FriendId.HasValue, x => x.FriendId == input.FriendId)
            .WhereIf(input.SessionId.HasValue, x => x.SessionId == input.SessionId)
            .WhereIf(input.SessionUnitId.HasValue, x => x.SessionUnitId == input.SessionUnitId)
            ;
        return await GetPagedListAsync(query, input);
    }
}
