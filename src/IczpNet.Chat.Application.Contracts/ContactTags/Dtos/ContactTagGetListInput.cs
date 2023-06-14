using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.ContactTags.Dtos;

public class ContactTagGetListInput : BaseGetListInput
{
    public virtual long? OwnerId { get; set; }
}
