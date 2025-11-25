using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.SessionSections.SessionUnits;

[Serializable]
public class SessionUnitCacheItem : ISessionUnit
{
    /// <summary>
    /// SessionUnitId
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
    /// DestinationId
    /// </summary>
    public virtual long? DestinationId { get; set; }

    /// <summary>
    /// DestinationType
    /// </summary>
    public virtual ChatObjectTypeEnums? DestinationObjectType { get; set; }

    /// <summary>
    /// 是否固定成员
    /// </summary>
    public virtual bool IsStatic { get; set; }

    /// <summary>
    /// 是否公开
    /// </summary>
    public virtual bool IsPublic { get; set; }

    /// <summary>
    /// 是否可见
    /// </summary>
    public virtual bool IsVisible { get; set; }

    /// <summary>
    /// 是否可用
    /// </summary>
    public virtual bool IsEnabled { get; set; }

    public virtual long? ReadedMessageId { get; set; }

    public virtual long? LastMessageId { get; set; }

    public virtual int PublicBadge { get; set; }

    public virtual int PrivateBadge { get; set; }

    public virtual int RemindAllCount { get; set; }

    public virtual int RemindMeCount { get; set; }

    public virtual int FollowingCount { get; set; }

    public virtual double Ticks { get; set; }

}
