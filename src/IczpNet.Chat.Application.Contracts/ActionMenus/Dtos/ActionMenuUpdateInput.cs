using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.ActionMenus.Dtos;

public class ActionMenuUpdateInput : BaseTreeInputDto<long>
{
    public virtual string Description { get; set; }
}
