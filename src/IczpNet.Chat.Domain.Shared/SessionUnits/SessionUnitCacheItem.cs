using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.SessionUnits;

[Serializable]
public class SessionUnitCacheItem : SessionUnitInfoBase, ISessionUnit
{
    /// <summary>
    /// BoxId
    /// </summary>
    public virtual Guid? BoxId { get; set; }

    /// <summary>
    /// 备注名称
    /// </summary>
    public virtual string Rename { get; set; }

    /// <summary>
    /// DestinationId
    /// </summary>
    public virtual long? DestinationId { get; set; }

    /// <summary>
    /// DestinationType
    /// </summary>
    public virtual ChatObjectTypeEnums? DestinationObjectType { get; set; }

    //public virtual long? ReadedMessageId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual long? LastMessageId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual int PublicBadge { get; set; }

    /// <summary>
    /// PrivateBadge
    /// </summary>
    public virtual int PrivateBadge { get; set; }

    /// <summary>
    /// RemindAllCount
    /// </summary>
    public virtual int RemindAllCount { get; set; }

    /// <summary>
    /// RemindMeCount
    /// </summary>
    public virtual int RemindMeCount { get; set; }

    /// <summary>
    /// FollowingCount
    /// </summary>
    public virtual int FollowingCount { get; set; }

    /// <summary>
    /// Ticks
    /// </summary>
    public virtual double Ticks { get; set; }

    /// <summary>
    /// Sorting
    /// </summary>
    public virtual double Sorting { get; set; }

    /// <summary>
    /// IsImmersed
    /// </summary>
    public virtual bool IsImmersed { get; set; }

    /// <summary>
    /// LastSendTime
    /// </summary>
    public DateTime? LastSendTime { get; set; }

    /// <summary>
    /// LastSendMessageId
    /// </summary>
    public virtual long? LastSendMessageId { get; set; }

    //public virtual SessionUnitSettingCacheItem Setting { get; set; }
}
