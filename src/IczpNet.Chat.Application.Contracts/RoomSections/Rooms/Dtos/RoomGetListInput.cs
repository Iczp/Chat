using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.RoomSections.Rooms.Dtos;

public class RoomGetListInput : BaseGetListInput
{
    /// <summary>
    /// 群主
    /// </summary>
    public virtual Guid? OwnerId { get; set; }

    public virtual RoomTypeEnum? Type { get; set; }

    /// <summary>
    /// 是否全体禁言
    /// </summary>
    public virtual bool? IsForbiddenAll { get; set; }

    /// <summary>
    /// 成员所在的群(我加入的群)
    /// </summary>
    public virtual Guid? MemberOwnerId { get; set; }

    /// <summary>
    /// 成员被禁言的群
    /// </summary>
    public virtual Guid? ForbiddenMemberOwnerId { get; set; }
    


}
