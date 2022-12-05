using IczpNet.Chat.BaseDtos;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.Friendships.Dtos;

public class FriendshipGetListInput : BaseGetListInput
{
    public virtual List<Guid> TagIdList { get; set; }

    public virtual Guid? OwnerId { get; set; }

    public virtual Guid? DestinationId { get; set; }

    public virtual bool? IsCantacts { get; set; }

    public virtual bool? IsPassive { get; set; }

    /// <summary>
    /// 消息免打扰，默认为 false
    /// </summary>
    public virtual bool? IsImmersed { get; set; }

    public virtual DateTime? StartCreationTime { get; set; }

    public virtual DateTime? EndCreationTime { get; set; }
}
