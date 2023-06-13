using IczpNet.Chat.ChatObjects.Dtos;
using System;

namespace IczpNet.Chat.Cantacts.Dtos
{
    public class ContactsDto
    {
        public virtual Guid Id { get; set; }

        public virtual string DisplayName { get; set; }

        public virtual ChatObjectDto Destination { get; set; }
    }
}
