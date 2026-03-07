using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionTags;
using IczpNet.Chat.SessionUnitSettings.Dtos;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitMemberDto 
{

    /// <summary>
    /// 
    /// </summary>
    public virtual Guid Id { get; set; }

    /// <summary>
    /// 会话Id
    /// </summary>
    public virtual Guid? SessionId { get; set; }

    /// <summary>
    /// OwnerId
    /// </summary>
    public virtual long OwnerId { get; set; }

    /// <summary>
    /// OwnerType
    /// </summary>
    public virtual ChatObjectTypeEnums? OwnerObjectType { get; set; }

    /// <summary>
    /// 会话内名称
    /// </summary>
    public virtual string MemberName { get; set; }

    /// <summary>
    /// DestinationId
    /// </summary>
    public virtual long? DestinationId { get; set; }

    /// <summary>
    /// DestinationType
    /// </summary>
    public virtual ChatObjectTypeEnums? DestinationObjectType { get; set; }

    /// <summary>
    /// Ticks
    /// </summary>
    public virtual double Ticks { get; set; }

    /// <summary>
    /// Sorting
    /// </summary>
    public virtual double Sorting { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual DateTime CreationTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual ChatObjectInfo Owner { get; set; }

    /// <summary>
    /// 好友关系
    /// </summary>
    public virtual SessionUnitFriendshipDto Friendship { get; set; }

    /// <summary>
    /// 设置
    /// </summary>
    public virtual SessionUnitMemberSettingDto Setting { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public virtual List<SessionTagCacheItem> TagList { get; set; }

}
