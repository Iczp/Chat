using IczpNet.Chat.Options;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Blobs.Validation;

/// <summary>
/// 校验媒体前置元数据（视频封面/时长、语音时长等）
/// </summary>
public class MediaMetadataUploadValidator(
    IOptions<AttachmentUploadOptions> options) : IAttachmentUploadValidator, ITransientDependency
{
    public int Order => 400;

    public Task ValidateAsync(AttachmentUploadValidationContext context, CancellationToken cancellationToken = default)
    {
        switch (context.MediaType)
        {
            case AttachmentMediaType.Video:
                if (options.Value.RequireVideoSnapshotOnUpload && !context.HasSnapshot)
                {
                    throw new BusinessException(ChatErrorCodes.Blobs.InvalidMediaMetadata)
                        .WithData("Reason", "Video snapshot is required on upload.");
                }
                break;

            case AttachmentMediaType.Sound:
                if (context.DurationMs.HasValue && context.DurationMs.Value <= 0)
                {
                    throw new BusinessException(ChatErrorCodes.Blobs.InvalidMediaMetadata)
                        .WithData("Reason", "Sound duration must be positive.");
                }
                break;
        }

        return Task.CompletedTask;
    }
}
