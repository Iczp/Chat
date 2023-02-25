using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.ChatObjects.Dtos;

public class ChatObjectGetListInput : BaseGetListInput
{
    public virtual ChatObjectTypeEnums? ObjectType { get; set; }
}
