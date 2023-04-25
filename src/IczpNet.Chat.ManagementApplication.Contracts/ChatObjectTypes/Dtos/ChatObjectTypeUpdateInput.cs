using IczpNet.Chat.Management.BaseDtos;

namespace IczpNet.Chat.Management.ChatObjectTypes.Dtos;

public class ChatObjectTypeUpdateInput : BaseInput
{
    public virtual string Name { get; set; }

    public virtual int MaxDepth { get; set; }

    public virtual bool IsHasChild { get; set; }

    public virtual string Description { get; set; }
}
