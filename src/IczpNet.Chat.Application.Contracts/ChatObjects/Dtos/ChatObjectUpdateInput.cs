using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.ChatObjects.Dtos;

public class ChatObjectUpdateInput : BaseTreeInputDto<long>
{
    public virtual string Code { get; set; }

    //public virtual bool IsActive { get; set; }
    public virtual Genders Gender { get; set; }

    public virtual string Description { get; set; }
}
