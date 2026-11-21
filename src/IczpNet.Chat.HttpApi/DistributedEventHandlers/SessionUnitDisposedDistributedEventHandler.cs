using IczpNet.Chat.Enums;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.SessionUnits;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.DistributedEventHandlers;

/// <summary>
/// 会话处置（移出/删除/隐藏/清空）分布式事件处理器，向客户端推送 SignalR 事件
/// </summary>
public class SessionUnitDisposedDistributedEventHandler : SendToClientDistributedEventHandler<SessionUnitDisposedDistributedEto>, ITransientDependency
{
    public override async Task HandleEventAsync(SessionUnitDisposedDistributedEto eventData)
    {
        await MeasureAsync(nameof(SendToClientAsync), () => SendToClientAsync(eventData));
    }

    protected async Task<bool> SendToClientAsync(SessionUnitDisposedDistributedEto eventData)
    {
        var command = eventData.Command;
        var connIdList = (await OnlineManager.GetConnectionIdsByOwnerAsync(eventData.OwnerId)).ToList();

        var commandPayload = new CommandPayload
        {
            Scopes = [new CommandPayload.ScopeUnit
            {
                ChatObjectId = eventData.OwnerId,
                SessionUnitId = eventData.SessionUnitId
            }],
            Command = command,
            Payload = eventData,
        };

        await HubContext.Clients.Clients(connIdList).ReceivedMessage(commandPayload);

        // 如果是群会话内的移除/退群，向群内其他在线成员广播 member-removed@session
        if (eventData.SessionId.HasValue && eventData.DisposalType == SessionUnitDisposalType.Remove)
        {
            var sessionConnDict = await OnlineManager.GetConnectionsBySessionAsync(eventData.SessionId.Value);
            var sessionConnIds = sessionConnDict.Keys.Where(id => !connIdList.Contains(id)).ToList();
            if (sessionConnIds.Count > 0)
            {
                var memberRemovedPayload = new CommandPayload
                {
                    Command = "member-removed@session",
                    Payload = new
                    {
                        sessionId = eventData.SessionId.Value,
                        sessionUnitId = eventData.SessionUnitId,
                        memberChatObjectId = eventData.OwnerId,
                        reason = (int)eventData.Reason,
                        operatorId = eventData.OperatorId,
                        creationTime = eventData.CreationTime
                    }
                };
                await HubContext.Clients.Clients(sessionConnIds).ReceivedMessage(memberRemovedPayload);
            }
        }

        return true;
    }
}
