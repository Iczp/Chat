using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.CallCenters;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
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

    [RemoteService(false)]
    [HttpGet]
    public async Task<CallCenterConfigurationDto> GetConfigurationAsync(long shopKeeperId)
    {
        await CheckShopOwnerAsync(shopKeeperId);
        var repository = LazyServiceProvider.LazyGetRequiredService<IRepository<CallCenterConfiguration, Guid>>();
        var config = await repository.FindAsync(x => x.ShopKeeperId == shopKeeperId);
        return MapConfiguration(config ?? new CallCenterConfiguration(Guid.Empty, shopKeeperId));
    }

    [RemoteService(false)]
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
