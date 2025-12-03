using IczpNet.Chat.Settings;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Volo.Abp.Settings;

namespace IczpNet.Chat.MessageSections.Messages;

public class IncremenetBadge(
    ISettingProvider settingProvider, 
    IBackgroundJobManager backgroundJobManager) : DomainService, IIncremenetBadge, ITransientDependency
{
    public ISettingProvider SettingProvider { get; } = settingProvider;
    public IBackgroundJobManager BackgroundJobManager { get; } = backgroundJobManager;

    /// <summary>
    /// 更新角标是否应该作为后台作业
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task<bool> ShouldbeBackgroundJobAsync(Message message)
    {
        //return true;

        //return BackgroundJobManager.IsAvailable();

        var sessionUnitCount = message.SessionUnitCount;

        var useBackgroundJobSenderMinSessionUnitCount = await SettingProvider.GetAsync(ChatSettings.UseBackgroundJobSenderMinSessionUnitCount, 500);

        var shouldbeBackgroundJob = BackgroundJobManager.IsAvailable() && !message.IsPrivateMessage() && sessionUnitCount > useBackgroundJobSenderMinSessionUnitCount;

        Logger.LogWarning($"ShouldbeBackgroundJobAsync: shouldbeBackgroundJob={shouldbeBackgroundJob}, sessionUnitCount:{sessionUnitCount},useBackgroundJobSenderMinSessionUnitCount={useBackgroundJobSenderMinSessionUnitCount}");

        return shouldbeBackgroundJob;
    }
}
