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
    IMessageManager messageManager,
    IMessageRepository messageRepository,
    IDistributedEventBus distributedEventBus,
    IObjectMapper objectMapper,
    ICurrentHosted currentHosted) : AsyncBackgroundJob<SessionUnitIncrementJobArgs>, ITransientDependency
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
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
        Logger.LogInformation("SessionUnitIncrementJob Completed totalCount:{TotalCount}.", totalCount);

        var message = await MessageRepository.GetAsync(args.LastMessageId);
        await SessionUnitManager.GetOrAddByMessageAsync(message);

        var cacheKey = await SessionUnitManager.GetCacheKeyByMessageAsync(message);
        var eventData = new SendMessageToClientDistributedEto
        {
            Command = CommandConsts.MessageUpdatedBadge,
            CacheKey = cacheKey,
            HostName = CurrentHosted.Name,
            MessageId = args.LastMessageId,
            Message = ObjectMapper.Map<Message, MessageInfo<object>>(message),
        };
        await DistributedEventBus.PublishAsync(eventData);
        Logger.LogInformation("DistributedEventBus published badge update for message {MessageId}.", args.LastMessageId);

        // Pusher's IncrementCompleted command is retired. The distributed event above
        // is consumed by Chat API instances and reaches active clients through SignalR.
        await uow.CompleteAsync();
    }
}
