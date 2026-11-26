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
    /// 校验附件访问权限（消息可见、未撤回，且当同时传入 rootMessageId 与 messageId 时两者均未撤回）
    /// </summary>
    Task<bool> CanReadAttachmentAsync(Guid sessionUnitId, long targetMessageId, long? childMessageId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 原子设置视频内容的 GIF 信息（仅当 GifBlobId 为空时更新成功，防止并发 Job 重复发布）
    /// </summary>
    Task<bool> TrySetVideoGifAsync(Guid videoContentId, Guid gifBlobId, string gifUrl,
        CancellationToken cancellationToken = default);
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
    /// 跨会话检索当前账号在任一未删且可见消息（含历史兼容结构化附件表）中是否有权限访问指定 BlobId
    /// </summary>
    Task<bool> HasVisibleBlobForOwnersAsync(Guid blobId, IReadOnlyCollection<long> ownerIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量原子更新 Blob 状态与引用计数，并通过 ABP 仓储写入 BlobMessageRef 授权记录；返回实际新增的引用数量
    /// </summary>
    Task<int> BindBlobsToMessageAsync(
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

    /// <summary>
    /// 缓存未命中时无跟踪批量回源消息主表、引用消息及对应内容表，避免 EF Core 懒加载代理引发 N+1 查询与全量多表笛卡尔积。
    /// </summary>
    Task<List<Message>> GetCacheMissMessagesAsync(
        IReadOnlyCollection<long> messageIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 按批次内实际出现的 MessageType 分组，无跟踪批量填充消息内容子表（及单层引用消息内容）。
    /// </summary>
    Task LoadMessageContentsBatchAsync(
        IReadOnlyCollection<Message> messages,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 插入转发消息并关联已存在的共享内容实体（不重复插入内容实体及其关联的源消息）
    /// </summary>
    Task<Message> InsertForwardedMessageAsync(Message message, IContentEntity existingContent, CancellationToken cancellationToken = default);

    //Task<int> IncrementRecorderAsync(List<long> messageIdList);

}
