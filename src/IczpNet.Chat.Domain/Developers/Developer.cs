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

        /// <summary>
        /// 开发者设置的Token
        /// </summary>
        [StringLength(50)]
        public virtual string Token { get; protected set; }

        /// <summary>
        /// 开发者设置的EncodingAESKey
        /// </summary>
        [StringLength(43)]
        public virtual string EncodingAesKey { get; protected set; }

        /// <summary>
        /// 开发者设置 的 Url
        /// </summary>
        [StringLength(256)]
        public virtual string PostUrl { get; set; }

        /// <summary>
        /// 是否启用开发者
        /// </summary>
        public virtual bool IsEnabled { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { OwnerId };
        }
    }
}
