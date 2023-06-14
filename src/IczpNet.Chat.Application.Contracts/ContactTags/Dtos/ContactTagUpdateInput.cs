using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.ContactTags.Dtos;

public class ContactTagUpdateInput : BaseInput
{
    public virtual string Name { get; set; }
}
