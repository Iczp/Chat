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
using Volo.Abp.Authorization;
using IczpNet.Chat.ServiceStates;

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
    public async Task<System.Collections.Generic.List<CallCenterAgentDto>> GetAgentsAsync(long shopKeeperId)
    {
        await CheckShopOwnerAsync(shopKeeperId);
        var shop = await ChatObjectManager.GetAsync(shopKeeperId);
        var staff = (await ChatObjectManager.GetChildsAsync(shopKeeperId))
            .Where(x => x.ObjectType == ChatObjectTypeEnums.ShopWaiter).ToList();
        var agents = new[] { shop }.Concat(staff).ToList();
        var counts = await GetActiveCountsAsync(shopKeeperId);
        var states = LazyServiceProvider.LazyGetRequiredService<IServiceStateManager>();
        var urlNormalizer = LazyServiceProvider.LazyGetRequiredService<IczpNet.Chat.UrlNormalizers.IUrlNormalizer>();
        var result = new System.Collections.Generic.List<CallCenterAgentDto>(agents.Count);
        foreach (var agent in agents)
            result.Add(new CallCenterAgentDto
            {
                Id = agent.Id,
                Name = agent.Name,
                Thumbnail = urlNormalizer.Normalize(agent.Thumbnail),
                Portrait = urlNormalizer.Normalize(agent.Portrait),
                ServiceStatus = await states.GetStatusAsync(agent.Id),
                ActiveCount = counts.TryGetValue(agent.Id, out var count) ? count : 0
            });
        return result;
    }

    [HttpGet]
    public async Task<CallCenterAssignmentDto> GetAssignmentAsync(Guid sessionId)
    {
        var assignment = await CallCenterManager.GetAssignmentAsync(sessionId)
            ?? throw new UserFriendlyException("客服接待记录不存在。");
        var staff = await ChatObjectManager.GetChildsAsync(assignment.ShopKeeperId);
        if (!CurrentUser.Id.HasValue) throw new AbpAuthorizationException("Authentication required.");
        var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
        var allowedId = new[] { assignment.CustomerId, assignment.ShopKeeperId }
            .Concat(staff.Where(x => x.ObjectType == ChatObjectTypeEnums.ShopWaiter).Select(x => x.Id))
            .FirstOrDefault(ownedIds.Contains);
        if (allowedId == 0) throw new AbpAuthorizationException("The current user is not a member of this consultation.");
        long? queuePosition = null;
        if (assignment.Status == CallCenterAssignmentStatus.Queued)
        {
            var repository = LazyServiceProvider.LazyGetRequiredService<IRepository<CallCenterAssignment, Guid>>();
            var preceding = (await repository.GetQueryableAsync()).Where(x =>
                x.ShopKeeperId == assignment.ShopKeeperId && x.Status == CallCenterAssignmentStatus.Queued &&
                (x.QueuedAt < assignment.QueuedAt ||
                 (x.QueuedAt == assignment.QueuedAt && x.Id.CompareTo(assignment.Id) < 0)));
            queuePosition = await AsyncExecuter.CountAsync(preceding) + 1;
        }
        return new CallCenterAssignmentDto
        {
            SessionId = assignment.SessionId,
            ShopKeeperId = assignment.ShopKeeperId,
            CustomerId = assignment.CustomerId,
            AgentId = assignment.AgentId,
            Status = assignment.Status,
            QueuedAt = assignment.QueuedAt,
            LastActivityAt = assignment.LastActivityAt,
            QueuePosition = queuePosition
        };
    }

    [HttpGet]
    public async Task<CallCenterAssignmentPageDto> GetAssignmentsAsync(long shopKeeperId,
        CallCenterAssignmentStatus status, DateTime? afterQueuedAt = null, Guid? afterId = null,
        int maxResultCount = 30)
    {
        await CheckShopOwnerAsync(shopKeeperId);
        if (status != CallCenterAssignmentStatus.Assigned && status != CallCenterAssignmentStatus.Queued)
            throw new UserFriendlyException("无效的接待状态。");
        if (maxResultCount is < 1 or > 100) throw new UserFriendlyException("分页大小必须在 1 到 100 之间。");
        if (afterQueuedAt.HasValue != afterId.HasValue)
            throw new UserFriendlyException("分页游标不完整。");
        var repository = LazyServiceProvider.LazyGetRequiredService<IRepository<CallCenterAssignment, Guid>>();
        var query = (await repository.GetQueryableAsync())
            .Where(x => x.ShopKeeperId == shopKeeperId && x.Status == status);
        if (afterQueuedAt.HasValue)
            query = query.Where(x => x.QueuedAt > afterQueuedAt.Value ||
                (x.QueuedAt == afterQueuedAt.Value && x.Id.CompareTo(afterId.Value) > 0));
        var rows = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.QueuedAt).ThenBy(x => x.Id)
            .Take(maxResultCount + 1));
        var page = rows.Take(maxResultCount).Select(x => new CallCenterAssignmentDto
        {
            SessionId = x.SessionId,
            ShopKeeperId = x.ShopKeeperId,
            CustomerId = x.CustomerId,
            AgentId = x.AgentId,
            Status = x.Status,
            QueuedAt = x.QueuedAt,
            LastActivityAt = x.LastActivityAt
        }).ToList();
        var last = rows.Count > maxResultCount ? rows[maxResultCount - 1] : null;
        return new CallCenterAssignmentPageDto
        {
            Items = page,
            NextQueuedAt = last?.QueuedAt,
            NextId = last?.Id
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
        if (!CurrentUser.Id.HasValue) throw new AbpAuthorizationException("Authentication required.");
        var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
        if (!ownedIds.Contains(assignment.CustomerId) && !ownedIds.Contains(assignment.ShopKeeperId))
            throw new AbpAuthorizationException("The current user cannot cancel this queue entry.");
        await CallCenterManager.CancelQueueAsync(sessionId, assignment.CustomerId);
    }

    [HttpPost]
    public async Task<CallCenterAssignmentDto> TransferAssignmentAsync(Guid sessionId, long destinationId)
    {
        var assignment = await CallCenterManager.GetAssignmentAsync(sessionId)
            ?? throw new UserFriendlyException("客服接待记录不存在。");
        await CheckShopOwnerAsync(assignment.ShopKeeperId);
        if (assignment.Status != CallCenterAssignmentStatus.Assigned || !assignment.AgentId.HasValue)
            throw new UserFriendlyException("排队会话尚无接待人，不能转接。");
        var source = await SessionUnitManager.FindBySessionIdAsync(sessionId, assignment.AgentId.Value)
            ?? throw new UserFriendlyException("店铺会话不存在。");
        await CallCenterManager.TransferToAsync(source.Id, destinationId, isNotice: true,
            expectedAgentId: assignment.AgentId.Value);
        return await GetAssignmentAsync(sessionId);
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
