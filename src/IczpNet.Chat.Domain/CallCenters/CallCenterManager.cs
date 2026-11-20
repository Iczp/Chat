using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Templates;
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

        var isOnline = await ServiceStateManager.IsOnlineAsync(waiterId);

        if (!isOnline)
        {
            Logger.LogWarning($"Waiter Name:${shopWaiter.Name},Id:{shopWaiter.Id} is offline");
        }

        var waiterSessionUnit = await SessionUnitManager.CreateIfNotContainsAsync(
                session: sessionUnit.Session,
                owner: shopWaiter,
                destination: sessionUnit.Destination,
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
        return waiterSessionUnit;
    }
}
