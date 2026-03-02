using IczpNet.Chat.ChatObjects;
using System;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitMemberDetailDto : SessionUnitCacheItem
{

    /// <summary>
    /// 是否好友
    /// </summary>
    public virtual bool? IsFriendship { get; set; }

    /// <summary>
    /// 好友名称
    /// </summary>
    public virtual string FriendshipName { get; set; }

    /// <summary>
    /// 好友会话Id
    /// </summary>
    public virtual Guid? FriendshipSessionUnitId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual ChatObjectInfo Destination { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual ChatObjectInfo Owner { get; set; }


}
