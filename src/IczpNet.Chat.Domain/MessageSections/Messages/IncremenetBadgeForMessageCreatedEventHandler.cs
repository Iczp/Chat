using IczpNet.Chat.Developers;
using IczpNet.Chat.Follows;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;
using Volo.Abp.Json;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Messages;

/// <summary>
/// 角标增量事件处理
/// </summary>
public class IncremenetBadgeForMessageCreatedEventHandler(
    IIncremenetBadge incremenetBadge,
    ISessionUnitManager sessionUnitManager,
    IFollowManager followManager,
    IJsonSerializer jsonSerializer,
    IBackgroundJobManager backgroundJobManager,
    IDeveloperManager developerManager) : DomainService, ILocalEventHandler<EntityCreatedEventData<Message>>, ITransientDependency
{
    public IIncremenetBadge IncremenetBadge { get; } = incremenetBadge;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    protected IFollowManager FollowManager { get; } = followManager;
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;
    public IBackgroundJobManager BackgroundJobManager { get; } = backgroundJobManager;
    protected IDeveloperManager DeveloperManager { get; } = developerManager;

    [UnitOfWork]
    public async Task HandleEventAsync(EntityCreatedEventData<Message> eventData)
    {
        await Task.Yield();
        var message = eventData.Entity;
        Logger.LogInformation($"Created message:{message}");

        var isPrivateMessage = message.IsPrivateMessage();
        // Args
        var sessionUnitIncrementJobArgs = new SessionUnitIncrementJobArgs()
        {
            SessionId = message.SessionId.Value,
            SenderSessionUnitId = message.SenderSessionUnitId.Value,
            RemindSessionUnitIdList = message.MessageReminderList.Select(x => x.SessionUnitId).ToList(),
            PrivateBadgeSessionUnitIdList = isPrivateMessage ? [message.ReceiverSessionUnitId.Value] : [],
            FollowingSessionUnitIdList = !isPrivateMessage ? await FollowManager.GetFollowerIdListAsync(message.SenderSessionUnitId.Value) : [],
            LastMessageId = message.Id,
            IsRemindAll = message.IsRemindAll,
            MessageCreationTime = message.CreationTime
        };

        Logger.LogInformation($"{nameof(SessionUnitIncrementJobArgs)}:{JsonSerializer.Serialize(sessionUnitIncrementJobArgs)}");

        if (await IncremenetBadge.ShouldbeBackgroundJobAsync(message))
        {
            var jobId = await BackgroundJobManager.EnqueueAsync(sessionUnitIncrementJobArgs);
            Logger.LogInformation($"{nameof(SessionUnitIncrementJobArgs)} backgroupJobId:{jobId}");
        }
        else
        {
            Logger.LogWarning($"BackgroundJobManager.IsAvailable():False");
            await SessionUnitManager.IncremenetAsync(sessionUnitIncrementJobArgs);
        }
    }
}
