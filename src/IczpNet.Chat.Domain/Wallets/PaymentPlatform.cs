using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Wallets
{
    public class PaymentPlatform : BaseEntity<string>, IIsEnabled
    {
        [StringLength(64)]
        public override string Id { get => base.Id; protected set => base.Id = value; }
        [StringLength(50)]
        public virtual string Name { get; set; }
        [StringLength(100)]
        public virtual string Description { get; set; }

        public virtual bool IsEnabled { get; protected set; }

        public virtual bool IsStatic { get; protected set; }
    }
}
