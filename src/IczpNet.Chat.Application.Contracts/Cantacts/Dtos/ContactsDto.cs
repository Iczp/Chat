using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.ContactTags.Dtos;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Cantacts.Dtos
{
    public class ContactsDto
    {
        public virtual Guid Id { get; set; }

        public virtual string DisplayName { get; set; }

        public virtual ChatObjectDto Destination { get; set; }

        //public virtual List<ContactTagSimpleDto> ContactTags { get; set; }

    }
}
