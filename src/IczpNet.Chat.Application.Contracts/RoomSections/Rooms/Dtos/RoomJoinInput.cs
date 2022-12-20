using IczpNet.Chat.BaseDtos;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.RoomSections.Rooms.Dtos;

public class RoomJoinInput : BaseInput
{
    //public virtual Guid RoomId { get; set; }

    /// <summary>
    /// 邀请人
    /// </summary>
    public virtual Guid InviterId { get; set; }

    /// <summary>
    /// ChatObjectId
    /// </summary>
    public virtual List<Guid> ChatObjectIdList { get; set; }
}
