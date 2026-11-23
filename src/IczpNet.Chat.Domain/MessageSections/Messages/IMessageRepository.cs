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
    /// 使用无跟踪批量更新与参数化 SQL 绑定 Blob 状态、引用计数与 BlobMessageRef 授权记录，避免触发 ABP 实体版本误递增
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
    /// 使用参数化 SQL 无跟踪批量写入 BlobMessageRef 授权记录，避免触发关联 Message 实体状态变更
    /// </summary>
    Task InsertBlobMessageRefsAsync(
        IReadOnlyCollection<(Guid BlobId, long MessageId, Guid? SessionId, BlobRefSourceTypes SourceType)> refs,
        Guid? creatorId = null,
        DateTime? now = null,
        CancellationToken cancellationToken = default);

    //Task<int> IncrementRecorderAsync(List<long> messageIdList);

}
