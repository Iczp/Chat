using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Blobs;

public class PropagateVideoGifBackgroundJob(
    IMessageManager messageManager) : AsyncBackgroundJob<PropagateVideoGifArgs>, ITransientDependency
{
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

        Logger.LogInformation(
            "PropagateVideoGifBackgroundJob completed: SourceMessageId={SourceMessageId}, VideoContentId={VideoContentId}, GifBlobId={GifBlobId}",
            args.SourceMessageId,
            args.VideoContentId,
            args.GifBlobId);
    }
}
