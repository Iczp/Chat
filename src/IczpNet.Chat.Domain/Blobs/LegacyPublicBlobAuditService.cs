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
    public int ConfirmedChatCount { get; set; }
    public int MissingRefCount { get; set; }
    public int UnclassifiedCount { get; set; }
    public int BackfilledRefCount { get; set; }
    public int PrivatizedCount { get; set; }
    public Guid? LastProcessedBlobId { get; set; }
    public bool HasMore { get; set; }
}

/// <summary>
/// 历史公开聊天附件可断点核对与分批私有化服务：
/// 先统计候选数量、缺失引用量与无法归类（或公开用途）数量，仅对已确认关联聊天消息的结构化附件在单一写事务内幂等绑定 BlobMessageRef、同步 Status/RefCount 并取消公开标记。
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
    private const string ChatFilesContainerName = "chat-files";

    public virtual async Task<LegacyPublicBlobAuditResult> AuditAndMigrateBatchAsync(
        Guid? afterBlobId = null,
        int batchSize = 200,
        bool dryRun = true,
        CancellationToken cancellationToken = default)
    {
        var effectiveBatchSize = Math.Clamp(batchSize, 1, 1000);
        var result = new LegacyPublicBlobAuditResult();
        List<Guid> batchCandidateIds;

        using (var readUow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
        {
            var query = (await blobRepository.GetQueryableAsync())
                .AsNoTracking()
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

            result.HasMore = fetched.Count > effectiveBatchSize;
            var candidates = result.HasMore ? fetched.Take(effectiveBatchSize).ToList() : fetched;
            if (candidates.Count == 0)
            {
                await readUow.CompleteAsync(cancellationToken);
                return result;
            }

            result.CandidateCount = candidates.Count;
            result.LastProcessedBlobId = candidates[^1].Id;
            batchCandidateIds = candidates.Select(x => x.Id).ToList();

            var analysis = await AnalyzeCandidatesAsync(candidates, cancellationToken);
            result.ConfirmedChatCount = analysis.ConfirmedChatBlobIds.Count;
            result.UnclassifiedCount = analysis.UnclassifiedCount;
            result.MissingRefCount = analysis.MissingRefs.Count;

            await readUow.CompleteAsync(cancellationToken);
        }

        if (dryRun || batchCandidateIds.Count == 0)
        {
            return result;
        }

        // 在单一事务 UOW 内重新核对分类与缺失引用，幂等绑定消息引用并同步 Status/RefCount/IsPublic。
        using var writeUow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var currentCandidates = (await blobRepository.GetQueryableAsync())
            .AsNoTracking()
            .Where(x => batchCandidateIds.Contains(x.Id) && x.IsPublic)
            .ToList();

        if (currentCandidates.Count == 0)
        {
            await writeUow.CompleteAsync(cancellationToken);
            return result;
        }

        var writeAnalysis = await AnalyzeCandidatesAsync(currentCandidates, cancellationToken);
        result.ConfirmedChatCount = writeAnalysis.ConfirmedChatBlobIds.Count;
        result.UnclassifiedCount = writeAnalysis.UnclassifiedCount;
        result.MissingRefCount = writeAnalysis.MissingRefs.Count;

        var now = Clock.Now;
        var messageRepository = LazyServiceProvider?.LazyGetService<IMessageRepository>();

        if (writeAnalysis.MissingRefs.Count > 0)
        {
            if (messageRepository != null)
            {
                var groupedByMessage = writeAnalysis.MissingRefs
                    .GroupBy(x => (x.MessageId, x.SessionId));

                foreach (var group in groupedByMessage)
                {
                    var inserted = await messageRepository.BindBlobsToMessageAsync(
                        group.Key.MessageId,
                        group.Key.SessionId,
                        group.Select(x => x.BlobId).ToList(),
                        BlobRefSourceTypes.Send,
                        creatorId: null,
                        now: now,
                        markAsBound: true,
                        cancellationToken: cancellationToken);
                    result.BackfilledRefCount += inserted;
                }
            }
            else
            {
                foreach (var (blobId, msgId, sessionId) in writeAnalysis.MissingRefs)
                {
                    await blobMessageRefRepository.InsertAsync(
                        new BlobMessageRef(GuidGenerator.Create(), blobId, msgId, sessionId, BlobRefSourceTypes.Send),
                        autoSave: true,
                        cancellationToken: cancellationToken);
                    result.BackfilledRefCount++;
                }
            }
        }

        if (writeAnalysis.ConfirmedChatBlobIds.Count > 0)
        {
            var confirmedIds = writeAnalysis.ConfirmedChatBlobIds;
            var activeRefCounts = (await blobMessageRefRepository.GetQueryableAsync())
                .Where(r => confirmedIds.Contains(r.BlobId))
                .GroupBy(r => r.BlobId)
                .Select(g => new { BlobId = g.Key, Count = g.Count() })
                .ToList()
                .ToDictionary(x => x.BlobId, x => x.Count);

            var toPrivatize = (await blobRepository.GetQueryableAsync())
                .Where(x => confirmedIds.Contains(x.Id) && x.IsPublic)
                .ToList();

            foreach (var blob in toPrivatize)
            {
                blob.IsPublic = false;
                blob.Status = BlobStatus.Bound;
                blob.ExpireTime = null;
                if (activeRefCounts.TryGetValue(blob.Id, out var actualRefCount) && actualRefCount > blob.RefCount)
                {
                    blob.RefCount = actualRefCount;
                }
                else if (blob.RefCount <= 0 && writeAnalysis.LinkedCountByBlob.TryGetValue(blob.Id, out var linkedCount) && linkedCount > 0)
                {
                    blob.RefCount = linkedCount;
                }

                await blobRepository.UpdateAsync(blob, autoSave: false, cancellationToken: cancellationToken);
                result.PrivatizedCount++;
            }
        }

        await writeUow.CompleteAsync(cancellationToken);
        return result;
    }

    private sealed class AuditAnalysis
    {
        public List<Guid> ConfirmedChatBlobIds { get; } = [];
        public List<(Guid BlobId, long MessageId, Guid? SessionId)> MissingRefs { get; } = [];
        public Dictionary<Guid, int> LinkedCountByBlob { get; } = [];
        public int UnclassifiedCount { get; set; }
    }

    private async Task<AuditAnalysis> AnalyzeCandidatesAsync(
        List<Blob> candidates,
        CancellationToken cancellationToken)
    {
        var analysis = new AuditAnalysis();
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
            var isChatContainer = string.Equals(candidate.Container, ChatFilesContainerName, StringComparison.OrdinalIgnoreCase);
            if (!isChatContainer || !pairsByBlob.TryGetValue(candidate.Id, out var linkedMessages) || linkedMessages.Count == 0)
            {
                analysis.UnclassifiedCount++;
                continue;
            }

            analysis.ConfirmedChatBlobIds.Add(candidate.Id);
            analysis.LinkedCountByBlob[candidate.Id] = linkedMessages.Count;
            foreach (var (msgId, sessionId) in linkedMessages)
            {
                if (!existingRefSet.Contains((candidate.Id, msgId)))
                {
                    analysis.MissingRefs.Add((candidate.Id, msgId, sessionId));
                }
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        return analysis;
    }
}
