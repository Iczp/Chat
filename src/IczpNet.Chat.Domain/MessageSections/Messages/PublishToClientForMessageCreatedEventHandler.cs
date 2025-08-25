using IczpNet.Chat.Enums;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Json;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Messages;

/// <summary>
/// 发送(创建)消息事件处理
/// </summary>
public class PublishToClientForMessageCreatedEventHandler(
    ISessionUnitManager sessionUnitManager,
    IMessageRepository messageRepository,
    IObjectMapper objectMapper,
    ICurrentHosted currentHosted,
    IJsonSerializer jsonSerializer,
    IDistributedEventBus distributedEventBus) : DomainService, ILocalEventHandler<EntityCreatedEventData<Message>>, ITransientDependency
{

    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;

    public IMessageRepository MessageRepository { get; } = messageRepository;
    public IObjectMapper ObjectMapper { get; } = objectMapper;
    public ICurrentHosted CurrentHosted { get; } = currentHosted;
    public IJsonSerializer JsonSerializer { get; } = jsonSerializer;
    public IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;


    [UnitOfWork]
    public async Task HandleEventAsync(EntityCreatedEventData<Message> eventData)
    {
        await Task.Yield();
        var message = eventData.Entity;
        Logger.LogInformation($"{nameof(PublishToClientForMessageCreatedEventHandler)} Created message:{message}");
        await PublishMessageDistributedEventAsync(message);
    }

    protected virtual async Task PublishMessageDistributedEventAsync(Message message, bool onUnitOfWorkComplete = true, bool useOutbox = true)
    {
        var cacheKey = await SessionUnitManager.GetCacheKeyByMessageAsync(message);

        var command = message.ForwardMessageId.HasValue ? Command.Forward : Command.Created;

        // 重新获取，防止导航属性没有加载完全 --- (无效果)
        //var dbMessage = await MessageRepository.GetAsync(message.Id);
        var dbMessage = message;

        var messageDto = ObjectMapper.Map<Message, MessageInfo<object>>(dbMessage);

        //fix: 导航属性没有加载完全 改为手动转换Map
        if (dbMessage.SenderSessionUnit == null && dbMessage.SenderSessionUnitId.HasValue)
        {
            var senderSessionUnit = await SessionUnitManager.GetAsync(dbMessage.SenderSessionUnitId.Value);
            messageDto.SenderSessionUnit = ObjectMapper.Map<SessionUnit, SessionUnitSenderInfo>(senderSessionUnit);
        }

        messageDto.Content ??= message.GetContentDto();

        var eventData = new SendToClientDistributedEto()
        {
            Command = command.ToString(),
            CacheKey = cacheKey,
            HostName = CurrentHosted.Name,
            MessageId = message.Id,
            Message = messageDto
        };

        Logger.LogInformation($"PublishMessageDistributedEventAsync-eventData:{JsonSerializer.Serialize(eventData)}");

        await SessionUnitManager.GetOrAddByMessageAsync(message);

        await DistributedEventBus.PublishAsync(eventData, onUnitOfWorkComplete, useOutbox);

        Logger.LogInformation($"PublishMessageDistributedEventAsync(onUnitOfWorkComplete:{onUnitOfWorkComplete},useOutbox:{useOutbox})[{nameof(SendToClientDistributedEto)}]:{eventData}");
    }
}
