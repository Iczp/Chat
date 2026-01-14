using IczpNet.AbpCommons.Dtos;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ConnectionPools.Dtos;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionUnits;
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
    protected virtual string GetListByUserPolicyName { get; set; } = ChatPermissions.ConnectionPoolPermission.GetConnectionIdsByUserId;
    protected virtual string GetCountByUserIdPolicyName { get; set; } = ChatPermissions.ConnectionPoolPermission.GetCountByUserId;
    public IAbortService AbortService { get; } = abortService;
    public IConnectionCacheManager ConnectionCacheManager { get; } = connectionCacheManager;

    private ConnectionPoolDto MapToDto(ConnectionPoolCacheItem item)
    {
        return ObjectMapper.Map<ConnectionPoolCacheItem, ConnectionPoolDto>(item);
    }

    /// <summary>
    /// 获取所有主机在线人数
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedResultDto<OnlineHostDto>> GetHostsAsync(ConnectionHostGetListInput input)
    {
        var allHost = await ConnectionCacheManager.GetAllHostsAsync();

        var countMap = await ConnectionCacheManager.GetCountByHostsAsync(allHost.Select(x => x.Key));

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
        var connIds = await ConnectionCacheManager.GetConnectionsByHostAsync(input.Host);
        var connList = await GetManyAsync(connIds.ToList());
        var query = connList.Values.AsQueryable();
        return await GetPagedListAsync(query, input);
    }


    protected async Task<PagedResultDto<ConnectionPoolDto>> GetPagedListByConnIdsAsync(List<string> connIds, GetListInput input)
    {
        var connList = await GetManyAsync(connIds);
        var query = connList.Values.AsQueryable();
        return await GetPagedListAsync(query, input);
    }

    /// <summary>
    /// 获取在线人数列表(聊天对象)
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedResultDto<ConnectionPoolDto>> GetListByOwnerAsync(long ownerId, GetListInput input)
    {
        await CheckPolicyAsync(GetListByChatObjectPolicyName);
        var dict = await ConnectionCacheManager.GetDevicesAsync([ownerId]);
        var connIds = dict.Values.SelectMany(x => x.Select(v => v.ConnectionId)).ToList();
        return await GetPagedListByConnIdsAsync(connIds, input);
    }

    /// <summary>
    /// 获取在线人数列表(用户)
    /// </summary>
    /// <returns></returns>
    public async Task<PagedResultDto<ConnectionPoolDto>> GetListByUserAsync(Guid userId, GetListInput input)
    {
        await CheckPolicyAsync(GetListByUserPolicyName);
        var connIds = await ConnectionCacheManager.GetConnectionsByUserAsync(userId);
        return await GetPagedListByConnIdsAsync(connIds.ToList(), input);
    }

    /// <summary>
    /// 获取在线人数列表(当前用户)
    /// </summary>
    /// <returns></returns>
    public async Task<PagedResultDto<ConnectionPoolDto>> GetListByCurrentUserAsync(GetListInput input)
    {
        await CheckPolicyAsync(GetListByCurrentUserPolicyName);
        return await GetListByUserAsync(CurrentUser.Id.Value, input);
    }

    /// <summary>
    /// 获取连接
    /// </summary>
    /// <param name="id">ConnectionId</param>
    /// <returns></returns>
    public async Task<ConnectionPoolDto> GetAsync(string id)
    {
        await CheckGetItemPolicyAsync();
        var item = await ConnectionCacheManager.GetAsync(id);
        return MapToDto(item);
    }

    /// <summary>
    /// 获取连接(多个)
    /// </summary>
    /// <param name="connectionIds"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, ConnectionPoolDto>> GetManyAsync(List<string> connectionIds)
    {
        await CheckGetItemPolicyAsync();
        var items = await ConnectionCacheManager.GetManyAsync(connectionIds);
        return items.ToDictionary(x => x.Key, x => MapToDto(x.Value));
    }

    /// <summary>
    /// 清空所有连接
    /// </summary>
    /// <param name="hosts"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, long>> ClearAllAsync(List<string> hosts, string reason)
    {
        await CheckPolicyAsync(ClearAllPolicyName);
        var hostList = hosts ?? (await ConnectionCacheManager.GetAllHostsAsync())
                .Select(x => x.Key).ToList();

        var result = new Dictionary<string, long>(hostList.Count);

        foreach (var item in hostList)
        {
            result[item] = await ConnectionCacheManager.DeleteByHostNameAsync(item);
        }
        return result;

    }

    /// <summary>
    /// 移除连接
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    public async Task RemoveAsync(string connectionId)
    {
        await CheckDeletePolicyAsync();
        await ConnectionCacheManager.DisconnectedAsync(connectionId);
    }

    /// <summary>
    /// 获取连接数量(用户)
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<long> GetCountByUserAsync(Guid userId)
    {
        await CheckPolicyAsync(GetCountByUserIdPolicyName);
        return await ConnectionCacheManager.GetCountByUserAsync(userId);
    }
    /// <summary>
    /// 获取连接数量(聊天对象)
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    public async Task<long> GetCountByOwnerAsync(long ownerId)
    {
        await CheckPolicyAsync(GetCountByUserIdPolicyName);
        return await ConnectionCacheManager.GetCountByOwnerAsync(ownerId);
    }

    /// <summary>
    /// 获取会话在线人数
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async Task<long> GetCountBySessionAsync(Guid sessionId)
    {
        await CheckPolicyAsync(GetPolicyName);
        return await ConnectionCacheManager.GetCountBySessionAsync(sessionId);
    }

    /// <summary>
    /// 获取最后在线时间
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedResultDto<OwnerLatestOnline>> GetLatestOnlineAsync(long ownerId, LatestOnlineGetListInput input)
    {
        var list = await ConnectionCacheManager.GetLatestOnlineAsync(ownerId);
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

    /// <summary>
    /// 获取在线好友数量
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    public Task<long> GetOnlineFriendsCountAsync(long ownerId)
    {
        return ConnectionCacheManager.GetOnlineFriendsCountAsync(ownerId);
    }

    /// <summary>
    /// 获取在线好友列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedResultDto<SessionUnitElement>> GetOnlineFriendsAsync(OnlineFriendsGetListInput input)
    {
        var list = await ConnectionCacheManager.GetOnlineFriendsAsync(input.OwnerId);

        var queryable = list.AsQueryable();
        var query = queryable
            .WhereIf(input.FriendId.HasValue, x => x.FriendId == input.FriendId)
            .WhereIf(input.SessionId.HasValue, x => x.SessionId == input.SessionId)
            .WhereIf(input.SessionUnitId.HasValue, x => x.SessionUnitId == input.SessionUnitId)
            ;
        return await GetPagedListAsync(query, input);
    }
}
