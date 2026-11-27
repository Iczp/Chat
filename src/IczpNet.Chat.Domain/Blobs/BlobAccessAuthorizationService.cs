using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Blobs;

public interface IBlobAccessAuthorizationService
{
    Task<Blob> CheckAccessAsync(
        Guid blobId,
        Guid? sessionUnitId,
        long? messageId,
        long? rootMessageId,
        IReadOnlyCollection<long> currentUserChatObjectIds,
        bool hasValidSignature,
        CancellationToken cancellationToken = default);
}

public class BlobAccessAuthorizationService(
    IRepository<Blob, Guid> blobRepository,
    IRepository<BlobMessageRef, Guid> blobMessageRefRepository,
    IMessageRepository messageRepository,
    ISessionUnitManager sessionUnitManager) : DomainService, IBlobAccessAuthorizationService
{
    public virtual async Task<Blob> CheckAccessAsync(
        Guid blobId,
        Guid? sessionUnitId,
        long? messageId,
        long? rootMessageId,
        IReadOnlyCollection<long> currentUserChatObjectIds,
        bool hasValidSignature,
        CancellationToken cancellationToken = default)
    {
        var blob = await blobRepository.FindAsync(blobId, cancellationToken: cancellationToken);
        if (blob == null)
        {
            return null;
        }

        if (blob.IsPublic)
        {
            return blob;
        }

        var isTempDraft = blob.Status == BlobStatus.Temp &&
            (blob.UploaderChatObjectId.HasValue || blob.ExpireTime.HasValue);
        if (isTempDraft)
        {
            if (blob.ExpireTime.HasValue && blob.ExpireTime.Value < Clock.Now)
            {
                throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied);
            }
            if (!blob.UploaderChatObjectId.HasValue ||
                currentUserChatObjectIds == null ||
                !Contains(currentUserChatObjectIds, blob.UploaderChatObjectId.Value))
            {
                throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied);
            }
            return blob;
        }

        var targetMessageId = rootMessageId ?? messageId;
        if (!sessionUnitId.HasValue || !targetMessageId.HasValue)
        {
            // 已登录旧客户端只传 BlobId 时，必须在当前用户所属身份下存在至少一条当前可见且未撤回的消息
            if (currentUserChatObjectIds != null && currentUserChatObjectIds.Count > 0)
            {
                var ownerIdList = currentUserChatObjectIds.ToList();
                if (await messageRepository.HasVisibleBlobForOwnersAsync(blobId, ownerIdList, cancellationToken))
                {
                    return blob;
                }
            }

            throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied);
        }

        if (!hasValidSignature)
        {
            var unit = await sessionUnitManager.GetCacheAsync(sessionUnitId.Value);
            if (currentUserChatObjectIds == null || !Contains(currentUserChatObjectIds, unit.OwnerId))
            {
                throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied);
            }
        }

        var childMsgId = rootMessageId.HasValue ? messageId : null;
        if (!await messageRepository.CanReadAttachmentAsync(sessionUnitId.Value, targetMessageId.Value, childMsgId, cancellationToken))
        {
            throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied);
        }

        if (childMsgId.HasValue && childMsgId.Value != targetMessageId.Value)
        {
            var hasChildRef = await blobMessageRefRepository.AnyAsync(
                x => x.BlobId == blobId && x.MessageId == childMsgId.Value,
                cancellationToken);
            var hasChildStructured = hasChildRef ||
                await HasStructuredAttachmentOnMessageAsync(childMsgId.Value, blobId, cancellationToken);
            if (!hasChildStructured)
            {
                throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied);
            }

            return blob;
        }

        var hasRef = await blobMessageRefRepository.AnyAsync(
            x => x.BlobId == blobId && x.MessageId == targetMessageId.Value,
            cancellationToken);

        if (hasRef)
        {
            return blob;
        }

        // 兼容历史消息（改造前未写 BlobMessageRef）：仅允许通过结构化附件表精确匹配绑定关系
        if (await HasStructuredAttachmentOnMessageAsync(targetMessageId.Value, blobId, cancellationToken))
        {
            return blob;
        }

        throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied);
    }

    private async Task<bool> HasStructuredAttachmentOnMessageAsync(
        long messageId,
        Guid blobId,
        CancellationToken cancellationToken)
    {
        var imageRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<MessageSections.Templates.ImageContent, Guid>>();
        if ((await imageRepo.GetQueryableAsync()).Any(x =>
                (x.BlobId == blobId || x.ThumbnailBlobId == blobId || x.OriginalBlobId == blobId) &&
                x.MessageList.Any(m => m.Id == messageId && !m.IsRollbacked && !m.RollbackTime.HasValue)))
        {
            return true;
        }

        var videoRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<MessageSections.Templates.VideoContent, Guid>>();
        if ((await videoRepo.GetQueryableAsync()).Any(x =>
                (x.BlobId == blobId || x.SnapshotBlobId == blobId || x.SnapshotThumbnailBlobId == blobId || x.GifBlobId == blobId) &&
                x.MessageList.Any(m => m.Id == messageId && !m.IsRollbacked && !m.RollbackTime.HasValue)))
        {
            return true;
        }

        var soundRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<MessageSections.Templates.SoundContent, Guid>>();
        if ((await soundRepo.GetQueryableAsync()).Any(x =>
                x.BlobId == blobId &&
                x.MessageList.Any(m => m.Id == messageId && !m.IsRollbacked && !m.RollbackTime.HasValue)))
        {
            return true;
        }

        var fileRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<MessageSections.Templates.FileContent, Guid>>();
        if ((await fileRepo.GetQueryableAsync()).Any(x =>
                x.BlobId == blobId &&
                x.MessageList.Any(m => m.Id == messageId && !m.IsRollbacked && !m.RollbackTime.HasValue)))
        {
            return true;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return false;
    }

    private static bool Contains(IReadOnlyCollection<long> ids, long target)
    {
        foreach (var id in ids)
        {
            if (id == target) return true;
        }
        return false;
    }
}
