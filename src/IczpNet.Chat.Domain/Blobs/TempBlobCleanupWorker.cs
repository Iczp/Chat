using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace IczpNet.Chat.Blobs;

/// <summary>
/// 定时清理过期未绑定消息的临时附件（Temp 状态且超过 ExpireTime）
/// </summary>
public class TempBlobCleanupWorker : AsyncPeriodicBackgroundWorkerBase
{
    public TempBlobCleanupWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration) : base(timer, serviceScopeFactory)
    {
        var periodSeconds = Math.Max(60, configuration.GetValue("Chat:Blobs:CleanupPeriodSeconds", 3600));
        Timer.Period = periodSeconds * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var cleanupService = workerContext.ServiceProvider.GetRequiredService<ITempBlobCleanupService>();
        var logger = workerContext.ServiceProvider.GetService<ILogger<TempBlobCleanupWorker>>();

        var totalDeleted = 0;
        while (!workerContext.CancellationToken.IsCancellationRequested)
        {
            var deleted = await cleanupService.CleanupExpiredTempBlobsAsync(200, workerContext.CancellationToken);
            totalDeleted += deleted;
            if (deleted < 200)
            {
                break;
            }
        }

        if (totalDeleted > 0)
        {
            logger?.LogInformation("TempBlobCleanupWorker cleaned up {TotalDeleted} expired temporary blobs.", totalDeleted);
        }
    }
}
