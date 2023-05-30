using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Developers
{
    public class Developer : BaseEntity, IChatOwner<long>, IIsEnabled
    {
        public virtual long OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; set; }

        [MaxLength(100)]
        public virtual string ApiUrl { get; set; }

        public virtual bool IsEnabled { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { OwnerId };
        }
    }
}
