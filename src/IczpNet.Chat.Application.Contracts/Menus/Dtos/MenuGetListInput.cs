using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.Menus.Dtos;

public class MenuGetListInput : BaseTreeGetListInput<Guid>
{
    /// <summary>
    /// 所属聊天对角
    /// </summary>
    public virtual long? OwnerId { get; set; }
}
