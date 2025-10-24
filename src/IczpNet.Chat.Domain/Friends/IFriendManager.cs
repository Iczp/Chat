using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.Friends;

/// <summary>
/// 好友管理
/// </summary>
public interface IFriendManager
{
    /// <summary>
    /// 获取我的朋友(用户)
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <returns></returns>
    Task<List<SessionUnitCacheItem>> GetFriendsAsync(Guid userId);

    /// <summary>
    /// 获取我的朋友(ChatObject)
    /// </summary>
    /// <param name="chatObjectId">聊天对象Id</param>
    /// <returns></returns>
    Task<List<SessionUnitCacheItem>> GetFriendsAsync(long chatObjectId);
}
