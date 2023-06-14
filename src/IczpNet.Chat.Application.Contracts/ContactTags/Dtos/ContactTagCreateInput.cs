using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.ContactTags.Dtos;

public class ContactTagCreateInput : ContactTagUpdateInput
{
    [Required]
    public virtual long OwnerId { get; set; }
}
