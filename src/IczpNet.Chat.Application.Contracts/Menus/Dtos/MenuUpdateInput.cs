using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.Menus.Dtos;

public class MenuUpdateInput : BaseTreeInputDto<Guid>
{
    public virtual string Description { get; set; }
}
