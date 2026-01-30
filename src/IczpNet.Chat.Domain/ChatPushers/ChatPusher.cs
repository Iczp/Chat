using IczpNet.Chat.SessionUnits;
using IczpNet.Pusher;
using IczpNet.Pusher.Commands;
using IczpNet.Pusher.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Local;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.ChatPushers;

public class ChatPusher(
    IPusherPublisher pusherPublisher,
    ILocalEventBus localEventBus,
    ISessionUnitManager sessionUnitManager,
    IObjectMapper objectMapper) : DomainService, IChatPusher
{
    protected IPusherPublisher PusherPublisher { get; } = pusherPublisher;
    protected ILocalEventBus LocalEventBus { get; } = localEventBus;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    protected IObjectMapper ObjectMapper { get; } = objectMapper;

    public async Task<long> ExecuteAsync(ChannelMessagePayload payload)
    {
        try
        {
            var ret = await PusherPublisher.PublishAsync(payload);

            Logger.LogInformation($"ChatPusher PublishAsync[{ret}]:{payload}");

            return ret;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"执行推送出错: {nameof(ChatPusher)},error: {ex.Message}");
            return 0;
        }
    }

    public async Task<Dictionary<string, long>> ExecuteAsync(object payload, Action<ChannelMessagePayload> action)
    {
        var result = new Dictionary<string, long>();

        var channelMessagePayload = new ChannelMessagePayload
        {
            Payload = payload,
        };

        action?.Invoke(channelMessagePayload);

        foreach (var command in CommandAttribute.GetValues(payload.GetType()))
        {
            channelMessagePayload.Command = command;

            var value = await ExecuteAsync(channelMessagePayload);

            result.TryAdd(command, value);
        }
        return result;
    }



    public async Task<Dictionary<string, long>> ExecuteBySessionIdAsync(Guid sessionId, object commandPayload, List<string> ignoreConnections = null)
    {
        await SessionUnitManager.GetOrAddCacheListAsync(sessionId);

        return await ExecuteAsync(commandPayload, x =>
        {
            x.CacheKey = $"{new SessionUnitCacheKey(sessionId)}";
            x.IgnoreConnections = ignoreConnections;
        });
    }

    // send private message 

    public async Task<Dictionary<string, long>> ExecutePrivateAsync(List<SessionUnit> sessionUnitList, object commandPayload, List<string> ignoreConnections = null)
    {
        var sessionUnitCacheList = ObjectMapper.Map<List<SessionUnit>, List<SessionUnitCacheItem>>(sessionUnitList);

        var key = $"{new SessionUnitCacheKey(DateTime.Now.Ticks)}";

        await SessionUnitManager.SetCacheListAsync(key, sessionUnitCacheList, new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });

        return await ExecuteAsync(commandPayload, x =>
        {
            x.CacheKey = key;
            x.IgnoreConnections = ignoreConnections;
        });
    }
}
