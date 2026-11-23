using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace IczpNet.Chat.CallCenters;

/// <summary>Retries durable queues when staff comes online or capacity expires.</summary>
public class CallCenterQueueWorker : AsyncPeriodicBackgroundWorkerBase
{
    public CallCenterQueueWorker(AbpAsyncTimer timer, IServiceScopeFactory scopes, IConfiguration configuration)
        : base(timer, scopes)
    {
        Timer.Period = Math.Max(10, configuration.GetValue("Chat:CallCenter:QueuePeriodSeconds", 30)) * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext context)
    {
        var provider = context.ServiceProvider;
        var repository = provider.GetRequiredService<IRepository<CallCenterAssignment, Guid>>();
        var executer = provider.GetRequiredService<IAsyncQueryableExecuter>();
        var uowManager = provider.GetRequiredService<IUnitOfWorkManager>();
        long[] shops;
        using (var uow = uowManager.Begin(requiresNew: true, isTransactional: false))
        {
            var query = (await repository.GetQueryableAsync())
                .Where(x => x.Status == CallCenterAssignmentStatus.Queued)
                .GroupBy(x => x.ShopKeeperId)
                .Select(x => new { ShopKeeperId = x.Key, FirstQueuedAt = x.Min(item => item.QueuedAt) })
                .OrderBy(x => x.FirstQueuedAt).ThenBy(x => x.ShopKeeperId)
                .Take(100).Select(x => x.ShopKeeperId);
            shops = (await executer.ToListAsync(query, context.CancellationToken)).ToArray();
            await uow.CompleteAsync(context.CancellationToken);
        }
        var manager = provider.GetRequiredService<ICallCenterManager>();
        var logger = provider.GetRequiredService<ILogger<CallCenterQueueWorker>>();
        foreach (var shop in shops)
        {
            if (context.CancellationToken.IsCancellationRequested) break;
            try { await manager.DrainQueueAsync(shop); }
            catch (Exception ex) { logger.LogWarning(ex, "客服排队恢复失败，店长 {ShopKeeperId}", shop); }
        }
    }
}
