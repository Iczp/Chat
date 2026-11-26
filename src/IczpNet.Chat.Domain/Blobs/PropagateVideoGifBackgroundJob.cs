using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Blobs;

public class PropagateVideoGifBackgroundJob(
    IMessageManager messageManager,
    IRepository<VideoGifPropagationOutbox, Guid> outboxRepository,
    IAsyncQueryableExecuter asyncExecuter,
    IClock clock) : AsyncBackgroundJob<PropagateVideoGifArgs>, ITransientDependency
{
    [UnitOfWork]
    public override async Task ExecuteAsync(PropagateVideoGifArgs args)
    {
        Logger.LogInformation(
            "PropagateVideoGifBackgroundJob started: SourceMessageId={SourceMessageId}, VideoContentId={VideoContentId}, GifBlobId={GifBlobId}",
            args.SourceMessageId,
            args.VideoContentId,
            args.GifBlobId);

        await messageManager.PropagateVideoGifAsync(
            args.SourceMessageId,
            args.VideoContentId,
            args.GifBlobId,
            args.GifUrl);

        var now = clock.Now;
        var pendingOutbox = await asyncExecuter.ToListAsync(
            (await outboxRepository.GetQueryableAsync())
            .Where(x => x.ProcessedTime == null && x.VideoContentId == args.VideoContentId && x.GifBlobId == args.GifBlobId));
        foreach (var item in pendingOutbox)
        {
            item.MarkProcessed(now);
            await outboxRepository.UpdateAsync(item, autoSave: false);
        }

        Logger.LogInformation(
            "PropagateVideoGifBackgroundJob completed: SourceMessageId={SourceMessageId}, VideoContentId={VideoContentId}, GifBlobId={GifBlobId}",
            args.SourceMessageId,
            args.VideoContentId,
            args.GifBlobId);
    }
}
