using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.ActionMenus
{
    public class ActionMenu : BaseTreeEntity<ActionMenu, long>, IChatOwner<long>
    {
        public virtual long OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; set; }

        //public virtual ActionEvents Event { get; set; }

        //[MaxLength(64)]
        //public virtual string Code { get; set; }

        //[MaxLength(500)]
        //public virtual string Args { get; set; }
    }
}
