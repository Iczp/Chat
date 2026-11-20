using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.CallCenters;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Linq;
using IczpNet.Chat.Enums;
using Volo.Abp;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Services;

/// <summary>
/// 呼叫中心
/// </summary>
public class CallCenterAppService(ICallCenterManager callCenterManager) : ChatAppService, ICallCenterAppService
{
    protected ICallCenterManager CallCenterManager { get; } = callCenterManager;

    private async Task CheckShopOwnerAsync(long shopKeeperId)
    {
        await CheckPolicyForUserAsync(shopKeeperId);
        var shop = await ChatObjectManager.GetAsync(shopKeeperId);
        if (shop.ObjectType != ChatObjectTypeEnums.ShopKeeper)
            throw new UserFriendlyException("聊天对象不是店长。");
    }

    private static CallCenterConfigurationDto MapConfiguration(CallCenterConfiguration configuration) => new()
    {
        ShopKeeperId = configuration.ShopKeeperId,
        DistributionStrategy = configuration.DistributionStrategy,
        OverflowStrategy = configuration.OverflowStrategy,
        MaxActiveConversations = configuration.MaxActiveConversations,
        ActiveTimeoutMinutes = configuration.ActiveTimeoutMinutes,
        RequireOnline = configuration.RequireOnline
    };

    [HttpGet]
    public async Task<CallCenterConfigurationDto> GetConfigurationAsync(long shopKeeperId)
    {
        await CheckShopOwnerAsync(shopKeeperId);
        var repository = LazyServiceProvider.LazyGetRequiredService<IRepository<CallCenterConfiguration, Guid>>();
        var config = await repository.FindAsync(x => x.ShopKeeperId == shopKeeperId);
        return MapConfiguration(config ?? new CallCenterConfiguration(Guid.Empty, shopKeeperId));
    }

    [HttpPost]
    public async Task<CallCenterConfigurationDto> UpdateConfigurationAsync(long shopKeeperId, UpdateCallCenterConfigurationInput input)
    {
        await CheckShopOwnerAsync(shopKeeperId);
        var distributedLock = LazyServiceProvider.LazyGetRequiredService<IAbpDistributedLock>();
        if (distributedLock is NullAbpDistributedLock)
            throw new UserFriendlyException("客服配置暂不可用，缺少分布式锁。");
        await using var handle = await distributedLock.TryAcquireAsync($"Chat:CallCenter:{shopKeeperId}", TimeSpan.FromSeconds(10));
        if (handle == null) throw new UserFriendlyException("客服配置正在更新，请重试。");
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var repository = LazyServiceProvider.LazyGetRequiredService<IRepository<CallCenterConfiguration, Guid>>();
        var config = await repository.FindAsync(x => x.ShopKeeperId == shopKeeperId);
        var isNew = config == null;
        config ??= new CallCenterConfiguration(GuidGenerator.Create(), shopKeeperId);
        config.Configure(input.DistributionStrategy, input.OverflowStrategy, input.MaxActiveConversations,
            input.ActiveTimeoutMinutes, input.RequireOnline);
        if (isNew) await repository.InsertAsync(config);
        else await repository.UpdateAsync(config);
        await uow.CompleteAsync();
        return MapConfiguration(config);
    }

    [HttpDelete]
    public async Task DeleteConfigurationAsync(long shopKeeperId)
    {
        await CheckShopOwnerAsync(shopKeeperId);
        var distributedLock = LazyServiceProvider.LazyGetRequiredService<IAbpDistributedLock>();
        if (distributedLock is NullAbpDistributedLock)
            throw new UserFriendlyException("客服配置暂不可用，缺少分布式锁。");
        await using var handle = await distributedLock.TryAcquireAsync($"Chat:CallCenter:{shopKeeperId}", TimeSpan.FromSeconds(10));
        if (handle == null) throw new UserFriendlyException("客服配置正在更新，请重试。");
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var repository = LazyServiceProvider.LazyGetRequiredService<IRepository<CallCenterConfiguration, Guid>>();
        var configuration = await repository.FindAsync(x => x.ShopKeeperId == shopKeeperId);
        if (configuration != null) await repository.DeleteAsync(configuration);
        await uow.CompleteAsync();
    }

    [HttpGet]
    public async Task<System.Collections.Generic.Dictionary<long, int>> GetActiveCountsAsync(long shopKeeperId)
    {
        await CheckShopOwnerAsync(shopKeeperId);
        var staff = await ChatObjectManager.GetChildsAsync(shopKeeperId);
        var counts = new System.Collections.Generic.Dictionary<long, int>();
        foreach (var batch in new[] { shopKeeperId }.Concat(staff.Where(x => x.ObjectType == ChatObjectTypeEnums.ShopWaiter)
            .Select(x => x.Id)).Distinct().Chunk(500))
            foreach (var (agentId, count) in await CallCenterManager.GetActiveCountsAsync(shopKeeperId, batch))
                counts[agentId] = count;
        return counts;
    }

    [HttpGet]
    public async Task<CallCenterAssignmentDto> GetAssignmentAsync(Guid sessionId)
    {
        var assignment = await CallCenterManager.GetAssignmentAsync(sessionId)
            ?? throw new UserFriendlyException("客服接待记录不存在。");
        var staff = await ChatObjectManager.GetChildsAsync(assignment.ShopKeeperId);
        await CheckPolicyForUserAsync(new[] { assignment.CustomerId, assignment.ShopKeeperId }
            .Concat(staff.Select(x => x.Id)));
        return new CallCenterAssignmentDto
        {
            SessionId = assignment.SessionId,
            ShopKeeperId = assignment.ShopKeeperId,
            CustomerId = assignment.CustomerId,
            AgentId = assignment.AgentId,
            Status = assignment.Status,
            QueuedAt = assignment.QueuedAt,
            LastActivityAt = assignment.LastActivityAt
        };
    }

    [HttpPost]
    public async Task<int> DrainQueueAsync(long shopKeeperId)
    {
        await CheckShopOwnerAsync(shopKeeperId);
        return await CallCenterManager.DrainQueueAsync(shopKeeperId);
    }

    [HttpPost]
    public async Task CancelQueueAsync(Guid sessionId)
    {
        var assignment = await CallCenterManager.GetAssignmentAsync(sessionId);
        if (assignment == null) return;
        await CheckPolicyForUserAsync(assignment.CustomerId);
        await CallCenterManager.CancelQueueAsync(sessionId, assignment.CustomerId);
    }

    /// <summary>
    /// 转接
    /// </summary>
    /// <param name="sessionUnitId">当前会话单元Id</param>
    /// <param name="destinationId">目标会话单元Id</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<SessionUnitOwnerDetailDto> TransferToAsync([Required] Guid sessionUnitId, [Required] long destinationId)
    {
        var source = await SessionUnitManager.GetAsync(sessionUnitId);
        await CheckPolicyForUserAsync(source.OwnerId);

        var entity = await CallCenterManager.TransferToAsync(sessionUnitId, destinationId, isNotice: true);

        return ObjectMapper.Map<SessionUnit, SessionUnitOwnerDetailDto>(entity);
    }
}
