using IczpNet.AbpTrees;
using System;

namespace IczpNet.Chat.Menus;

public class MenuInfo : TreeInfo<Guid>
{
    public virtual long OwnerId { get; set; }
}
