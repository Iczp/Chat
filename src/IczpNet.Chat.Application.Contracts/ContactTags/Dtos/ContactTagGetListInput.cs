using IczpNet.Chat.BaseDtos;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.ContactTags.Dtos;

public class ContactTagGetListInput : GetListInput
{
    //[Required]
    public virtual long? OwnerId { get; set; }
}
