using IczpNet.AbpCommons;
using IczpNet.Chat.BaseEntities;
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

        public virtual IList<ChatObjectCategoryUnit> ChatObjectCategoryUnitList { get; set; }

        public override void SetParent(ChatObjectCategory parent)
        {
            if (Parent != null)
            {
                Assert.If(Parent.ChatObjectTypeId != ChatObjectTypeId, $"不是父级的聊天对象[ChatObjectTypeId]：'{Parent.ChatObjectTypeId}','{ChatObjectTypeId}'");
            }
            base.SetParent(parent);
        }
    }
}
