using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Blobs;

public interface ILegacyPublicBlobAuditService
{
    Task<LegacyPublicBlobAuditResult> AuditAndMigrateBatchAsync(
        Guid? afterBlobId = null,
        int batchSize = 200,
        bool dryRun = true,
        CancellationToken cancellationToken = default);
}

public class LegacyPublicBlobAuditResult
{
    public int CandidateCount { get; set; }
    public int MissingRefCount { get; set; }
    public int UnclassifiedCount { get; set; }
    public int BackfilledRefCount { get; set; }
    public int PrivatizedCount { get; set; }
    public Guid? LastProcessedBlobId { get; set; }
    public bool HasMore { get; set; }
}

/// <summary>
/// 历史公开聊天附件可断点核对与分批私有化服务：
/// 先统计候选数量、缺失引用量与无法归类（或公开用途）数量，仅对已确认关联聊天消息的结构化附件补齐 BlobMessageRef 并取消公开标记。
/// </summary>
public class LegacyPublicBlobAuditService(
    IRepository<Blob, Guid> blobRepository,
    IRepository<BlobMessageRef, Guid> blobMessageRefRepository,
    IRepository<ImageContent, Guid> imageContentRepository,
    IRepository<VideoContent, Guid> videoContentRepository,
    IRepository<SoundContent, Guid> soundContentRepository,
    IRepository<FileContent, Guid> fileContentRepository,
    IUnitOfWorkManager unitOfWorkManager) : DomainService, ILegacyPublicBlobAuditService
{
    public virtual async Task<LegacyPublicBlobAuditResult> AuditAndMigrateBatchAsync(
        Guid? afterBlobId = null,
        int batchSize = 200,
        bool dryRun = true,
        CancellationToken cancellationToken = default)
    {
        var effectiveBatchSize = Math.Clamp(batchSize, 1, 1000);
        var result = new LegacyPublicBlobAuditResult();

        List<Blob> candidates;
        bool hasMore;

        var missingRefs = new List<(Guid BlobId, long MessageId, Guid? SessionId)>();
        var confirmedChatBlobIds = new List<Guid>();

        using (var readUow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
        {
            var query = (await blobRepository.GetQueryableAsync())
                .Where(x => x.IsPublic);

            if (afterBlobId.HasValue)
            {
                var cursor = afterBlobId.Value;
                query = query.Where(x => x.Id.CompareTo(cursor) > 0);
            }

            var fetched = query
                .OrderBy(x => x.Id)
                .Take(effectiveBatchSize + 1)
                .ToList();

            hasMore = fetched.Count > effectiveBatchSize;
            candidates = hasMore ? fetched.Take(effectiveBatchSize).ToList() : fetched;

            if (candidates.Count == 0)
            {
                await readUow.CompleteAsync(cancellationToken);
                return result;
            }

            result.CandidateCount = candidates.Count;
            result.LastProcessedBlobId = candidates[^1].Id;
            result.HasMore = hasMore;

            var candidateIds = candidates.Select(x => x.Id).ToList();

            var pairsByBlob = new Dictionary<Guid, List<(long MessageId, Guid? SessionId)>>();
            void AddLink(Guid? maybeBlobId, long msgId, Guid? sessionId)
            {
                if (!maybeBlobId.HasValue || !candidateIds.Contains(maybeBlobId.Value)) return;
                if (!pairsByBlob.TryGetValue(maybeBlobId.Value, out var list))
                {
                    list = [];
                    pairsByBlob[maybeBlobId.Value] = list;
                }
                if (!list.Any(x => x.MessageId == msgId))
                {
                    list.Add((msgId, sessionId));
                }
            }

            var imageLinks = (await imageContentRepository.WithDetailsAsync(x => x.MessageList))
                .AsNoTracking()
                .Where(x =>
                    (x.BlobId.HasValue && candidateIds.Contains(x.BlobId.Value)) ||
                    (x.ThumbnailBlobId.HasValue && candidateIds.Contains(x.ThumbnailBlobId.Value)) ||
                    (x.OriginalBlobId.HasValue && candidateIds.Contains(x.OriginalBlobId.Value)))
                .ToList();
            foreach (var l in imageLinks)
            {
                foreach (var m in l.MessageList)
                {
                    AddLink(l.BlobId, m.Id, m.SessionId);
                    AddLink(l.ThumbnailBlobId, m.Id, m.SessionId);
                    AddLink(l.OriginalBlobId, m.Id, m.SessionId);
                }
            }

            var videoLinks = (await videoContentRepository.WithDetailsAsync(x => x.MessageList))
                .AsNoTracking()
                .Where(x =>
                    (x.BlobId.HasValue && candidateIds.Contains(x.BlobId.Value)) ||
                    (x.SnapshotBlobId.HasValue && candidateIds.Contains(x.SnapshotBlobId.Value)) ||
                    (x.SnapshotThumbnailBlobId.HasValue && candidateIds.Contains(x.SnapshotThumbnailBlobId.Value)) ||
                    (x.GifBlobId.HasValue && candidateIds.Contains(x.GifBlobId.Value)))
                .ToList();
            foreach (var l in videoLinks)
            {
                foreach (var m in l.MessageList)
                {
                    AddLink(l.BlobId, m.Id, m.SessionId);
                    AddLink(l.SnapshotBlobId, m.Id, m.SessionId);
                    AddLink(l.SnapshotThumbnailBlobId, m.Id, m.SessionId);
                    AddLink(l.GifBlobId, m.Id, m.SessionId);
                }
            }

            var soundLinks = (await soundContentRepository.WithDetailsAsync(x => x.MessageList))
                .AsNoTracking()
                .Where(x => x.BlobId.HasValue && candidateIds.Contains(x.BlobId.Value))
                .ToList();
            foreach (var l in soundLinks)
            {
                foreach (var m in l.MessageList)
                {
                    AddLink(l.BlobId, m.Id, m.SessionId);
                }
            }

            var fileLinks = (await fileContentRepository.WithDetailsAsync(x => x.MessageList))
                .AsNoTracking()
                .Where(x => x.BlobId.HasValue && candidateIds.Contains(x.BlobId.Value))
                .ToList();
            foreach (var l in fileLinks)
            {
                foreach (var m in l.MessageList)
                {
                    AddLink(l.BlobId, m.Id, m.SessionId);
                }
            }

            var existingRefSet = (await blobMessageRefRepository.GetQueryableAsync())
                .Where(r => candidateIds.Contains(r.BlobId))
                .Select(r => new { r.BlobId, r.MessageId })
                .ToList()
                .Select(r => (r.BlobId, r.MessageId))
                .ToHashSet();

            foreach (var candidate in candidates)
            {
                var isChatContainer = string.Equals(candidate.Container, "chat-files", StringComparison.OrdinalIgnoreCase);
                if (!isChatContainer || !pairsByBlob.TryGetValue(candidate.Id, out var linkedMessages) || linkedMessages.Count == 0)
                {
                    result.UnclassifiedCount++;
                    continue;
                }

                confirmedChatBlobIds.Add(candidate.Id);
                foreach (var (msgId, sessionId) in linkedMessages)
                {
                    if (!existingRefSet.Contains((candidate.Id, msgId)))
                    {
                        missingRefs.Add((candidate.Id, msgId, sessionId));
                    }
                }
            }

            result.MissingRefCount = missingRefs.Count;
            await readUow.CompleteAsync(cancellationToken);
        }

        if (!dryRun && (missingRefs.Count > 0 || confirmedChatBlobIds.Count > 0))
        {
            using var writeUow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);

            foreach (var (blobId, msgId, sessionId) in missingRefs)
            {
                await blobMessageRefRepository.InsertAsync(
                    new BlobMessageRef(GuidGenerator.Create(), blobId, msgId, sessionId, BlobRefSourceTypes.Send),
                    autoSave: false,
                    cancellationToken: cancellationToken);
                result.BackfilledRefCount++;
            }

            if (confirmedChatBlobIds.Count > 0)
            {
                var toPrivatize = (await blobRepository.GetQueryableAsync())
                    .Where(x => confirmedChatBlobIds.Contains(x.Id) && x.IsPublic)
                    .ToList();
                foreach (var blob in toPrivatize)
                {
                    blob.IsPublic = false;
                    await blobRepository.UpdateAsync(blob, autoSave: false, cancellationToken: cancellationToken);
                    result.PrivatizedCount++;
                }
            }

            await writeUow.CompleteAsync(cancellationToken);
        }

        return result;
    }
}
