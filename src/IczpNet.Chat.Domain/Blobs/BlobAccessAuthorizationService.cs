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
                if (await HasVisibleBoundMessageAsync(blobId, ownerIdList, cancellationToken))
                {
                    return blob;
                }

                // 兼容早期私有消息（缺少 BlobMessageRef）：精确反查结构化附件表并校验消息可见性
                if (await HasVisibleLegacyAttachmentMessageAsync(blobId, ownerIdList, cancellationToken))
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

        var hasRef = await blobMessageRefRepository.AnyAsync(
            x => x.BlobId == blobId && x.MessageId == targetMessageId.Value,
            cancellationToken);

        if (hasRef)
        {
            return blob;
        }

        // 兼容历史消息（改造前未写 BlobMessageRef，但结构化附件字段或 ContentJson 中包含该 BlobId）
        var message = await messageRepository.FindAsync(targetMessageId.Value, cancellationToken: cancellationToken);
        if (message != null &&
            !message.IsRollbackMessage() &&
            !string.IsNullOrWhiteSpace(message.ContentJson) &&
            message.ContentJson.Contains(blobId.ToString("D"), StringComparison.OrdinalIgnoreCase))
        {
            return blob;
        }

        throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied);
    }

    private async Task<bool> HasVisibleBoundMessageAsync(
        Guid blobId,
        IReadOnlyCollection<long> ownerIdList,
        CancellationToken cancellationToken)
    {
        var candidates = (await blobMessageRefRepository.GetQueryableAsync())
            .Where(x => x.BlobId == blobId && x.SessionId.HasValue)
            .OrderByDescending(x => x.CreationTime)
            .Select(x => new { x.MessageId, SessionId = x.SessionId!.Value })
            .Take(50)
            .ToList();

        if (candidates.Count == 0)
        {
            return false;
        }

        var sessionIds = candidates.Select(x => x.SessionId).Distinct().ToList();
        var unitIds = (await sessionUnitRepository.GetQueryableAsync())
            .Where(u => u.SessionId.HasValue &&
                sessionIds.Contains(u.SessionId.Value) &&
                ownerIdList.Contains(u.OwnerId) &&
                !u.Setting.IsKilled)
            .Select(u => new { u.Id, SessionId = u.SessionId!.Value })
            .Take(50)
            .ToList();

        if (unitIds.Count == 0)
        {
            return false;
        }

        var unitsBySession = unitIds.ToLookup(u => u.SessionId, u => u.Id);
        foreach (var item in candidates)
        {
            foreach (var suId in unitsBySession[item.SessionId])
            {
                if (await messageRepository.CanReadAttachmentAsync(suId, item.MessageId, cancellationToken: cancellationToken))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private async Task<bool> HasVisibleLegacyAttachmentMessageAsync(
        Guid blobId,
        IReadOnlyCollection<long> ownerIdList,
        CancellationToken cancellationToken)
    {
        var candidateMessages = new List<(long MessageId, Guid SessionId)>();

        var imageRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<MessageSections.Templates.ImageContent, Guid>>();
        candidateMessages.AddRange((await imageRepo.GetQueryableAsync())
            .Where(x => x.BlobId == blobId || x.ThumbnailBlobId == blobId || x.OriginalBlobId == blobId)
            .SelectMany(x => x.MessageList.Where(m => m.SessionId.HasValue && !m.IsRollbacked && !m.RollbackTime.HasValue)
                .Select(m => new { m.Id, SessionId = m.SessionId!.Value }))
            .Take(20)
            .ToList()
            .Select(x => (x.Id, x.SessionId)));

        var videoRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<MessageSections.Templates.VideoContent, Guid>>();
        candidateMessages.AddRange((await videoRepo.GetQueryableAsync())
            .Where(x => x.BlobId == blobId || x.SnapshotBlobId == blobId || x.SnapshotThumbnailBlobId == blobId || x.GifBlobId == blobId)
            .SelectMany(x => x.MessageList.Where(m => m.SessionId.HasValue && !m.IsRollbacked && !m.RollbackTime.HasValue)
                .Select(m => new { m.Id, SessionId = m.SessionId!.Value }))
            .Take(20)
            .ToList()
            .Select(x => (x.Id, x.SessionId)));

        var soundRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<MessageSections.Templates.SoundContent, Guid>>();
        candidateMessages.AddRange((await soundRepo.GetQueryableAsync())
            .Where(x => x.BlobId == blobId)
            .SelectMany(x => x.MessageList.Where(m => m.SessionId.HasValue && !m.IsRollbacked && !m.RollbackTime.HasValue)
                .Select(m => new { m.Id, SessionId = m.SessionId!.Value }))
            .Take(20)
            .ToList()
            .Select(x => (x.Id, x.SessionId)));

        var fileRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<MessageSections.Templates.FileContent, Guid>>();
        candidateMessages.AddRange((await fileRepo.GetQueryableAsync())
            .Where(x => x.BlobId == blobId)
            .SelectMany(x => x.MessageList.Where(m => m.SessionId.HasValue && !m.IsRollbacked && !m.RollbackTime.HasValue)
                .Select(m => new { m.Id, SessionId = m.SessionId!.Value }))
            .Take(20)
            .ToList()
            .Select(x => (x.Id, x.SessionId)));

        if (candidateMessages.Count == 0)
        {
            return false;
        }

        var sessionIds = candidateMessages.Select(x => x.SessionId).Distinct().ToList();
        var unitIds = (await sessionUnitRepository.GetQueryableAsync())
            .Where(u => u.SessionId.HasValue &&
                sessionIds.Contains(u.SessionId.Value) &&
                ownerIdList.Contains(u.OwnerId) &&
                !u.Setting.IsKilled)
            .Select(u => new { u.Id, SessionId = u.SessionId!.Value })
            .Take(50)
            .ToList();

        var unitsBySession = unitIds.ToLookup(u => u.SessionId, u => u.Id);
        foreach (var (msgId, sessionId) in candidateMessages.Distinct())
        {
            foreach (var suId in unitsBySession[sessionId])
            {
                if (await messageRepository.CanReadAttachmentAsync(suId, msgId, cancellationToken: cancellationToken))
                {
                    return true;
                }
            }
        }

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
