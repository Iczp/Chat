using System;

namespace IczpNet.Chat.Menus;

public class MenuTriggerArgs
{
    public virtual Guid MenuId { get; set; }

    /// <summary>
    /// 触发人的会话单元Id (用于回发被动回复)
    /// </summary>
    public virtual Guid? SessionUnitId { get; set; }

    /// <summary>
    /// 触发人Id
    /// </summary>
    public virtual long? SenderId { get; set; }
}
