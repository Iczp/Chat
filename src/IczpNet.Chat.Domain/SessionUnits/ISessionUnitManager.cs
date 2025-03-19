using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionUnits;

public interface ISessionUnitManager
{
    /// <summary>
    /// 获取会话单元
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="isReadOnly"></param>
    /// <returns></returns>
    Task<SessionUnit> GetAsync(Guid sessionUnitId, bool isReadOnly = true);

    /// <summary>
    /// 获取会话单元
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <returns></returns>
    Task<SessionUnitCacheItem> GetByCacheAsync(Guid sessionUnitId);

    /// <summary>
    /// 获取会话单元
    /// </summary>
    /// <param name="idList"></param>
    /// <returns></returns>
    Task<List<SessionUnit>> GetManyAsync(List<Guid> idList);

    /// <summary>
    /// 查找会话单元
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Task<SessionUnit> FindAsync(Expression<Func<SessionUnit, bool>> predicate);

    /// <summary>
    /// 查找会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="destinactionId"></param>
    /// <returns></returns>
    Task<SessionUnit> FindAsync(long ownerId, long destinactionId);

    /// <summary>
    /// 创建会话单元
    /// </summary>
    /// <param name="session"></param>
    /// <param name="owner"></param>
    /// <param name="destination"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    SessionUnit Create(Session session, ChatObject owner, ChatObject destination, Action<SessionUnitSetting> action);

    /// <summary>
    /// 创建会话单元
    /// </summary>
    /// <param name="session"></param>
    /// <param name="owner"></param>
    /// <param name="destination"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    SessionUnit Generate(Session session, ChatObject owner, ChatObject destination, Action<SessionUnitSetting> action);

    /// <summary>
    /// 创建会话单元
    /// </summary>
    /// <param name="sessionUnit"></param>
    /// <returns></returns>
    Task<SessionUnit> CreateIfNotContainsAsync(SessionUnit sessionUnit);

    /// <summary>
    /// 创建会话单元
    /// </summary>
    /// <param name="session"></param>
    /// <param name="owner"></param>
    /// <param name="destination"></param>
    /// <param name="setting"></param>
    /// <returns></returns>
    Task<SessionUnit> CreateIfNotContainsAsync(Session session, ChatObject owner, ChatObject destination, Action<SessionUnitSetting> setting);

    /// <summary>
    /// 查找会话单元Id
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    Task<SessionUnit> FindBySessionIdAsync(Guid sessionId, long ownerId);

    /// <summary>
    /// 查找会话单元Id
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="destinactionId"></param>
    /// <returns></returns>
    Task<Guid?> FindIdAsync(long ownerId, long destinactionId);

    /// <summary>
    /// 是否存在
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="destinactionId"></param>
    /// <returns></returns>
    Task<bool> IsAnyAsync(long ownerId, long destinactionId);

    /// <summary>
    /// 查找
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Task<Guid?> FindIdAsync(Expression<Func<SessionUnit, bool>> predicate);

    /// <summary>
    /// 设置成员名称
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="memberName"></param>
    /// <returns></returns>
    Task<SessionUnit> SetMemberNameAsync(SessionUnit entity, string memberName);

    /// <summary>
    /// 设置重命名
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="rename"></param>
    /// <returns></returns>
    Task<SessionUnit> SetRenameAsync(SessionUnit entity, string rename);

    /// <summary>
    /// 设置置顶
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="isTopping"></param>
    /// <returns></returns>
    Task<SessionUnit> SetToppingAsync(SessionUnit entity, bool isTopping);

    /// <summary>
    /// 设置已读的消息Id
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="isForce"></param>
    /// <param name="messageId"></param>
    /// <returns></returns>
    Task<SessionUnit> SetReadedMessageIdAsync(SessionUnit entity, bool isForce = false, long? messageId = null);

    /// <summary>
    /// 设置是否为沉浸式
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    Task<SessionUnit> SetImmersedAsync(SessionUnit entity, bool isImmersed);

    /// <summary>
    /// 设置是否为联系人
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="isContacts"></param>
    /// <returns></returns>
    Task<SessionUnit> SetIsContactsAsync(SessionUnit entity, bool isContacts);

    /// <summary>
    /// 设置是否显示成员名称
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="isShowMemberName"></param>
    /// <returns></returns>
    Task<SessionUnit> SetIsShowMemberNameAsync(SessionUnit entity, bool isShowMemberName);

    /// <summary>
    /// 移除会话
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<SessionUnit> RemoveAsync(SessionUnit entity);

    /// <summary>
    /// 删除会话
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<SessionUnit> KillAsync(SessionUnit entity);

    /// <summary>
    /// 清空消息
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<SessionUnit> ClearMessageAsync(SessionUnit entity);

    /// <summary>
    /// 删除消息
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="messageId"></param>
    /// <returns></returns>
    Task<SessionUnit> DeleteMessageAsync(SessionUnit entity, long messageId);

    /// <summary>
    /// 获取聊天对象角标
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    Task<Dictionary<ChatObjectTypeEnums, int>> GetTypeBadgeByOwnerIdAsync(long ownerId, bool? isImmersed = null);

    /// <summary>
    /// 获取聊天对象角标
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    Task<int> GetBadgeByOwnerIdAsync(long ownerId, bool? isImmersed = null);

    /// <summary>
    /// 获取聊天对象角标
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    Task<int> GetBadgeByIdAsync(Guid sessionUnitId, bool? isImmersed = null);

    /// <summary>
    /// 获取聊天对象角标  
    /// </summary>
    /// <param name="sessionUnitIdList"></param>
    /// <param name="minMessageId"></param>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    Task<Dictionary<Guid, int>> GetBadgeByIdAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null);

    /// <summary>
    /// 获取计数器信息
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="minMessageId"></param>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    Task<SessionUnitCounterInfo> GetCounterAsync(Guid sessionUnitId, long minMessageId = 0, bool? isImmersed = null);

    /// <summary>
    /// 获取统计信息
    /// </summary>
    /// <param name="sessionUnitIdList"></param>
    /// <param name="minMessageId"></param>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    Task<Dictionary<Guid, SessionUnitStatModel>> GetStatsAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null);

    /// <summary>
    /// 获取提醒器数量
    /// </summary>
    /// <param name="sessionUnitIdList"></param>
    /// <param name="minMessageId"></param>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    Task<Dictionary<Guid, int>> GetReminderCountByIdAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null);

    /// <summary>
    /// 获取关注数量
    /// </summary>
    /// <param name="sessionUnitIdList"></param>
    /// <param name="minMessageId"></param>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    Task<Dictionary<Guid, int>> GetFollowingCountByIdAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null);

    /// <summary>
    /// 获取聊天对象的会话数量
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task<int> GetCountBySessionIdAsync(Guid sessionId);

    /// <summary>
    /// 获取聊天对象的会话数量
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    Task<int> GetCountByOwnerIdAsync(long ownerId);

    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <returns></returns>
    Task<List<SessionUnitCacheItem>> GetCacheListAsync(string cacheKey);

    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task<List<SessionUnitCacheItem>> GetOrAddCacheListAsync(Guid sessionId);

    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <param name="sessionUnitList"></param>
    /// <returns></returns>
    Task<List<SessionUnitCacheItem>> GetOrAddCacheListAsync(List<SessionUnit> sessionUnitList);

    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task<List<SessionUnitCacheItem>> GetCacheListBySessionIdAsync(Guid sessionId);

    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="sessionUnitId"></param>
    /// <returns></returns>
    Task<SessionUnitCacheItem> GetCacheItemAsync(Guid sessionId, Guid sessionUnitId);

    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <param name="sessionUnit"></param>
    /// <returns></returns>
    Task<SessionUnitCacheItem> GetCacheItemAsync(SessionUnit sessionUnit);

    /// <summary>
    /// 设置缓存
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="sessionUnitList"></param>
    /// <returns></returns>
    Task SetCacheListBySessionIdAsync(Guid sessionId, List<SessionUnitCacheItem> sessionUnitList);

    /// <summary>
    /// 获取缓存Key(消息)
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<string> GetCacheKeyByMessageAsync(Message message);

    /// <summary>
    /// 设置缓存
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<List<SessionUnitCacheItem>> GetOrAddByMessageAsync(Message message);

    /// <summary>
    /// 设置缓存
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="sessionUnitList"></param>
    /// <param name="options"></param>
    /// <param name="hideErrors"></param>
    /// <param name="considerUow"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task SetCacheListAsync(string cacheKey, List<SessionUnitCacheItem> sessionUnitList, DistributedCacheEntryOptions options = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default);

    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task<List<SessionUnitCacheItem>> GetListBySessionIdAsync(Guid sessionId);

    /// <summary>
    /// 移除缓存
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task RemoveCacheListBySessionIdAsync(Guid sessionId);

    /// <summary>
    /// 查询相同会话
    /// </summary>
    /// <param name="sourceChatObjectId"></param>
    /// <param name="targetChatObjectId"></param>
    /// <param name="chatObjectTypeList"></param>
    /// <returns></returns>
    Task<IQueryable<SessionUnit>> GetSameSessionQeuryableAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null);

    /// <summary>
    /// 获取相同会话
    /// </summary>
    /// <param name="sourceChatObjectId"></param>
    /// <param name="targetChatObjectId"></param>
    /// <param name="chatObjectTypeList"></param>
    /// <returns></returns>
    Task<int> GetSameSessionCountAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null);

    /// <summary>
    /// 查询相同目标（好友、群）
    /// </summary>
    /// <param name="sourceChatObjectId"></param>
    /// <param name="targetChatObjectId"></param>
    /// <param name="chatObjectTypeList"></param>
    /// <returns></returns>
    Task<IQueryable<SessionUnit>> GetSameDestinationQeuryableAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null);

    /// <summary>
    /// 获取相同目标（好友、群）数量
    /// </summary>
    /// <param name="sourceChatObjectId"></param>
    /// <param name="targetChatObjectId"></param>
    /// <param name="chatObjectTypeList"></param>
    /// <returns></returns>
    Task<int> GetSameDestinationCountAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null);

    /// <summary>
    /// 自增关注数量 +1
    /// </summary>
    /// <param name="senderSessionUnit"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<int> IncrementFollowingCountAsync(SessionUnit senderSessionUnit, Message message);

    ///// <summary>
    ///// 更新缓存
    ///// </summary>
    ///// <param name="senderSessionUnit"></param>
    ///// <param name="message"></param>
    ///// <returns></returns>
    //Task<int> UpdateCachesAsync(SessionUnit senderSessionUnit, Message message);

    /// <summary>
    /// 批量更新
    /// </summary>
    /// <param name="senderSessionUnit"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<int> BatchUpdateAsync(SessionUnit senderSessionUnit, Message message);

    /// <summary>
    /// 获取SessionUnitId
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="nameList"></param>
    /// <returns></returns>
    Task<List<Guid>> GetIdListByNameAsync(Guid sessionId, List<string> nameList);

    /// <summary>
    /// 角标自增 +1
    /// </summary>
    /// <param name="args"></param>
    /// <returns>受影响的行数</returns>
    Task<int> IncremenetAsync(SessionUnitIncrementJobArgs args);

    /// <summary>
    /// 设置禁言过期时间
    /// </summary>
    /// <param name="muterSessionUnit"></param>
    /// <param name="muteExpireTime"></param>
    /// <param name="setterSessionUnit"></param>
    /// <param name="isSendMessage"></param>
    /// <returns></returns>
    Task<DateTime?> SetMuteExpireTimeAsync(SessionUnit muterSessionUnit, DateTime? muteExpireTime, SessionUnit setterSessionUnit, bool isSendMessage);

    /// <summary>
    /// 设置禁言过期时间
    /// </summary>
    /// <param name="muterSessionUnit"></param>
    /// <param name="muteExpireTime"></param>
    /// <returns></returns>
    Task<DateTime?> SetMuteExpireTimeAsync(SessionUnit muterSessionUnit, DateTime? muteExpireTime);

}
