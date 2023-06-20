using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.ContactTags.Dtos;

public class ContactTagGetListInput : GetListInput
{
    public virtual long? OwnerId { get; set; }
}
