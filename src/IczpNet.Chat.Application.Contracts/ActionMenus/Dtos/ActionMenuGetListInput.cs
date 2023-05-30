using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.ActionMenus.Dtos;

public class ActionMenuGetListInput : BaseTreeGetListInput<long>
{
    public virtual long? OwnerId { get; set; }


    
}
