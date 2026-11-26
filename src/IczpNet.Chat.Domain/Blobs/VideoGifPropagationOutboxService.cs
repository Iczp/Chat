using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Blobs;

/// <summary>
/// 投递与 GIF 转码同事务落库的待传播 Outbox 记录，失败按指数退避重试
/// </summary>
public class VideoGifPropagationOutboxService(
    IRepository<VideoGifPropagationOutbox, Guid> outboxRepository,
    IMessageManager messageManager) : DomainService, IVideoGifPropagationOutboxService
{
    public virtual async Task<int> ProcessPendingAsync(int batchSize = 50, CancellationToken cancellationToken = default)
    {
        var uowManager = LazyServiceProvider.LazyGetRequiredService<IUnitOfWorkManager>();
        var now = Clock.Now;

        var candidates = Array.Empty<(Guid Id, long SourceMessageId, Guid VideoContentId, Guid GifBlobId, string GifUrl)>();
        using (var readUow = uowManager.Begin(requiresNew: true, isTransactional: false))
        {
            candidates = (await outboxRepository.GetQueryableAsync())
                .Where(x => x.ProcessedTime == null && (x.NextRetryTime == null || x.NextRetryTime <= now))
                .OrderBy(x => x.CreationTime)
                .Take(batchSize)
                .Select(x => new { x.Id, x.SourceMessageId, x.VideoContentId, x.GifBlobId, x.GifUrl })
                .AsEnumerable()
                .Select(x => (x.Id, x.SourceMessageId, x.VideoContentId, x.GifBlobId, x.GifUrl))
                .ToArray();

            await readUow.CompleteAsync(cancellationToken);
        }

        var processedCount = 0;
        foreach (var candidate in candidates)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                // PropagateVideoGifAsync opens its own short requiresNew UOWs per batch; keep it outside
                // any outer transaction so SQLite/SQL Server connections never nest transactions.
                await messageManager.PropagateVideoGifAsync(
                    candidate.SourceMessageId,
                    candidate.VideoContentId,
                    candidate.GifBlobId,
                    candidate.GifUrl);

                using var markUow = uowManager.Begin(requiresNew: true, isTransactional: true);
                var item = await outboxRepository.FindAsync(candidate.Id, cancellationToken: cancellationToken);
                if (item != null && !item.ProcessedTime.HasValue)
                {
                    item.MarkProcessed(Clock.Now);
                    await outboxRepository.UpdateAsync(item, autoSave: true, cancellationToken: cancellationToken);
                    processedCount++;
                }
                await markUow.CompleteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "VideoGifPropagationOutboxService failed to deliver outbox {OutboxId}; scheduling retry.", candidate.Id);

                try
                {
                    using var errorUow = uowManager.Begin(requiresNew: true, isTransactional: true);
                    var item = await outboxRepository.FindAsync(candidate.Id, cancellationToken: cancellationToken);
                    if (item != null && !item.ProcessedTime.HasValue)
                    {
                        var delaySeconds = Math.Min(1800, 15 * (1 << Math.Min(item.RetryCount, 8)));
                        item.MarkFailed(ex.Message, Clock.Now.AddSeconds(delaySeconds));
                        await outboxRepository.UpdateAsync(item, autoSave: true, cancellationToken: cancellationToken);
                    }
                    await errorUow.CompleteAsync(cancellationToken);
                }
                catch (Exception innerEx)
                {
                    Logger.LogWarning(innerEx, "VideoGifPropagationOutboxService failed to record retry metadata for {OutboxId}.", candidate.Id);
                }
            }
        }

        return processedCount;
    }
}
