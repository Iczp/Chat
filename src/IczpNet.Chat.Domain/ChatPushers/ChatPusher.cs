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

        public Task ExecuteAsync<TCommand>(object data, List<string> ignoreConnections = null)
        {
            return ExecuteAsync<TCommand>(new CommandData
            {
                Command = CommandAttribute.GetValue<TCommand>(),
                Payload = data,
                IgnoreConnections = ignoreConnections
            });
        }

        public async Task ExecuteAsync<TCommand>(CommandData commandData)
        {
            Logger.LogDebug($"ChatPusher PublishAsync:{commandData}");

            await PusherPublisher.PublishAsync(commandData);

            await LocalEventBus.PublishAsync(commandData);
        }
    }
}
