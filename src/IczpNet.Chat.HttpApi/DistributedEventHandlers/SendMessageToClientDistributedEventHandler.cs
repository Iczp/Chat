using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;

namespace IczpNet.Chat.DistributedEventHandlers;

public class SendMessageToClientDistributedEventHandler : SendToClientDistributedEventHandler<SendMessageToClientDistributedEto>, ITransientDependency
{
    public ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();
    public ISessionUnitCacheManager SessionUnitCacheManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitCacheManager>();
    public IConnectionPoolManager ConnectionPoolManager => LazyServiceProvider.LazyGetRequiredService<IConnectionPoolManager>();

    public ICurrentHosted CurrentHosted => LazyServiceProvider.LazyGetRequiredService<ICurrentHosted>();
    public IJsonSerializer JsonSerializer => LazyServiceProvider.LazyGetRequiredService<IJsonSerializer>();

    public override async Task HandleEventAsync(SendMessageToClientDistributedEto eventData)
    {
        await MeasureAsync(nameof(SendToClientBySessionAsync), () => SendToClientBySessionAsync(eventData));
    }

    /// <summary>
    /// 推送消息（在线状的会话索引取连接信息 - 可能要移除）
    /// </summary>
    /// <param name="eventData"></param>
    /// <returns></returns>
    protected async Task<bool> SendToClientBySessionBackAsync(SendMessageToClientDistributedEto eventData)
    {
        var sessionId = eventData.Message.SessionId;
        var command = eventData.Command;
        //var reminderIdList = eventData.ReminderIdList;
        //var followerIdList = eventData.FollowerIdList;

        var connDict = await OnlineManager.GetConnectionsBySessionAsync(sessionId);

        var onlineOwnerIds = connDict.SelectMany(x => x.Value).Distinct().ToList();

        var members = await SessionUnitCacheManager.GetMembersAsync(sessionId);
        var ownerUnitMap = members.Where(x => onlineOwnerIds.Contains(x.OwnerId)).ToDictionary(x => x.OwnerId, x => x.Id);

        //await HubContext.Clients.Group(sessionId.ToString()).ReceivedMessage(commandPayload);

        foreach (var item in connDict)
        {
            var connectionId = item.Key;
            var chatObjectIdList = item.Value;
            var units = chatObjectIdList
                .Select(chatObjectId =>
                {
                    var sessionUnitId = ownerUnitMap.GetOrDefault(chatObjectId);
                    return new CommandPayload.ScopeUnit
                    {
                        ChatObjectId = chatObjectId,
                        //SessionUnitId = sessionUnitInfoList.Find(x => x.OwnerId == chatObjectId).Id
                        SessionUnitId = sessionUnitId,
                        //Extra = new
                        //{
                        //    IsReminder = reminderIdList.Contains(sessionUnitId),
                        //    IsFollowing = followerIdList.Contains(sessionUnitId),
                        //}
                    };
                }).ToList();

            var commandPayload = new CommandPayload()
            {
                //AppUserId = item.UserId,
                Scopes = units,
                Command = command,
                Payload = eventData,
            };

            await HubContext.Clients.Client(connectionId).ReceivedMessage(commandPayload);
        }

        return true;
    }

    /// <summary>
    /// 推送消息(从登录设备取连接信息)
    /// </summary>
    /// <param name="eventData"></param>
    /// <returns></returns>
    protected async Task<bool> SendToClientBySessionAsync(SendMessageToClientDistributedEto eventData)
    {
        var sessionId = eventData.Message.SessionId;

        var command = eventData.Command;

        var members = await SessionUnitCacheManager.GetMembersAsync(sessionId);

        var ownerIds = members.Select(x => x.OwnerId).Distinct().ToList();

        var ownerDevices = await OnlineManager.GetDevicesAsync(ownerIds);

        var ownerUnitMap = members.DistinctBy(x => x.OwnerId).ToDictionary(x => x.OwnerId, x => x.Id);

        var connMap = ownerDevices.SelectMany(x => x.Value).GroupBy(x => x.ConnectionId).ToDictionary(x => x.Key, x => x.ToList());

        foreach (var item in connMap)
        {
            var connectionId = item.Key;
            var devices = item.Value;
            var units = devices
                .Select(device =>
                {
                    var sessionUnitId = ownerUnitMap.GetOrDefault(device.OwnerId);
                    return new CommandPayload.ScopeUnit
                    {
                        ChatObjectId = device.OwnerId,
                        //SessionUnitId = sessionUnitInfoList.Find(x => x.OwnerId == chatObjectId).Id
                        SessionUnitId = sessionUnitId,
                        //Extra = new
                        //{
                        //    IsReminder = reminderIdList.Contains(sessionUnitId),
                        //    IsFollowing = followerIdList.Contains(sessionUnitId),
                        //}
                    };
                }).ToList();

            var commandPayload = new CommandPayload()
            {
                //AppUserId = item.UserId,
                Scopes = units,
                Command = command,
                Payload = eventData,
            };

            await HubContext.Clients.Client(connectionId).ReceivedMessage(commandPayload);
        }

        return true;
    }
}
