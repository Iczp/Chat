using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace IczpNet.Chat.QrCodes;

/// <summary>
/// 定时物理清理已停用或已过期超过保留天数的 QrCode 历史记录，避免数据库膨胀。
/// </summary>
public class QrCodeCleanupWorker : AsyncPeriodicBackgroundWorkerBase
{
    public const int DefaultBatchSize = 500;

    public QrCodeCleanupWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration) : base(timer, serviceScopeFactory)
    {
        var periodSeconds = Math.Max(60, configuration.GetValue("Chat:QrCodes:CleanupPeriodSeconds", 3600));
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
        var options = serviceProvider.GetService<IOptions<QrCodeOptions>>()?.Value ?? new QrCodeOptions();
        var clock = serviceProvider.GetRequiredService<IClock>();
        var dataFilter = serviceProvider.GetRequiredService<IDataFilter>();
        var unitOfWorkManager = serviceProvider.GetRequiredService<IUnitOfWorkManager>();
        var repository = serviceProvider.GetRequiredService<IRepository<QrCode, Guid>>();
        var asyncExecuter = serviceProvider.GetRequiredService<IAsyncQueryableExecuter>();
        var logger = serviceProvider.GetService<ILogger<QrCodeCleanupWorker>>();

        var retentionDays = Math.Max(1, options.CleanupRetentionDays);
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
                var staleBatch = await asyncExecuter.ToListAsync(
                    queryable
                        .Where(x =>
                            (!x.IsEnabled && (x.LastModificationTime ?? x.CreationTime) < cutoffTime)
                            || (!x.IsPermanent && x.ExpirationTime != null && x.ExpirationTime < cutoffTime))
                        .OrderBy(x => x.CreationTime)
                        .ThenBy(x => x.Id)
                        .Take(effectiveBatchSize),
                    cancellationToken);

                batchCount = staleBatch.Count;
                if (batchCount == 0)
                {
                    break;
                }

                await repository.HardDeleteAsync(staleBatch, autoSave: true, cancellationToken: cancellationToken);
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
            logger?.LogInformation("QrCodeCleanupWorker deleted {Count} stale QrCode records.", totalDeleted);
        }

        return totalDeleted;
    }
}
