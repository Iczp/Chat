using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Officials
{
    public class Official : ChatObject
    {
        public virtual Guid? ChatObjectId { get; set; }

        [ForeignKey(nameof(ChatObjectId))]
        public virtual ChatObject ChatObject { get; set; }


        protected Official()
        {
            ChatObjectType = ChatObjectType.Room;
        }
        protected Official(Guid id) : base(id, ChatObjectType.Official)
        {

        }
    }
}
