using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Blobs;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.MessageSections.Messages;

public interface IMessageRepository : IRepository<Message, long>
{
    Task<SessionMessageWindowCacheItem> GetReadWindowAsync(System.Guid sessionId, long minMessageId,
        long maxMessageId, int batchSize, bool isDescending, System.Threading.CancellationToken cancellationToken = default);

    Task<List<long>> GetVisibleIdsAsync(System.Guid sessionUnitId, long? minMessageId, long? maxMessageId,
        int maxResultCount, bool isDescending, System.Threading.CancellationToken cancellationToken = default);

    Task<long> CountVisibleAsync(System.Guid sessionUnitId, long? minMessageId, long? maxMessageId,
        System.Threading.CancellationToken cancellationToken = default);

    Task<bool> CanReadAsync(System.Guid sessionUnitId, long messageId, bool includeReferences = false,
        System.Threading.CancellationToken cancellationToken = default);
    /// <summary>
    /// 增加已读次数 +1
    /// </summary>
    /// <param name="messageIdList"></param>
    /// <returns></returns>
    Task<int> IncrementReadedCountAsync(List<long> messageIdList);

    /// <summary>
    /// 增加打开次数 +1
    /// </summary>
    /// <param name="messageIdList"></param>
    /// <returns></returns>
    Task<int> IncrementOpenedCountAsync(List<long> messageIdList);

    /// <summary>
    /// 增加收藏次数 +1
    /// </summary>
    /// <param name="messageIdList"></param>
    /// <returns></returns>
    Task<int> IncrementFavoritedCountAsync(List<long> messageIdList);

    /// <summary>
    /// 增加引用次数 +1
    /// </summary>
    /// <param name="messageIdList"></param>
    /// <returns></returns>
    Task<int> IncrementQuoteCountAsync(List<long> messageIdList);

    /// <summary>
    /// 增加转发次数 +1
    /// </summary>
    /// <param name="messageIdList"></param>
    /// <returns></returns>
    Task<int> IncrementForwardCountAsync(List<long> messageIdList);

    /// <summary>
    /// 删除次数 +1
    /// </summary>
    /// <param name="messageIdList"></param>
    /// <returns></returns>
    Task<int> IncrementDeletedCountAsync(List<long> messageIdList);

    /// <summary>
    /// 无跟踪批量查询 Blob，避免附加到 ChangeTracker 触发主实体关联副作用
    /// </summary>
    Task<List<Blob>> GetBlobsAsNoTrackingAsync(
        IReadOnlyCollection<Guid> blobIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量原子更新 Blob 状态与引用计数，并通过 ABP 仓储写入 BlobMessageRef 授权记录
    /// </summary>
    Task BindBlobsToMessageAsync(
        long messageId,
        Guid? sessionId,
        IReadOnlyCollection<Guid> blobIds,
        BlobRefSourceTypes sourceType,
        Guid? creatorId = null,
        DateTime? now = null,
        bool markAsBound = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 通过 ABP 仓储批量写入 BlobMessageRef 授权记录，保留实体审计与 EF Core 表映射
    /// </summary>
    Task InsertBlobMessageRefsAsync(
        IReadOnlyCollection<(Guid BlobId, long MessageId, Guid? SessionId, BlobRefSourceTypes SourceType)> refs,
        Guid? creatorId = null,
        DateTime? now = null,
        CancellationToken cancellationToken = default);

    //Task<int> IncrementRecorderAsync(List<long> messageIdList);

}
