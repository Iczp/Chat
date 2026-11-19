using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// 定时物理清理创建时间超过 30 天的会话处置记录（防表膨胀）
/// </summary>
public class SessionUnitDisposalCleanupWorker : AsyncPeriodicBackgroundWorkerBase
{
    public const int DefaultBatchSize = 500;

    public SessionUnitDisposalCleanupWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration) : base(timer, serviceScopeFactory)
    {
        var periodSeconds = Math.Max(300, configuration.GetValue("Chat:SessionUnitDisposals:CleanupPeriodSeconds", 86400));
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
        var clock = serviceProvider.GetRequiredService<IClock>();
        var unitOfWorkManager = serviceProvider.GetRequiredService<IUnitOfWorkManager>();
        var repository = serviceProvider.GetRequiredService<IRepository<SessionUnitDisposalRecord, long>>();
        var asyncExecuter = serviceProvider.GetRequiredService<IAsyncQueryableExecuter>();
        var logger = serviceProvider.GetService<ILogger<SessionUnitDisposalCleanupWorker>>();

        var cutoffTime = clock.Now.AddDays(-30);
        var effectiveBatchSize = Math.Max(1, batchSize);
        var totalDeleted = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            using var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
            var query = (await repository.GetQueryableAsync())
                .Where(x => x.CreationTime < cutoffTime)
                .OrderBy(x => x.Id)
                .Take(effectiveBatchSize);

            var batch = await asyncExecuter.ToListAsync(query, cancellationToken);
            if (batch.Count == 0)
            {
                await uow.CompleteAsync(cancellationToken);
                break;
            }

            await repository.DeleteManyAsync(batch, autoSave: true, cancellationToken: cancellationToken);
            await uow.CompleteAsync(cancellationToken);

            totalDeleted += batch.Count;
            if (batch.Count < effectiveBatchSize)
            {
                break;
            }
        }

        if (totalDeleted > 0 && logger != null)
        {
            logger.LogInformation("Cleaned up {Count} expired SessionUnitDisposalRecords older than {CutoffTime}.", totalDeleted, cutoffTime);
        }

        return totalDeleted;
    }
}
