using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Caching;

namespace IczpNet.Chat.MessageSections.Messages;

public interface IMessageManager
{
    IDistributedCache<MessageCacheItem, MessageCacheKey> MessageCache { get; }

    /// <summary>
    /// 创建缓存
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="minMessageId"></param>
    /// <param name="maxMessageId"></param>
    /// <param name="max"></param>
    /// <param name="batchSize"></param>
    /// <returns></returns>
    Task<List<long>> BuildCacheAsync(Guid sessionId, long? minMessageId, long? maxMessageId, int max = 5000, int batchSize = 1000);

    /// <summary>
    /// 创建缓存(全部)
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="minMessageId"></param>
    /// <param name="max"></param>
    /// <param name="batchSize"></param>
    /// <returns></returns>
    Task<List<long>> BuildAllCacheAsync(Guid sessionId, long? minMessageId, int max = 5000, int batchSize = 1000);

    /// <summary>
    /// 移除消息缓存
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task<bool> RemoveCacheAsync(Guid sessionId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="minMessageId"></param>
    /// <param name="maxResultCount"></param>
    /// <returns></returns>
    Task<List<long>> GetLatestAsync(SessionUnitCacheItem unit, long minMessageId, int maxResultCount = 20);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="maxMessageId"></param>
    /// <param name="maxResultCount"></param>
    /// <returns></returns>
    Task<List<long>> GetHistoryAsync(SessionUnitCacheItem unit, long? maxMessageId, int maxResultCount = 20);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="minMessageId"></param>
    /// <param name="maxMessageId"></param>
    /// <returns></returns>
    Task<long> GetSessionMessageTotalCountAsync(SessionUnitCacheItem unit, long minMessageId = 0, long maxMessageId = long.MaxValue);

    /// <summary>
    /// 创建消息
    /// </summary>
    /// <param name="senderSessionUnit"></param>
    /// <param name="action"></param>
    /// <param name="clientMessageId"></param>
    /// <param name="receiverSessionUnitId"></param>
    /// <param name="quoteMessageId"></param>
    /// <param name="remindList"></param>
    /// <returns></returns>
    Task<Message> CreateMessageAsync(SessionUnitCacheItem senderSessionUnit,
        Func<Message, Task<IContentEntity>> action,
        string clientMessageId = null,
        Guid? receiverSessionUnitId = null,
        long? quoteMessageId = null,
        List<Guid> remindList = null);

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <typeparam name="TContentInfo"></typeparam>
    /// <typeparam name="TContentEntity"></typeparam>
    /// <param name="senderSessionUnit"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<MessageInfo<TContentInfo>> SendAsync<TContentInfo, TContentEntity>(
        SessionUnitCacheItem senderSessionUnit,
        MessageInput<TContentInfo> input)
        where TContentInfo : IContentInfo
        where TContentEntity : IContentEntity;

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <typeparam name="TContentInfo"></typeparam>
    /// <typeparam name="TContentEntity"></typeparam>
    /// <param name="senderSessionUnit"></param>
    /// <param name="input"></param>
    /// <param name="contentEntity"></param>
    /// <returns></returns>
    Task<MessageInfo<TContentInfo>> SendAsync<TContentInfo, TContentEntity>(
        SessionUnitCacheItem senderSessionUnit,
        MessageInput input,
        TContentEntity contentEntity)
        where TContentInfo : IContentInfo
        where TContentEntity : IContentEntity;

    /// <summary>
    /// 转发消息
    /// </summary>
    /// <param name="currentSessionUnitId"></param>
    /// <param name="sourceMessageId"></param>
    /// <param name="targetSessionUnitIdList"></param>
    /// <returns></returns>
    Task<List<Message>> ForwardAsync(Guid currentSessionUnitId, long sourceMessageId, List<Guid> targetSessionUnitIdList);

    /// <summary>
    /// 回滚消息
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<Dictionary<string, long>> RollbackAsync(Message message);

    /// <summary>
    /// 是否提醒 @我
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="sessionUnitId"></param>
    /// <returns></returns>
    Task<bool> IsRemindAsync(long messageId, Guid sessionUnitId);

    /// <summary>
    /// 提醒 @我 的消息Id
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="messageIdList"></param>
    /// <returns></returns>
    Task<List<long>> GetRemindMessageIdListAsync(Guid sessionUnitId, List<long> messageIdList);

    /// <summary>
    /// 缓存消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="senderSessionUnit"></param>
    /// <param name="options"></param>
    /// <param name="hideErrors"></param>
    /// <param name="considerUow"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<MessageCacheItem> SetCacheAsync(
        Message message,
        SessionUnitCacheItem senderSessionUnit = null,
        DistributedCacheEntryOptions options = null,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default);

    /// <summary>
    /// 获取消息缓存
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="hideErrors"></param>
    /// <param name="considerUow"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<MessageCacheItem> GetCacheAsync(
        long messageId,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default);

    /// <summary>
    /// 获取消息缓存
    /// </summary>
    /// <param name="messageIds"></param>
    /// <param name="hideErrors"></param>
    /// <param name="considerUow"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<KeyValuePair<MessageCacheKey, MessageCacheItem>[]> GetManyCacheAsync(
        IEnumerable<long> messageIds,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default);

    /// <summary>
    /// 获取消息缓存
    /// </summary>
    /// <param name="messageIds"></param>
    /// <param name="optionsFactory"></param>
    /// <param name="hideErrors"></param>
    /// <param name="considerUow"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<KeyValuePair<MessageCacheKey, MessageCacheItem>[]> GetOrAddManyCacheAsync(
        IEnumerable<long> messageIds,
        Func<DistributedCacheEntryOptions> optionsFactory = null,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default);

    /// <summary>
    /// 获取会话最大消息Id
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task<long> GetMaxMessageIdAsync(Guid sessionId);

    /// <summary>
    /// 设置会话最大消息Id
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="messageId"></param>
    /// <returns></returns>
    Task SetMaxMessageIdAsync(Guid sessionId, long messageId);

}
