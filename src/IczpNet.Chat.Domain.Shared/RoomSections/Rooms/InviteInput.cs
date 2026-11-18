using IczpNet.Chat.Enums;
using System.Collections.Generic;

namespace IczpNet.Chat.RoomSections.Rooms;

public class InviteInput
{
    /// <summary>
    /// 群Id
    /// </summary>
    public virtual long RoomId { get; set; }

    /// <summary>
    /// 聊天对象Id(群成员)
    /// </summary>
    public virtual List<long> MemberIdList { get; set; }

    /// <summary>
    /// 邀请人(可空)
    /// </summary>
    public virtual long? InviterId { get; set; }

    /// <summary>
    /// 加入方式（默认邀请，扫码入群传 JoinWays.Scan）
    /// </summary>
    public virtual JoinWays? JoinWay { get; set; }
}
