using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjectCategoryUnits;
using IczpNet.Chat.ChatObjectTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.ChatObjectCategorys
{
    public class ChatObjectCategory : BaseTreeEntity<ChatObjectCategory, Guid>
    {
        public virtual string ChatObjectTypeId { get; set; }

        [ForeignKey(nameof(ChatObjectTypeId))]
        public virtual ChatObjectType ChatObjectType { get; set; }

        public virtual IList<ChatObjectCategoryUnit> ChatObjectCategoryUnitList{ get; set; }
}
}
