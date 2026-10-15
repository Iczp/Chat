using IczpNet.Chat.Commands;
using IczpNet.Chat.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Messages;

public class MessageUpdatedLocalEventHandler(
    IUnitOfWorkManager unitOfWorkManager,
    IServiceScopeFactory serviceScopeFactory,
    ICurrentHosted currentHosted) : DomainService, ILocalEventHandler<EntityUpdatedEventData<Message>>, ITransientDependency
{

    [UnitOfWork]
    public Task HandleEventAsync(EntityUpdatedEventData<Message> eventData)
    {
        var message = eventData.Entity;
        var unitOfWork = unitOfWorkManager.Current ?? throw new InvalidOperationException("Message updates require a unit of work.");
        var messageId = message.Id;

        // Reuse multi-instance delivery without rebroadcasting message content.
        var notification = !message.IsRollbackMessage() || !message.SessionId.HasValue ? null : new SendMessageToClientDistributedEto
        {
            Command = CommandConsts.MessageRollbacked,
            HostName = currentHosted.Name,
            MessageId = message.Id,
            Message = new MessageInfo<object>
            {
                Id = message.Id,
                SyncVersion = message.SyncVersion,
                ClientMessageId = message.ClientMessageId,
                SessionId = message.SessionId.Value,
                SenderId = message.SenderId,
                SenderSessionUnitId = message.SenderSessionUnitId,
                ReceiverId = message.ReceiverId,
                ReceiverSessionUnitId = message.ReceiverSessionUnitId,
                MessageType = message.MessageType,
                CreationTime = message.CreationTime,
                IsPrivate = message.IsPrivate,
                IsRollbacked = true,
                RollbackTime = message.RollbackTime,
            },
        };
        // Redis Pub/Sub has no durable outbox in every Host. Do not publish
        // before SQL commits. Use the same fresh-scope completion pattern as
        // MessageCacheManager so handler-scoped services cannot be disposed.
        unitOfWork.OnCompleted(async () =>
        {
            using var scope = serviceScopeFactory.CreateScope();
            var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache<MessageCacheItem, MessageCacheKey>>();
            await cache.RemoveAsync(new MessageCacheKey(messageId), hideErrors: false, considerUow: false);
            if (notification != null)
            {
                var bus = scope.ServiceProvider.GetRequiredService<IDistributedEventBus>();
                await bus.PublishAsync(notification, onUnitOfWorkComplete: false, useOutbox: false);
            }
        });
        return Task.CompletedTask;
    }
}
