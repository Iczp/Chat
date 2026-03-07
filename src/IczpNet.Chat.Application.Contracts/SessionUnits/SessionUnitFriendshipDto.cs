using System;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitFriendshipDto
{
    /// <summary>
    /// 是否好友
    /// </summary>
    public virtual bool IsFriendship { get; set; } = false;

    /// <summary>
    /// 好友名称
    /// </summary>
    public virtual string DisplayName { get; set; }

    /// <summary>
    /// 好友会话Id
    /// </summary>
    public virtual Guid? SessionUnitId { get; set; }

    /// <summary>
    /// 访问者 SessionUnitId
    /// </summary>
    public virtual Guid VisitorId { get; set; }
}
