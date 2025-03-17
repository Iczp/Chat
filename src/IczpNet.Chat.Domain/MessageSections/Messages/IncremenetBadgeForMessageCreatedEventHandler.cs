using IczpNet.Chat.Developers;
using IczpNet.Chat.Follows;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.Settings;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;
using Volo.Abp.Json;
using Volo.Abp.Settings;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Messages;

/// <summary>
/// 角标增量事件处理
/// </summary>
public class IncremenetBadgeForMessageCreatedEventHandler(
    ISessionUnitManager sessionUnitManager,
    IFollowManager followManager,
    IJsonSerializer jsonSerializer,
    ISettingProvider settingProvider,
    IBackgroundJobManager backgroundJobManager,
    IDeveloperManager developerManager) : DomainService, ILocalEventHandler<EntityCreatedEventData<Message>>, ITransientDependency
{
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    protected IFollowManager FollowManager { get; } = followManager;
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;
    public ISettingProvider SettingProvider { get; } = settingProvider;
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

        if (await ShouldbeBackgroundJobAsync(message))
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

    protected virtual async Task<bool> ShouldbeBackgroundJobAsync(Message message)
    {
        //return true;
        await Task.Yield();

        //return BackgroundJobManager.IsAvailable();

        var useBackgroundJobSenderMinSessionUnitCount = await SettingProvider.GetAsync(ChatSettings.UseBackgroundJobSenderMinSessionUnitCount, 500);

        var shouldbeBackgroundJob = BackgroundJobManager.IsAvailable() && !message.IsPrivateMessage() && message.SessionUnitCount > useBackgroundJobSenderMinSessionUnitCount;

        Logger.LogWarning($"ShouldbeBackgroundJobAsync: shouldbeBackgroundJob={shouldbeBackgroundJob},useBackgroundJobSenderMinSessionUnitCount={useBackgroundJobSenderMinSessionUnitCount}");

        return shouldbeBackgroundJob;
    }
}
