using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.RoomSections.Rooms.Dtos;

public class RoomDto : BaseDto<Guid>
{
    public virtual long AutoId { get; set; }

    public virtual Guid SessionId { get; set; }

    public virtual string Name { get; set; }

    public virtual string Code { get; set; }

    public virtual RoomTypes Type { get; set; }

    public virtual int MemberCount { get; set; }

}
