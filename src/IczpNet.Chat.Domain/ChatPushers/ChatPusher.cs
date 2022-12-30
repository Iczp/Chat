using IczpNet.Chat.Commands;
using IczpNet.Pusher;
using Microsoft.Extensions.Logging;
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

        public ChatPusher(
            IPusherPublisher pusherPublisher,
            ILocalEventBus localEventBus)
        {
            PusherPublisher = pusherPublisher;
            LocalEventBus = localEventBus;
        }

        public async Task ExecuteAsync<TCommand>(object data, List<string> ignoreConnections = null)
        {
            var eventData = new
            {
                Command = CommandAttribute.GetValue<TCommand>(),
                Data = data,
                IgnoreConnections = ignoreConnections
            };

            Logger.LogDebug($"ChatPusher PublishAsync:{eventData}");

            await PusherPublisher.PublishAsync(eventData);

            await LocalEventBus.PublishAsync(eventData);
        }
    }
}
