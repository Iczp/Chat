using IczpNet.Chat.ChatPushers;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
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
    IDistributedEventBus distributedEventBus,
    ICurrentHosted currentHosted) : AsyncBackgroundJob<SessionUnitIncrementJobArgs>, ITransientDependency
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    protected IChatPusher ChatPusher { get; } = chatPusher;
    public IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;
    public ICurrentHosted CurrentHosted { get; } = currentHosted;

    [UnitOfWork]
    public override async Task ExecuteAsync(SessionUnitIncrementJobArgs args)
    {
        using var uow = UnitOfWorkManager.Begin();

        var totalCount = await SessionUnitManager.IncremenetAsync(args);

        Logger.LogInformation($"SessionUnitIncrementJob Completed totalCount:{totalCount}.");

        var eventData = new MessageChangedDistributedEto()
        {
            Command = Command.UpdateBadge.ToString(),
            MessageId = args.LastMessageId,
            CacheKey = $"{new SessionUnitCacheKey(args.SessionId)}",
            HostName = CurrentHosted.Name
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
