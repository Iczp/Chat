using IczpNet.Chat.Commands;
using IczpNet.Pusher;
using IczpNet.Pusher.Models;
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

        public Task ExecuteAsync<TCommand>(object payload, List<string> ignoreConnections = null)
        {
            return ExecuteAsync(new CommandPayload
            {
                Command = CommandAttribute.GetValue<TCommand>(),
                Payload = payload,
                IgnoreConnections = ignoreConnections
            });
        }

        public async Task ExecuteAsync(CommandPayload commandData)
        {
            Logger.LogDebug($"ChatPusher PublishAsync:{commandData}");

            await PusherPublisher.PublishAsync(commandData);

            //await LocalEventBus.PublishAsync(commandData);
        }
    }
}
