using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.SessionSections.Friendships.Dtos;

public class FriendshipGetListInput : BaseGetListInput
{
    public virtual Guid? OwnerId { get; set; }

    public virtual Guid? FriendId { get; set; }

    public virtual bool? IsCantacts { get; set; }

    /// <summary>
    /// 消息免打扰，默认为 false
    /// </summary>
    public virtual bool? IsImmersed { get; set; }

    public virtual DateTime? StartCreationTime { get; set; }

    public virtual DateTime? EndCreationTime { get; set; }
}
