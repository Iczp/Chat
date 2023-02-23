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

        public Task<long> ExecuteAsync<TCommand>(object payload, List<string> ignoreConnections = null)
        {
            return ExecuteAsync(new ChannelMessagePayload
            {
                Command = CommandAttribute.GetValue<TCommand>(),
                Payload = payload,
                IgnoreConnections = ignoreConnections
            });
        }

        public async Task<long> ExecuteAsync(ChannelMessagePayload payload)
        {
            Logger.LogDebug($"ChatPusher PublishAsync:{payload}");

            return await PusherPublisher.PublishAsync(payload);
        }

        public async Task<long> ExecuteBySessionIdAsync(Guid sessionId, object commandPayload, List<string> ignoreConnections = null)
        {
            await SessionUnitManager.GetOrAddCacheListBySessionIdAsync(sessionId);

            return await ExecuteAsync(new ChannelMessagePayload
            {
                SessionId = sessionId,
                Command = CommandAttribute.GetValue(commandPayload.GetType()),
                Payload = commandPayload,
                IgnoreConnections = ignoreConnections
            });
        }
    }
}
