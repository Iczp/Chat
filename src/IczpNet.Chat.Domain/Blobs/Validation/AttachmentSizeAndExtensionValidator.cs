using IczpNet.Chat.Options;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Blobs.Validation;

/// <summary>
/// 校验文件大小、文件名安全性与扩展名策略
/// </summary>
public class AttachmentSizeAndExtensionValidator(
    IOptions<AttachmentUploadOptions> options) : IAttachmentUploadValidator, ITransientDependency
{
    public int Order => 200;

    public Task ValidateAsync(AttachmentUploadValidationContext context, CancellationToken cancellationToken = default)
    {
        if (context.FileSize <= 0)
        {
            throw new BusinessException(ChatErrorCodes.Blobs.FileEmpty);
        }

        if (string.IsNullOrWhiteSpace(context.FileName) ||
            context.FileName.Contains("..", StringComparison.Ordinal) ||
            context.FileName.IndexOfAny(['/', '\\']) >= 0)
        {
            throw new BusinessException(ChatErrorCodes.Blobs.FileTypeNotAllowed)
                .WithData("FileName", context.FileName);
        }

        var cfg = options.Value;
        var suffix = context.Suffix ?? string.Empty;

        if (!string.IsNullOrEmpty(suffix) && cfg.BlockedExtensions.Contains(suffix))
        {
            throw new BusinessException(ChatErrorCodes.Blobs.FileTypeNotAllowed)
                .WithData("Suffix", suffix);
        }

        switch (context.MediaType)
        {
            case AttachmentMediaType.Image:
            case AttachmentMediaType.Snapshot:
                if (string.IsNullOrEmpty(suffix) || !cfg.AllowedImageExtensions.Contains(suffix))
                {
                    throw new BusinessException(ChatErrorCodes.Blobs.FileTypeNotAllowed)
                        .WithData("Suffix", suffix);
                }
                break;
            case AttachmentMediaType.Video:
                if (string.IsNullOrEmpty(suffix) || !cfg.AllowedVideoExtensions.Contains(suffix))
                {
                    throw new BusinessException(ChatErrorCodes.Blobs.FileTypeNotAllowed)
                        .WithData("Suffix", suffix);
                }
                break;
            case AttachmentMediaType.Sound:
                if (string.IsNullOrEmpty(suffix) || !cfg.AllowedSoundExtensions.Contains(suffix))
                {
                    throw new BusinessException(ChatErrorCodes.Blobs.FileTypeNotAllowed)
                        .WithData("Suffix", suffix);
                }
                break;
        }

        var maxBytes = context.MediaType switch
        {
            AttachmentMediaType.Image => cfg.MaxImageSize,
            AttachmentMediaType.Snapshot => cfg.MaxSnapshotSize,
            AttachmentMediaType.Video => cfg.MaxVideoSize,
            AttachmentMediaType.Sound => cfg.MaxSoundSize,
            _ => cfg.MaxFileSize,
        };

        if (context.FileSize > maxBytes)
        {
            throw new BusinessException(ChatErrorCodes.Blobs.FileSizeExceeded)
                .WithData("FileSize", context.FileSize)
                .WithData("MaxSize", maxBytes);
        }

        return Task.CompletedTask;
    }
}
