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

    /// <inheritdoc/>
    public async Task<SessionUnit> TransferToAsync(Guid sessionUnitId, long waiterId, bool isNotice = true)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

        Assert.If(sessionUnit.OwnerId == waiterId, "Unable to transfer to oneself");

        Assert.If(!sessionUnit.IsWaiterOfDestination(), "Destination object type error");

        Assert.If(!sessionUnit.IsWaiterOfOwner(), "Owner object type error");

        var waiters = await ChatObjectManager.GetRootChildrenAsync(sessionUnit.OwnerId);

        var shopWaiter = waiters.FirstOrDefault(x => x.Id.Equals(waiterId));

        Assert.If(shopWaiter == null, "Is not some root");

        //Assert.If(!await ChatObjectManager.IsSomeRootAsync(sessionUnit.OwnerId, waiterId), "Is not some root");

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

        if (sessionUnit.OwnerObjectType != ChatObjectTypeEnums.ShopKeeper)
        {
            sessionUnit.Setting.IsDisplay = false;
        }
        waiterSessionUnit.Setting.IsDisplay = true;

        waiterSessionUnit.Setting.IsInputEnabled = true;

        // set other waiter
        var ignoreIdList = new List<long>() { sessionUnit.OwnerId, waiterId };

        foreach (var waiter in waiters.Where(x => !ignoreIdList.Contains(x.Id)))
        {
            var unit = await SessionUnitManager.FindAsync(waiter.Id, sessionUnit.DestinationId.Value);
            if (unit != null)
            {
                unit.Setting.IsDisplay = false;
            }
        }
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
            sessionUnit.Setting.ReadedMessageId = message.Id;
        }
        return waiterSessionUnit;
    }
}
