using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.SessionSections.SessionUnits;

[Serializable]
public class SessionUnitCacheItem : SessionUnitInfoBase, ISessionUnit
{
    /// <summary>
    /// 备注名称
    /// </summary>
    public virtual string Rename { get; set; }

    /// <summary>
    /// DestinationId
    /// </summary>
    public virtual long? DestinationId { get; set; }

    /// <summary>
    /// BoxId
    /// </summary>
    public virtual Guid? BoxId { get; set; }

    /// <summary>
    /// DestinationType
    /// </summary>
    public virtual ChatObjectTypeEnums? DestinationObjectType { get; set; }

    //public virtual long? ReadedMessageId { get; set; }

    public virtual long? LastMessageId { get; set; }

    public virtual int PublicBadge { get; set; }

    public virtual int PrivateBadge { get; set; }

    public virtual int RemindAllCount { get; set; }

    public virtual int RemindMeCount { get; set; }

    public virtual int FollowingCount { get; set; }

    public virtual double Ticks { get; set; }

    public virtual double Sorting { get; set; }

    //public virtual SessionUnitSettingCacheItem Setting { get; set; }

    /// <summary>
    /// Setting.IsImmersed
    /// </summary>
    public virtual bool IsImmersed { get; set; }
}
