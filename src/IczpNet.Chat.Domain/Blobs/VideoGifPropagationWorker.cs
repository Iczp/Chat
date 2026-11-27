using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace IczpNet.Chat.Blobs;

/// <summary>
/// 定时补偿扫描并投递未完成的视频 GIF 传播 Outbox 记录
/// </summary>
public class VideoGifPropagationWorker : AsyncPeriodicBackgroundWorkerBase
{
    public VideoGifPropagationWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration) : base(timer, serviceScopeFactory)
    {
        var periodSeconds = Math.Max(5, configuration.GetValue("Chat:Blobs:GifPropagationPeriodSeconds", 15));
        Timer.Period = periodSeconds * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var outboxService = workerContext.ServiceProvider.GetRequiredService<IVideoGifPropagationOutboxService>();
        var logger = workerContext.ServiceProvider.GetService<ILogger<VideoGifPropagationWorker>>();

        var totalProcessed = 0;
        while (!workerContext.CancellationToken.IsCancellationRequested)
        {
            var processed = await outboxService.ProcessPendingAsync(50, workerContext.CancellationToken);
            totalProcessed += processed;
            if (processed < 50)
            {
                break;
            }
        }

        if (totalProcessed > 0)
        {
            logger?.LogInformation("VideoGifPropagationWorker processed {TotalProcessed} GIF propagation outbox item(s).", totalProcessed);
        }

        var reuseManager = workerContext.ServiceProvider.GetService<IAttachmentProcessingReuseManager>();
        if (reuseManager != null)
        {
            var recovered = await reuseManager.RecoverPendingGifWaitersAsync(20, workerContext.CancellationToken);
            if (recovered > 0)
            {
                logger?.LogInformation("VideoGifPropagationWorker recovered {RecoveredCount} pending GIF waiter group(s).", recovered);
            }
        }
    }
}
