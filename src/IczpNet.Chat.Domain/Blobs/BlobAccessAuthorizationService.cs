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
    ISessionUnitManager sessionUnitManager,
    ISessionUnitRepository sessionUnitRepository) : DomainService, IBlobAccessAuthorizationService
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
            // 已登录用户未传 su/msg 上下文时，校验当前用户是否属于该 Blob 授权的任意会话
            if (currentUserChatObjectIds != null && currentUserChatObjectIds.Count > 0)
            {
                if (blob.UploaderChatObjectId.HasValue && Contains(currentUserChatObjectIds, blob.UploaderChatObjectId.Value))
                {
                    return blob;
                }

                var refSessionIds = (await blobMessageRefRepository.GetQueryableAsync())
                    .Where(x => x.BlobId == blobId && x.SessionId.HasValue)
                    .Select(x => x.SessionId.Value)
                    .Distinct()
                    .ToList();

                if (refSessionIds.Count > 0)
                {
                    var isMember = await sessionUnitRepository.AnyAsync(
                        x => refSessionIds.Contains(x.SessionId.Value) &&
                             currentUserChatObjectIds.Contains(x.OwnerId) &&
                             x.Setting.IsEnabled,
                        cancellationToken);
                    if (isMember)
                    {
                        return blob;
                    }
                }

                // 兼容历史消息（迁移前未写 BlobMessageRef）：通过内容表的索引 BlobId 关联反查 SessionId
                var legacySessionIds = await FindLegacySessionIdsByBlobIdAsync(blobId);
                if (legacySessionIds.Count > 0)
                {
                    var isLegacyMember = await sessionUnitRepository.AnyAsync(
                        x => legacySessionIds.Contains(x.SessionId.Value) &&
                             currentUserChatObjectIds.Contains(x.OwnerId) &&
                             x.Setting.IsEnabled,
                        cancellationToken);
                    if (isLegacyMember)
                    {
                        return blob;
                    }
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

        if (!await messageRepository.CanReadAsync(sessionUnitId.Value, targetMessageId.Value))
        {
            throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied);
        }

        var hasRef = await blobMessageRefRepository.AnyAsync(
            x => x.BlobId == blobId && x.MessageId == targetMessageId.Value,
            cancellationToken);

        if (hasRef)
        {
            return blob;
        }

        // 兼容历史消息（改造前未写 BlobMessageRef，但 ContentJson 中包含该 BlobId）
        var message = await messageRepository.FindAsync(targetMessageId.Value, cancellationToken: cancellationToken);
        if (message != null &&
            !string.IsNullOrWhiteSpace(message.ContentJson) &&
            message.ContentJson.Contains(blobId.ToString("D"), StringComparison.OrdinalIgnoreCase))
        {
            return blob;
        }

        throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied);
    }

    private async Task<List<Guid>> FindLegacySessionIdsByBlobIdAsync(Guid blobId)
    {
        var sessionIds = new HashSet<Guid>();

        var imageRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<MessageSections.Templates.ImageContent, Guid>>();
        foreach (var sid in (await imageRepo.GetQueryableAsync())
            .Where(x => x.BlobId == blobId || x.ThumbnailBlobId == blobId || x.OriginalBlobId == blobId)
            .SelectMany(x => x.MessageList.Where(m => m.SessionId.HasValue).Select(m => m.SessionId.Value))
            .Distinct()
            .ToList())
        {
            sessionIds.Add(sid);
        }

        var videoRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<MessageSections.Templates.VideoContent, Guid>>();
        foreach (var sid in (await videoRepo.GetQueryableAsync())
            .Where(x => x.BlobId == blobId || x.SnapshotBlobId == blobId || x.SnapshotThumbnailBlobId == blobId || x.GifBlobId == blobId)
            .SelectMany(x => x.MessageList.Where(m => m.SessionId.HasValue).Select(m => m.SessionId.Value))
            .Distinct()
            .ToList())
        {
            sessionIds.Add(sid);
        }

        var soundRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<MessageSections.Templates.SoundContent, Guid>>();
        foreach (var sid in (await soundRepo.GetQueryableAsync())
            .Where(x => x.BlobId == blobId)
            .SelectMany(x => x.MessageList.Where(m => m.SessionId.HasValue).Select(m => m.SessionId.Value))
            .Distinct()
            .ToList())
        {
            sessionIds.Add(sid);
        }

        var fileRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<MessageSections.Templates.FileContent, Guid>>();
        foreach (var sid in (await fileRepo.GetQueryableAsync())
            .Where(x => x.BlobId == blobId)
            .SelectMany(x => x.MessageList.Where(m => m.SessionId.HasValue).Select(m => m.SessionId.Value))
            .Distinct()
            .ToList())
        {
            sessionIds.Add(sid);
        }

        return sessionIds.ToList();
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
