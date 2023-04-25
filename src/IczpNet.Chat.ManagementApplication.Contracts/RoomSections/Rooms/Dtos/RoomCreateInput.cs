using IczpNet.Chat.Management.BaseDtos;
using IczpNet.Chat.Enums;
using System.Collections.Generic;

namespace IczpNet.Chat.Management.RoomSections.Rooms.Dtos;

public class RoomCreateInput : BaseInput
{
    public virtual string Name { get; set; }

    public virtual string Code { get; set; }

    /// <summary>
    /// 群拥有者 OwnerUserId (群主)
    /// </summary>
    public virtual long? OwnerId { get; set; }

    /// <summary>
    /// 群类型（自由群、职位群）
    /// </summary>
    public virtual RoomTypes Type { get; set; }

    public virtual string Description { get; set; }

    /// <summary>
    /// ChatObjectId
    /// </summary>
    public virtual List<long> ChatObjectIdList { get; set; }
}
