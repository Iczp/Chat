using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Pusher;
using IczpNet.Pusher.Commands;
using IczpNet.Pusher.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Local;

namespace IczpNet.Chat.ChatPushers
{
    public class ChatPusher : DomainService, IChatPusher
    {
        protected IPusherPublisher PusherPublisher { get; }
        protected ILocalEventBus LocalEventBus { get; }
        protected ISessionUnitManager SessionUnitManager { get; }

        public ChatPusher(
            IPusherPublisher pusherPublisher,
            ILocalEventBus localEventBus,
            ISessionUnitManager sessionUnitManager)
        {
            PusherPublisher = pusherPublisher;
            LocalEventBus = localEventBus;
            SessionUnitManager = sessionUnitManager;
        }
        public async Task<long> ExecuteAsync(ChannelMessagePayload payload)
        {
            var ret = await PusherPublisher.PublishAsync(payload);

            Logger.LogInformation($"ChatPusher PublishAsync[{ret}]:{payload}");

            return ret;
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
            await SessionUnitManager.GetOrAddCacheListBySessionIdAsync(sessionId);

            return await ExecuteAsync(commandPayload, x =>
            {
                x.SessionId = sessionId;
                x.IgnoreConnections = ignoreConnections;
            });
        }
    }
}
