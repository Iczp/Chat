using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.ChatObjects.Dtos;

public class ChatObjectUpdateInput : BaseInput
{
    public virtual string Name { get; set; }

    public virtual string Code { get; set; }
}
