using IczpNet.Chat.ChatPushers;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.Commands;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitIncrementJob(
    IUnitOfWorkManager unitOfWorkManager,
    ISessionUnitManager sessionUnitManager,
    IChatPusher chatPusher,
    IMessageManager messageManager,
    IMessageRepository messageRepository,
    IDistributedEventBus distributedEventBus,
    IObjectMapper objectMapper,
    ICurrentHosted currentHosted) : AsyncBackgroundJob<SessionUnitIncrementJobArgs>, ITransientDependency
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    protected IChatPusher ChatPusher { get; } = chatPusher;
    public IMessageManager MessageManager { get; } = messageManager;
    public IMessageRepository MessageRepository { get; } = messageRepository;
    public IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;
    public IObjectMapper ObjectMapper { get; } = objectMapper;
    public ICurrentHosted CurrentHosted { get; } = currentHosted;

    [UnitOfWork]
    public override async Task ExecuteAsync(SessionUnitIncrementJobArgs args)
    {
        using var uow = UnitOfWorkManager.Begin();

        var totalCount = await SessionUnitManager.IncremenetAsync(args);

        Logger.LogInformation($"SessionUnitIncrementJob Completed totalCount:{totalCount}.");

        var message = await MessageRepository.GetAsync(args.LastMessageId);

        await SessionUnitManager.GetOrAddByMessageAsync(message);

        var cacheKey = await SessionUnitManager.GetCacheKeyByMessageAsync(message);

        var eventData = new SendToClientDistributedEto()
        {
            Command = CommandConsts.MessageUpdatedBadge,
            CacheKey = cacheKey,//$"{new SessionUnitCacheKey(args.SessionId)}",
            HostName = CurrentHosted.Name,
            MessageId = args.LastMessageId,
            Message = ObjectMapper.Map<Message, MessageInfo<object>>(message),
        };
        await DistributedEventBus.PublishAsync(eventData);

        Logger.LogInformation($"DistributedEventBus.PublishAsync eventData:{eventData}.");

        //消息角标更新成功后通知前端
        await ChatPusher.ExecuteBySessionIdAsync(args.SessionId, new IncrementCompletedCommandPayload()
        {
            MessageId = args.LastMessageId
        });
    }
}
