using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Connections.Dtos;
using IczpNet.Chat.Permissions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Connections;

/// <summary>
/// 在线人数管理
/// </summary>
public class ConnectionAppService(
    IRepository<Connection, string> repository,
    IConnectionManager connectionManager)
        : CrudChatAppService<
        Connection,
        ConnectionDetailDto,
        ConnectionDto,
        string,
        ConnectionGetListInput>(repository),
    IConnectionAppService
{
    protected override string GetPolicyName { get; set; } = ChatPermissions.ConnectionPermission.Default;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.ConnectionPermission.Default;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.ConnectionPermission.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.ConnectionPermission.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.ConnectionPermission.Delete;
    protected virtual string SetActivePolicyName { get; set; } = ChatPermissions.ConnectionPermission.SetActive;
    protected virtual string GetOnlineCountPolicyName { get; set; } = ChatPermissions.ConnectionPermission.GetOnlineCount;

    protected IConnectionManager ConnectionManager { get; } = connectionManager;

    protected override async Task<IQueryable<Connection>> CreateFilteredQueryAsync(ConnectionGetListInput input)
    {
        var config = await ConnectionManager.GetConfigAsync();
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(input.AppUserId.HasValue, x => x.AppUserId == input.AppUserId)
            .WhereIf(input.ChatObjectId.HasValue, x => x.ConnectionChatObjectList.Any(d => d.ChatObjectId == input.ChatObjectId))
            .WhereIf(input.StartCreationTime.HasValue, x => x.CreationTime >= input.StartCreationTime)
            .WhereIf(input.EndCreationTime.HasValue, x => x.CreationTime < input.EndCreationTime)
            .WhereIf(!input.DeviceId.IsEmpty(), x => x.DeviceId == input.DeviceId)
            .WhereIf(!input.ServerHostId.IsEmpty(), x => x.ServerHostId == input.ServerHostId)
            .WhereIf(!input.IpAddress.IsEmpty(), x => x.IpAddress == input.IpAddress)
            .WhereIf(input.IsOnline == true, x => x.ActiveTime > DateTime.Now.AddSeconds(-config.InactiveSeconds))
            ;
    }

    /// <summary>
    /// 设置为活跃的
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ticks"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<string> SetActiveAsync(string id, string ticks)
    {
        await CheckPolicyAsync(SetActivePolicyName);
        await ConnectionManager.UpdateActiveTimeAsync(id);
        return ticks;
    }

    /// <summary>
    /// 获取在线用户数量
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<GetOnlineCountOutput> GetOnlineCountAsync()
    {
        await CheckPolicyAsync(GetOnlineCountPolicyName);
        var currentTime = Clock.Now;
        var count = await ConnectionManager.GetOnlineCountAsync(currentTime);
        return new GetOnlineCountOutput { Count = count, CurrentTime = currentTime };
    }

    /// <summary>
    /// 获取配置信息
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ConnectionOptions> GetConfigAsync()
    {
        return await ConnectionManager.GetConfigAsync();
    }
}
