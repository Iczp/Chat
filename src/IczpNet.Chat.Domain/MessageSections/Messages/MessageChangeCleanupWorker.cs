using IczpNet.Chat.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Settings;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Messages;

/// <summary>
/// 定时物理清理已软删除且超过保留天数的 MessageChange 历史记录。
/// </summary>
public class MessageChangeCleanupWorker : AsyncPeriodicBackgroundWorkerBase
{
    public const int DefaultBatchSize = 500;

    public MessageChangeCleanupWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration) : base(timer, serviceScopeFactory)
    {
        var periodSeconds = Math.Max(60, configuration.GetValue("Chat:MessageChanges:CleanupPeriodSeconds", 3600));
        Timer.Period = periodSeconds * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        await CleanupExpiredAsync(workerContext.ServiceProvider, workerContext.CancellationToken);
    }

    public static async Task<int> CleanupExpiredAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default,
        int batchSize = DefaultBatchSize)
    {
        var settingProvider = serviceProvider.GetRequiredService<ISettingProvider>();
        var clock = serviceProvider.GetRequiredService<IClock>();
        var dataFilter = serviceProvider.GetRequiredService<IDataFilter>();
        var unitOfWorkManager = serviceProvider.GetRequiredService<IUnitOfWorkManager>();
        var repository = serviceProvider.GetRequiredService<IMessageChangeRepository>();
        var asyncExecuter = serviceProvider.GetRequiredService<IAsyncQueryableExecuter>();
        var logger = serviceProvider.GetService<ILogger<MessageChangeCleanupWorker>>();

        var retentionDays = Math.Max(1, await settingProvider.GetAsync(ChatSettings.MessageChangeRetentionDays, defaultValue: 30));
        var cutoffTime = clock.Now.AddDays(-retentionDays);
        var effectiveBatchSize = Math.Max(1, batchSize);
        var totalDeleted = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            using var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
            int batchCount;
            using (dataFilter.Disable<ISoftDelete>())
            {
                var queryable = await repository.GetQueryableAsync();
                var expiredBatch = await asyncExecuter.ToListAsync(
                    queryable
                        .Where(x => x.IsDeleted && x.DeletionTime != null && x.DeletionTime < cutoffTime)
                        .OrderBy(x => x.DeletionTime)
                        .ThenBy(x => x.Id)
                        .Take(effectiveBatchSize),
                    cancellationToken);

                batchCount = expiredBatch.Count;
                if (batchCount == 0)
                {
                    break;
                }

                await repository.HardDeleteAsync(expiredBatch, autoSave: true, cancellationToken: cancellationToken);
                totalDeleted += batchCount;
            }

            await uow.CompleteAsync(cancellationToken);

            if (batchCount < effectiveBatchSize)
            {
                break;
            }
        }

        if (totalDeleted > 0)
        {
            logger?.LogInformation(
                "Physically cleaned up {Count} expired soft-deleted MessageChange records older than {CutoffTime} (RetentionDays={RetentionDays}).",
                totalDeleted,
                cutoffTime,
                retentionDays);
        }

        return totalDeleted;
    }
}
