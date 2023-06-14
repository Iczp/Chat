using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Contacts.Dtos
{
    public class ContactsGetListInput : BaseGetListInput
    {
        [Required]
        public virtual long OwnerId { get; set; }

        public virtual ChatObjectTypeEnums? DestinationObjectType { get; set; }

        public virtual Guid? TagId { get; set; }

    }
}
