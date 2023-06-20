using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.Menus.Dtos;

public class MenuUpdateInput : BaseTreeInputDto<Guid>
{
    /// <summary>
    /// 说明
    /// </summary>
    public virtual string Description { get; set; }
}
