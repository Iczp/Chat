using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System.Collections.Generic;

namespace IczpNet.Chat.RoomSections.Rooms.Dtos;

public class RoomCreateInput : BaseInput
{
    /// <summary>
    /// 群名称
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// 编码(唯一)
    /// </summary>
    public virtual string Code { get; set; }

    /// <summary>
    /// 群拥有者 OwnerUserId (群主)
    /// </summary>
    public virtual long? OwnerId { get; set; }

    /// <summary>
    /// 群类型（自由群、职位群）
    /// </summary>
    public virtual RoomTypes Type { get; set; }

    /// <summary>
    /// 说明
    /// </summary>
    public virtual string Description { get; set; }

    /// <summary>
    /// 群成员【聊天对象】列表
    /// </summary>
    public virtual List<long> ChatObjectIdList { get; set; }
}
