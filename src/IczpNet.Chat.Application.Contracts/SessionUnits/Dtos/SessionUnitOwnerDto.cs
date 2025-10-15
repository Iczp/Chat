using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.SessionUnitSettings.Dtos;
using System;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitOwnerDto : SessionUnitDto
{
    public virtual ChatObjectDto Destination { get; set; }

    public virtual SessionUnitSettingDto Setting { get; set; }

    public virtual MessageDto LastMessage { get; set; }
    //public virtual MessageSimpleDto LastMessage { get; set; }

    public virtual long? LastMessageId { get; set; }

    public virtual int PublicBadge { get; set; }

    public virtual int PrivateBadge { get; set; }

    public virtual int RemindAllCount { get; set; }

    public virtual int RemindMeCount { get; set; }

    public virtual int FollowingCount { get; set; }

    public virtual bool IsWaiter { get; set; }

    public virtual double Sorting { get; set; }

    public virtual double Ticks { get; set; }

    /// <summary>
    /// CreationTime
    /// </summary>
    public virtual DateTime CreationTime { get; set; }
}
