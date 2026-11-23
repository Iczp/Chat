using IczpNet.AbpTrees;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.Menus;

public class MenuInfo : TreeInfo<Guid>
{
    public virtual long OwnerId { get; set; }

    /// <summary>
    /// 事件类型 (Action/Click, Navigation/View 等)
    /// </summary>
    public virtual ActionEvents Event { get; set; }

    /// <summary>
    /// 菜单编码/Key
    /// </summary>
    public virtual string Code { get; set; }

    /// <summary>
    /// 参数/URL
    /// </summary>
    public virtual string Args { get; set; }
}
