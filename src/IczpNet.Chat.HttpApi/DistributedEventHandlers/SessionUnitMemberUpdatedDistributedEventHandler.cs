using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.SessionUnits;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.DistributedEventHandlers;

/// <summary>
/// 会话成员变更分布式事件处理器，向客户端推送 member-updated@session
/// </summary>
public class SessionUnitMemberUpdatedDistributedEventHandler : SendToClientDistributedEventHandler<SessionUnitMemberUpdatedDistributedEto>, ITransientDependency
{
    public override async Task HandleEventAsync(SessionUnitMemberUpdatedDistributedEto eventData)
    {
        await MeasureAsync(nameof(SendToClientAsync), () => SendToClientAsync(eventData));
    }

    protected async Task<bool> SendToClientAsync(SessionUnitMemberUpdatedDistributedEto eventData)
    {
        var sessionConnDict = await OnlineManager.GetConnectionsBySessionAsync(eventData.SessionId);
        var sessionConnIds = sessionConnDict.Keys.ToList();
        if (sessionConnIds.Count == 0)
        {
            return true;
        }

        var memberUpdatedPayload = new CommandPayload
        {
            Command = "member-updated@session",
            Payload = new
            {
                sessionId = eventData.SessionId,
                sessionUnitId = eventData.SessionUnitId,
                memberChatObjectId = eventData.MemberChatObjectId,
                isAdmin = eventData.IsAdmin,
                isCreator = eventData.IsCreator,
                score = eventData.Score,
                memberName = eventData.MemberName,
                creationTime = eventData.CreationTime,
            }
        };

        await HubContext.Clients.Clients(sessionConnIds).ReceivedMessage(memberUpdatedPayload);
        return true;
    }
}
