using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.Online.Dtos;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.ConnectionPools;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.Online;

/// <summary>
/// 在线连接池管理服务
/// </summary>
[Authorize(ChatAdminPermissions.Online.Default)]
[ExposeServices(typeof(OnlineAppService), typeof(IOnlineAdminAppService))]
public class OnlineAppService(
    IOnlineManager onlineManager,
    IAbortService abortService) : ApplicationService, IOnlineAdminAppService
{
    protected IOnlineManager OnlineManager { get; } = onlineManager;
    protected IAbortService AbortService { get; } = abortService;

    [Authorize(ChatAdminPermissions.Online.GetList)]
    public virtual async Task<PagedResultDto<OnlineAdminConnectionDto>> GetListAsync(OnlineAdminGetListInput input)
    {
        input ??= new OnlineAdminGetListInput();

        var connIds = await OnlineManager.GetConnectionIdsByHostAsync(input.Host);
        var distinctIds = connIds.Distinct().ToList();
        if (distinctIds.Count == 0)
        {
            return new PagedResultDto<OnlineAdminConnectionDto>();
        }

        var rawItems = await OnlineManager.GetManyAsync(distinctIds);
        var query = rawItems
            .Select(x => x.Value)
            .Where(x => x != null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var kw = input.Keyword.Trim();
            query = query.Where(x =>
                (x.ConnectionId != null && x.ConnectionId.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                (x.UserName != null && x.UserName.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                (x.IpAddress != null && x.IpAddress.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                (x.DeviceId != null && x.DeviceId.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                (x.ClientId != null && x.ClientId.Contains(kw, StringComparison.OrdinalIgnoreCase)));
        }
        if (!string.IsNullOrWhiteSpace(input.Host))
        {
            query = query.Where(x => x.Host == input.Host.Trim());
        }
        if (!string.IsNullOrWhiteSpace(input.ConnectionId))
        {
            query = query.Where(x => x.ConnectionId == input.ConnectionId.Trim());
        }
        if (!string.IsNullOrWhiteSpace(input.ClientId))
        {
            query = query.Where(x => x.ClientId == input.ClientId.Trim());
        }
        if (!string.IsNullOrWhiteSpace(input.Platform))
        {
            query = query.Where(x => x.Platform == input.Platform.Trim());
        }
        if (input.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == input.UserId.Value);
        }
        if (input.ChatObjectId.HasValue)
        {
            query = query.Where(x => x.ChatObjectIdList != null && x.ChatObjectIdList.Contains(input.ChatObjectId.Value));
        }
        if (input.StartActiveTime.HasValue)
        {
            query = query.Where(x => x.ActiveTime >= input.StartActiveTime.Value);
        }
        if (input.EndActiveTime.HasValue)
        {
            query = query.Where(x => x.ActiveTime < input.EndActiveTime.Value);
        }
        if (input.StartCreationTime.HasValue)
        {
            query = query.Where(x => x.CreationTime >= input.StartCreationTime.Value);
        }
        if (input.EndCreationTime.HasValue)
        {
            query = query.Where(x => x.CreationTime < input.EndCreationTime.Value);
        }

        var ordered = query.OrderByDescending(x => x.CreationTime).ToList();
        var paged = ordered
            .Skip(Math.Max(0, input.SkipCount))
            .Take(Math.Clamp(input.MaxResultCount, 1, 200))
            .Select(MapToConnectionDto)
            .ToList();

        return new PagedResultDto<OnlineAdminConnectionDto>(ordered.Count, paged);
    }

    [Authorize(ChatAdminPermissions.Online.GetDetail)]
    public virtual async Task<OnlineAdminConnectionDto> GetAsync(string id)
    {
        var item = await OnlineManager.GetAsync(id);
        return item == null ? null : MapToConnectionDto(item);
    }

    [Authorize(ChatAdminPermissions.Online.GetList)]
    public virtual async Task<long> GetTotalCountAsync(string host = null)
    {
        if (!string.IsNullOrWhiteSpace(host))
        {
            return await OnlineManager.GetCountByHostAsync(host.Trim());
        }

        var allHosts = await OnlineManager.GetAllHostsAsync();
        var countMap = await OnlineManager.GetCountByHostsAsync(allHosts.Select(x => x.Key));
        return countMap.Values.Sum();
    }

    [Authorize(ChatAdminPermissions.Online.GetList)]
    public virtual async Task<PagedResultDto<OnlineAdminHostDto>> GetHostsAsync(OnlineAdminHostGetListInput input)
    {
        input ??= new OnlineAdminHostGetListInput();

        var allHost = await OnlineManager.GetAllHostsAsync();
        var countMap = await OnlineManager.GetCountByHostsAsync(allHost.Select(x => x.Key));

        var list = allHost
            .Where(x => string.IsNullOrWhiteSpace(input.Host) || x.Key == input.Host.Trim())
            .Select(x => new OnlineAdminHostDto
            {
                Host = x.Key,
                StartTime = x.Value,
                Count = countMap.GetValueOrDefault(x.Key)
            })
            .OrderByDescending(x => x.StartTime)
            .ToList();

        var paged = list
            .Skip(Math.Max(0, input.SkipCount))
            .Take(Math.Clamp(input.MaxResultCount, 1, 200))
            .ToList();

        return new PagedResultDto<OnlineAdminHostDto>(list.Count, paged);
    }

    [Authorize(ChatAdminPermissions.Online.GetList)]
    public virtual async Task<PagedResultDto<OnlineAdminConnectionDto>> GetByUserAsync(
        Guid userId,
        PagedAndSortedResultRequestDto input)
    {
        input ??= new PagedAndSortedResultRequestDto();
        var connIds = (await OnlineManager.GetConnectionIdsByUserAsync(userId)).Distinct().ToList();
        return await GetPagedConnectionsAsync(connIds, input);
    }

    [Authorize(ChatAdminPermissions.Online.GetList)]
    public virtual async Task<PagedResultDto<OnlineAdminConnectionDto>> GetByOwnerAsync(
        long ownerId,
        PagedAndSortedResultRequestDto input)
    {
        input ??= new PagedAndSortedResultRequestDto();
        var dict = await OnlineManager.GetDevicesAsync([ownerId]);
        var connIds = dict.Values.SelectMany(x => x.Select(v => v.ConnectionId)).Distinct().ToList();
        return await GetPagedConnectionsAsync(connIds, input);
    }

    [Authorize(ChatAdminPermissions.Online.GetList)]
    public virtual Task<long> GetCountByUserAsync(Guid userId)
    {
        return OnlineManager.GetCountByUserAsync(userId);
    }

    [Authorize(ChatAdminPermissions.Online.GetList)]
    public virtual Task<long> GetCountByOwnerAsync(long ownerId)
    {
        return OnlineManager.GetCountByOwnerAsync(ownerId);
    }

    [Authorize(ChatAdminPermissions.Online.GetList)]
    public virtual Task<long> GetCountBySessionAsync(Guid sessionId)
    {
        return OnlineManager.GetCountBySessionAsync(sessionId);
    }

    [Authorize(ChatAdminPermissions.Online.GetList)]
    public virtual async Task<PagedResultDto<OnlineAdminLastOnlineDto>> GetLastOnlineAsync(
        long ownerId,
        OnlineAdminLastOnlineGetListInput input)
    {
        input ??= new OnlineAdminLastOnlineGetListInput();
        var list = await OnlineManager.GetLastOnlineAsync(ownerId);
        var filtered = list
            .Where(x => string.IsNullOrWhiteSpace(input.DeviceId) || x.DeviceId == input.DeviceId.Trim())
            .Where(x => string.IsNullOrWhiteSpace(input.DeviceType) || x.DeviceType == input.DeviceType.Trim())
            .Select(x => new OnlineAdminLastOnlineDto
            {
                OwnerId = x.OwnerId,
                DeviceId = x.DeviceId,
                DeviceType = x.DeviceType,
                ActiveTime = x.LastTime
            })
            .ToList();

        var paged = filtered
            .Skip(Math.Max(0, input.SkipCount))
            .Take(Math.Clamp(input.MaxResultCount, 1, 200))
            .ToList();

        return new PagedResultDto<OnlineAdminLastOnlineDto>(filtered.Count, paged);
    }

    [Authorize(ChatAdminPermissions.Online.GetList)]
    public virtual Task<long> GetOnlineFriendsCountAsync(long ownerId)
    {
        return OnlineManager.GetOnlineFriendsCountAsync(ownerId);
    }

    [Authorize(ChatAdminPermissions.Online.Abort)]
    public virtual async Task AbortAsync(OnlineAdminAbortInput input)
    {
        if (input?.ConnectionIdList == null || input.ConnectionIdList.Count is < 1 or > 100)
        {
            throw new BusinessException("ChatAdmin:InvalidConnectionList", "连接 ID 列表长度须在 1 到 100 之间。");
        }

        await AbortService.AbortAsync(input.ConnectionIdList, input.Reason);
    }

    [Authorize(ChatAdminPermissions.Online.Delete)]
    public virtual Task DeleteAsync(string connectionId)
    {
        return OnlineManager.DisconnectedAsync(connectionId);
    }

    [Authorize(ChatAdminPermissions.Online.ClearAll)]
    public virtual async Task<Dictionary<string, long>> ClearAllAsync(
        List<string> hosts = null,
        string reason = null)
    {
        var hostList = hosts != null && hosts.Count > 0
            ? hosts
            : (await OnlineManager.GetAllHostsAsync()).Select(x => x.Key).ToList();

        var result = new Dictionary<string, long>(hostList.Count);
        foreach (var item in hostList)
        {
            result[item] = await OnlineManager.DeleteByHostNameAsync(item);
        }

        return result;
    }

    private async Task<PagedResultDto<OnlineAdminConnectionDto>> GetPagedConnectionsAsync(
        List<string> connIds,
        PagedAndSortedResultRequestDto input)
    {
        if (connIds.Count == 0)
        {
            return new PagedResultDto<OnlineAdminConnectionDto>();
        }

        var rawItems = await OnlineManager.GetManyAsync(connIds);
        var list = rawItems
            .Select(x => x.Value)
            .Where(x => x != null)
            .OrderByDescending(x => x.CreationTime)
            .ToList();

        var paged = list
            .Skip(Math.Max(0, input.SkipCount))
            .Take(Math.Clamp(input.MaxResultCount, 1, 200))
            .Select(MapToConnectionDto)
            .ToList();

        return new PagedResultDto<OnlineAdminConnectionDto>(list.Count, paged);
    }

    private static OnlineAdminConnectionDto MapToConnectionDto(ConnectionPoolCacheItem item)
    {
        return new OnlineAdminConnectionDto
        {
            ConnectionId = item.ConnectionId,
            PushClientId = item.PushClientId,
            AppId = item.AppId,
            AppName = item.AppName,
            QueryId = item.QueryId,
            ClientId = item.ClientId,
            ClientName = item.ClientName,
            Host = item.Host,
            UserId = item.UserId,
            UserName = item.UserName,
            DeviceId = item.DeviceId,
            DeviceType = item.DeviceType,
            IpAddress = item.IpAddress,
            BrowserInfo = item.BrowserInfo,
            DeviceInfo = item.DeviceInfo,
            ActiveTime = item.ActiveTime,
            CreationTime = item.CreationTime,
            Browser = item.Browser,
            Brand = item.Brand,
            Model = item.Model,
            Platform = item.Platform,
            ChatObjectIdList = item.ChatObjectIdList ?? []
        };
    }
}
