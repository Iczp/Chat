using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionUnits;

public interface ISessionUnitCacheManager
{
    /// <summary>
    /// 设置会话成员
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    Task<IEnumerable<SessionUnitCacheItem>> SetMembersAsync(Guid sessionId, IEnumerable<SessionUnitCacheItem> units);

    /// <summary>
    /// 设置会话成员
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="fetchTask"></param>
    /// <returns></returns>
    Task<IEnumerable<SessionUnitCacheItem>> SetMembersAsync(Guid sessionId, Func<Guid, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask);

    /// <summary>
    /// 设置会话成员（仅当不存在时）
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="fetchTask"></param>
    /// <returns></returns>
    Task<IEnumerable<SessionUnitCacheItem>> SetMembersIfNotExistsAsync(Guid sessionId, Func<Guid, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask);

    /// <summary>
    /// 获取或设置会话成员
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="fetchTask"></param>
    /// <returns></returns>
    Task<IEnumerable<SessionUnitCacheItem>> GetOrSetMembersAsync(Guid sessionId, Func<Guid, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask);

    /// <summary>
    /// 获取会话成员映射
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns>{Key:SessionUnitId, Value:OwnerId}</returns>
    Task<IDictionary<Guid, long>> GetMembersMapAsync(Guid sessionId);

    /// <summary>
    /// 获取会话成员数量
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task<long> GetMembersCountAsync(Guid sessionId);

    /// <summary>
    /// 获取会话成员
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="minScore"></param>
    /// <param name="maxScore"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <param name="isDescending"></param>

    Task<IEnumerable<MemberModel>> GetMembersAsync(
        Guid sessionId,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true);

    /// <summary>
    /// 获取会话成员
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="minScore"></param>
    /// <param name="maxScore"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <param name="isDescending"></param>
    Task<IEnumerable<SessionUnitCacheItem>> GetMemberUnitsAsync(
        Guid sessionId,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true);

    /// <summary>
    /// 获取会话置顶映射
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task<IDictionary<long, double>> GetToppingBySessionAsync(Guid sessionId);

    /// <summary>
    /// 获取会话免打扰映射
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task<IDictionary<long, bool>> GetImmersedBySessionAsync(Guid sessionId);

    /// <summary>
    /// 设置好友会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    Task<IEnumerable<SessionUnitCacheItem>> SetFriendsAsync(long ownerId, IEnumerable<SessionUnitCacheItem> units);

    /// <summary>
    /// 设置好友会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="fetchTask"></param>
    /// <returns></returns>
    Task<IEnumerable<SessionUnitCacheItem>> SetFriendsAsync(long ownerId, Func<long, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask);

    /// <summary>
    /// 设置好友会话单元（仅当不存在时）
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="fetchTask"></param>
    /// <returns></returns>
    Task<IEnumerable<SessionUnitCacheItem>> SetFriendsIfNotExistsAsync(long ownerId, Func<long, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask);

    /// <summary>
    /// 获取或设置好友会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="fetchTask"></param>
    /// <returns></returns>
    Task<IEnumerable<SessionUnitCacheItem>> GetOrSetFriendsAsync(long ownerId, Func<long, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask);

    /// <summary>
    /// 获取好友会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="minScore"></param>
    /// <param name="maxScore"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <param name="isDescending"></param>
    /// <returns></returns>
    Task<IEnumerable<SessionUnitCacheItem>> GetFriendUnitsAsync(
        long ownerId,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true);


    /// <summary>
    /// 获取好友会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="minScore"></param>
    /// <param name="maxScore"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <param name="isDescending"></param>
    /// <returns></returns>
    Task<IEnumerable<FriendModel>> GetFriendsAsync(
        long ownerId,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true);

    /// <summary>
    /// 获取好友会话单元数量
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    Task<long> GetFirendsCountAsync(long ownerId);

    /// <summary>
    /// 获取会话单元(多个)
    /// </summary>
    /// <param name="unitIds"></param>
    /// <returns></returns>
    Task<KeyValuePair<Guid, SessionUnitCacheItem>[]> GetManyAsync(IEnumerable<Guid> unitIds);

    /// <summary>
    /// 获取会话单元(单个)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<SessionUnitCacheItem> GetAsync(Guid id);

    /// <summary>
    /// 获取或设置会话单元(多个)
    /// </summary>
    /// <param name="unitIds"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    Task<KeyValuePair<Guid, SessionUnitCacheItem>[]> GetOrSetManyAsync(IEnumerable<Guid> unitIds, Func<List<Guid>, Task<KeyValuePair<Guid, SessionUnitCacheItem>[]>> func);

    /// <summary>
    /// 批量自增计数器(核心)
    /// </summary>
    /// <param name="message"></param>
    /// <param name="expire"></param>
    /// <returns></returns>
    Task BatchIncrementAsync(Message message, TimeSpan? expire = null);

    /// <summary>
    /// 更新会话单元计数器信息
    /// </summary>
    /// <param name="counter"></param>
    /// <param name="fetchTask"></param>
    /// <returns></returns>
    Task UpdateCounterAsync(SessionUnitCounterInfo counter, Func<Guid, Task<SessionUnitCacheItem>> fetchTask);

    /// <summary>
    /// 获取统计信息
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    Task<SessionUnitStatistic> GetStatisticAsync(long ownerId);

    /// <summary>
    /// 移除统计信息
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    Task<bool> RemoveStatisticAsync(long ownerId);

    /// <summary>
    /// 获取原始角标
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    Task<Dictionary<string, long>> GetRawBadgeMapAsync(long ownerId);

    /// <summary>
    /// 获取角标
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    Task<Dictionary<ChatObjectTypeEnums, long>> GetBadgeMapAsync(long ownerId);

    /// <summary>
    /// 设置置顶
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="unitId"></param>
    /// <param name="ownerId"></param>
    /// <param name="sorting"></param>
    /// <returns></returns>
    Task SetToppingAsync(Guid sessionId, Guid unitId, long ownerId, long sorting);

    /// <summary>
    /// 变更免打扰状态
    /// </summary>
    /// <param name="unitId"></param>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    Task ChangeImmersedAsync(Guid unitId, bool isImmersed);

}