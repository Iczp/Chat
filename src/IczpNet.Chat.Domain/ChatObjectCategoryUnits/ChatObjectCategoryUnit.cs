using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjectCategorys;
using IczpNet.Chat.ChatObjects;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.ChatObjectCategoryUnits
{
    public class ChatObjectCategoryUnit : BaseEntity
    {

        public virtual long ChatObjectId { get; set; }

        public virtual Guid CategoryId { get; set; }

        [ForeignKey(nameof(ChatObjectId))]
        public virtual ChatObject ChatObject { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual ChatObjectCategory Category { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { ChatObjectId, CategoryId };
        }
    }
}
