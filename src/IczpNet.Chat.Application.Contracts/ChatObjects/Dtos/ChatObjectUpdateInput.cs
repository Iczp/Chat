using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.ChatObjects.Dtos;

public class ChatObjectUpdateInput : BaseTreeInputDto<long>
{
    public virtual string Code { get; set; }

    public virtual string Description { get; set; }
}
