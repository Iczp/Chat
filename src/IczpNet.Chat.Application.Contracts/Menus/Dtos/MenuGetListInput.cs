using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.Menus.Dtos;

public class MenuGetListInput : BaseTreeGetListInput<Guid>
{
    public virtual long? OwnerId { get; set; }


    
}
