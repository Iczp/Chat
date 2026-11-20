using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.ServiceStates;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.TextTemplates;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Uow;
using Volo.Abp;

namespace IczpNet.Chat.CallCenters;

public class CallCenterManager(ISessionUnitManager sessionUnitManager,
    ISessionGenerator sessionGenerator,
    ISessionUnitIdGenerator sessionUnitIdGenerator,
    IChatObjectManager chatObjectManager,
    IMessageSender messageSender,
    IServiceStateManager serviceStateManager) : DomainService, ICallCenterManager
{
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    protected ISessionGenerator SessionGenerator { get; } = sessionGenerator;
    protected ISessionUnitIdGenerator SessionUnitIdGenerator { get; } = sessionUnitIdGenerator;
    protected IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    protected IMessageSender MessageSender { get; } = messageSender;
    protected IServiceStateManager ServiceStateManager { get; } = serviceStateManager;
    protected Type ObjectMapperContext { get; set; }

    private IRepository<CallCenterAssignment, Guid> Assignments => LazyServiceProvider.LazyGetRequiredService<IRepository<CallCenterAssignment, Guid>>();
    private IRepository<CallCenterConfiguration, Guid> Configurations => LazyServiceProvider.LazyGetRequiredService<IRepository<CallCenterConfiguration, Guid>>();
    private IAbpDistributedLock DistributedLock => LazyServiceProvider.LazyGetRequiredService<IAbpDistributedLock>();
    private IUnitOfWorkManager UnitOfWorkManager => LazyServiceProvider.LazyGetRequiredService<IUnitOfWorkManager>();

    private async Task<IAbpDistributedLockHandle> LockShopAsync(long shopKeeperId)
    {
        if (DistributedLock is NullAbpDistributedLock)
            throw new UserFriendlyException("客服分配需要分布式锁。");
        return await DistributedLock.TryAcquireAsync($"Chat:CallCenter:{shopKeeperId}", TimeSpan.FromSeconds(10))
            ?? throw new UserFriendlyException("客服分配正在处理，请重试。");
    }

    public Task<CallCenterAssignment> GetAssignmentAsync(Guid sessionId) =>
        Assignments.FindAsync(x => x.SessionId == sessionId);

    private async Task<CallCenterConfiguration> GetConfigurationAsync(long shopKeeperId) =>
        await Configurations.FindAsync(x => x.ShopKeeperId == shopKeeperId)
        ?? new CallCenterConfiguration(Guid.Empty, shopKeeperId);

    private async Task<List<long>> GetEligibleAgentsAsync(long shopKeeperId, CallCenterConfiguration configuration)
    {
        var children = await ChatObjectManager.GetChildsAsync(shopKeeperId);
        var ids = new List<long> { shopKeeperId };
        ids.AddRange(children.Where(x => x.ObjectType == ChatObjectTypeEnums.ShopWaiter).Select(x => x.Id));
        if (!configuration.RequireOnline) return ids;
        var eligible = new List<long>();
        foreach (var id in ids)
            if (await ServiceStateManager.GetStatusAsync(id) == ServiceStatus.Online) eligible.Add(id);
        return eligible;
    }

    private async Task<bool> TryAssignAsync(CallCenterAssignment assignment, CallCenterConfiguration configuration)
    {
        var eligible = await GetEligibleAgentsAsync(assignment.ShopKeeperId, configuration);
        var counts = await GetActiveCountsAsync(assignment.ShopKeeperId, eligible);
        var candidates = eligible.Select(id => new { Id = id, Count = counts.GetValueOrDefault(id) }).ToList();
        var available = candidates.Where(x => x.Count < configuration.MaxActiveConversations).ToList();
        long? selected = null;
        if (configuration.DistributionStrategy == CallCenterDistributionStrategy.ShopKeeper)
            selected = available.FirstOrDefault(x => x.Id == assignment.ShopKeeperId)?.Id;
        else if (available.Count > 0)
            selected = available.OrderBy(x => x.Count).ThenBy(x => x.Id).First().Id;
        if (selected == null)
        {
            selected = configuration.OverflowStrategy switch
            {
                CallCenterOverflowStrategy.ShopKeeper => assignment.ShopKeeperId,
                CallCenterOverflowStrategy.AllowOverCapacity when candidates.Count > 0 =>
                    candidates.OrderBy(x => x.Count).ThenBy(x => x.Id).First().Id,
                // When every agent is offline, keep the consultation visible to the shopkeeper.
                CallCenterOverflowStrategy.AllowOverCapacity => assignment.ShopKeeperId,
                _ => null
            };
        }
        if (!selected.HasValue) return false;
        assignment.Assign(selected.Value);
        return true;
    }

    public async Task ProcessMessageAsync(Message message)
    {
        if (message.MessageType == MessageTypes.Cmd || !message.SessionId.HasValue || !message.SenderId.HasValue || !message.ReceiverId.HasValue)
            return;
        var sender = await ChatObjectManager.GetAsync(message.SenderId.Value);
        var receiver = await ChatObjectManager.GetAsync(message.ReceiverId.Value);
        var customerTypes = new[] { ChatObjectTypeEnums.Personal, ChatObjectTypeEnums.Anonymous, ChatObjectTypeEnums.Customer };
        var customer = customerTypes.Contains(sender.ObjectType.GetValueOrDefault()) ? sender :
            customerTypes.Contains(receiver.ObjectType.GetValueOrDefault()) ? receiver : null;
        var shop = sender.ObjectType is ChatObjectTypeEnums.ShopKeeper or ChatObjectTypeEnums.ShopWaiter ? sender :
            receiver.ObjectType is ChatObjectTypeEnums.ShopKeeper or ChatObjectTypeEnums.ShopWaiter ? receiver : null;
        if (customer == null || shop == null) return;
        var shopKeeperId = shop.ObjectType == ChatObjectTypeEnums.ShopKeeper ? shop.Id : shop.ParentId;
        if (!shopKeeperId.HasValue) return;
        await using var handle = await LockShopAsync(shopKeeperId.Value);
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var configuration = await GetConfigurationAsync(shopKeeperId.Value);
        var assignment = await GetAssignmentAsync(message.SessionId.Value);
        if (assignment == null)
        {
            assignment = new CallCenterAssignment(GuidGenerator.Create(), shopKeeperId.Value, message.SessionId.Value, customer.Id, message.CreationTime);
            assignment.RecordActivity(message.Id, message.CreationTime);
            var hasQueue = await Assignments.AnyAsync(x => x.ShopKeeperId == shopKeeperId.Value && x.Status == CallCenterAssignmentStatus.Queued);
            if (!hasQueue) await TryAssignAsync(assignment, configuration);
            await Assignments.InsertAsync(assignment);
        }
        else
        {
            if (message.Id <= assignment.LastActivityMessageId)
            {
                await uow.CompleteAsync();
                return;
            }
            var wasExpired = assignment.Status == CallCenterAssignmentStatus.Assigned &&
                !assignment.IsActive(Clock.Now, TimeSpan.FromMinutes(configuration.ActiveTimeoutMinutes));
            assignment.RecordActivity(message.Id, message.CreationTime);
            if (wasExpired)
                assignment.Queue(Clock.Now);
            if (assignment.Status == CallCenterAssignmentStatus.Queued)
            {
                var earlier = await Assignments.AnyAsync(x => x.ShopKeeperId == shopKeeperId.Value &&
                    x.Status == CallCenterAssignmentStatus.Queued && x.SessionId != assignment.SessionId &&
                    x.QueuedAt < assignment.QueuedAt);
                if (!earlier) await TryAssignAsync(assignment, configuration);
            }
            await Assignments.UpdateAsync(assignment);
        }
        await uow.CompleteAsync();
    }

    public async Task<int> DrainQueueAsync(long shopKeeperId, int maxCount = 50)
    {
        if (maxCount < 1 || maxCount > 200) throw new ArgumentOutOfRangeException(nameof(maxCount));
        await using var handle = await LockShopAsync(shopKeeperId);
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var configuration = await GetConfigurationAsync(shopKeeperId);
        var query = (await Assignments.GetQueryableAsync())
            .Where(x => x.ShopKeeperId == shopKeeperId && x.Status == CallCenterAssignmentStatus.Queued)
            .OrderBy(x => x.QueuedAt).ThenBy(x => x.Id).Take(maxCount);
        var waiting = await AsyncExecuter.ToListAsync(query);
        var assigned = 0;
        foreach (var item in waiting)
        {
            if (!await TryAssignAsync(item, configuration)) break;
            await Assignments.UpdateAsync(item, autoSave: true);
            assigned++;
        }
        await uow.CompleteAsync();
        return assigned;
    }

    public async Task CancelQueueAsync(Guid sessionId, long customerId)
    {
        var assignment = await GetAssignmentAsync(sessionId);
        if (assignment == null) return;
        if (assignment.CustomerId != customerId)
            throw new UserFriendlyException("客服排队记录不属于当前客户。");
        await using var handle = await LockShopAsync(assignment.ShopKeeperId);
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        assignment = await GetAssignmentAsync(sessionId);
        if (assignment?.Status == CallCenterAssignmentStatus.Queued)
            await Assignments.DeleteAsync(assignment);
        await uow.CompleteAsync();
    }

    public virtual async Task<Dictionary<long, int>> GetActiveCountsAsync(long shopKeeperId, IReadOnlyCollection<long> agentIds)
    {
        ArgumentNullException.ThrowIfNull(agentIds);
        if (agentIds.Count == 0) return new Dictionary<long, int>();
        if (agentIds.Count > 500) throw new ArgumentOutOfRangeException(nameof(agentIds), "At most 500 agents can be counted per batch.");
        var configurations = LazyServiceProvider.LazyGetRequiredService<IRepository<CallCenterConfiguration, Guid>>();
        var assignments = LazyServiceProvider.LazyGetRequiredService<IRepository<CallCenterAssignment, Guid>>();
        var configuration = await configurations.FindAsync(x => x.ShopKeeperId == shopKeeperId)
            ?? new CallCenterConfiguration(Guid.Empty, shopKeeperId);
        var cutoff = Clock.Now.AddMinutes(-configuration.ActiveTimeoutMinutes);
        var query = (await assignments.GetQueryableAsync())
            .Where(x => x.ShopKeeperId == shopKeeperId && x.Status == CallCenterAssignmentStatus.Assigned &&
                        x.AgentId.HasValue && agentIds.Contains(x.AgentId.Value) && x.LastActivityAt > cutoff)
            .GroupBy(x => x.AgentId.Value)
            .Select(x => new { AgentId = x.Key, Count = x.Count() });
        var counts = await AsyncExecuter.ToListAsync(query);
        return counts.ToDictionary(x => x.AgentId, x => x.Count);
    }

    /// <inheritdoc/>
    public async Task<SessionUnit> TransferToAsync(Guid sessionUnitId, long waiterId, bool isNotice = true)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

        Assert.If(sessionUnit.OwnerId == waiterId, "Unable to transfer to oneself");

        Assert.If(!sessionUnit.IsWaiterOfDestination(), "Destination object type error");

        Assert.If(!sessionUnit.IsWaiterOfOwner(), "Owner object type error");

        var owner = await ChatObjectManager.GetAsync(sessionUnit.OwnerId);
        Assert.If(owner.ObjectType != ChatObjectTypeEnums.ShopKeeper && owner.ObjectType != ChatObjectTypeEnums.ShopWaiter,
            "Transfer source must be a shopkeeper or shop waiter");
        var shopKeeperId = owner.ObjectType == ChatObjectTypeEnums.ShopKeeper
            ? owner.Id
            : owner.ParentId ?? throw new InvalidOperationException("Shop waiter has no shopkeeper.");
        var shopKeeper = owner.Id == shopKeeperId ? owner : await ChatObjectManager.GetAsync(shopKeeperId);
        Assert.If(shopKeeper.ObjectType != ChatObjectTypeEnums.ShopKeeper, "Shop waiter has no valid shopkeeper");
        var shopWaiter = await ChatObjectManager.GetAsync(waiterId);
        var isShopKeeper = shopWaiter.ObjectType == ChatObjectTypeEnums.ShopKeeper && shopWaiter.Id == shopKeeperId;
        var isShopWaiter = shopWaiter.ObjectType == ChatObjectTypeEnums.ShopWaiter && shopWaiter.ParentId == shopKeeperId;
        Assert.If(!isShopKeeper && !isShopWaiter, "Transfer target must belong to the same shop");

        await using var transferLock = await LockShopAsync(shopKeeperId);
        using var transferUow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var customerSessionId = sessionUnit.SessionId ?? throw new InvalidOperationException("Customer session is missing.");
        var assignment = await GetAssignmentAsync(customerSessionId);
        if (assignment != null && (assignment.ShopKeeperId != shopKeeperId || assignment.CustomerId != sessionUnit.DestinationId))
            throw new UserFriendlyException("客服会话归属不一致。");
        if (assignment == null)
        {
            assignment = new CallCenterAssignment(GuidGenerator.Create(), shopKeeperId, customerSessionId,
                sessionUnit.DestinationId ?? throw new InvalidOperationException("Customer is missing."), Clock.Now);
            assignment.Assign(waiterId);
            await Assignments.InsertAsync(assignment);
        }
        else
        {
            assignment.Assign(waiterId);
            await Assignments.UpdateAsync(assignment);
        }

        var isOnline = await ServiceStateManager.IsOnlineAsync(waiterId);

        if (!isOnline)
        {
            Logger.LogWarning($"Waiter Name:${shopWaiter.Name},Id:{shopWaiter.Id} is offline");
        }

        var session = sessionUnit.Session ?? await LazyServiceProvider.LazyGetRequiredService<IRepository<Session, Guid>>()
            .GetAsync(customerSessionId);
        var customer = sessionUnit.Destination ?? await ChatObjectManager.GetAsync(
            sessionUnit.DestinationId ?? throw new InvalidOperationException("Customer is missing."));
        var waiterSessionUnit = await SessionUnitManager.CreateIfNotContainsAsync(
                session: session,
                owner: shopWaiter,
                destination: customer,
                x =>
                {
                    x.JoinWay = JoinWays.AutoJoin;
                });

        waiterSessionUnit.Setting.IsDisplay = true;

        waiterSessionUnit.Setting.IsInputEnabled = true;
        //
        if (isNotice)
        {
            var unit = await SessionUnitManager.MapToCacheAsync(sessionUnit);
            var message = await MessageSender.SendCmdAsync(unit, new MessageInput<CmdContentInfo>()
            {
                Content = new CmdContentInfo()
                {
                    Cmd = MessageKeyNames.Transfer,
                    Text = new TextTemplate("{source} 转接给 {destination}")
                       .WithData("source", new SessionUnitTextTemplate(sessionUnit))
                       .WithData("destination", new SessionUnitTextTemplate(waiterSessionUnit))
                       .ToString(),
                }
            });
            sessionUnit.Setting.ReadMessageId = message.Id;
        }
        await transferUow.CompleteAsync();
        return waiterSessionUnit;
    }
}
