using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Wallets
{
    public class WalletBusiness : BaseEntity<string>, IIsEnabled, IIsStatic
    {
        [StringLength(50)]
        public virtual string Name { get; protected set; }

        public virtual WalletBusinessTypes BusinessType { get; protected set; }

        [StringLength(100)]
        public virtual string Description { get; protected set; }

        [InverseProperty(nameof(WalletRecorder.WalletBusiness))]
        public virtual List<WalletRecorder> WalletRecorderList { get; protected set; }

        public virtual bool IsEnabled { get; protected set; }

        public virtual bool IsStatic { get; protected set; }

        protected WalletBusiness() { }

        public WalletBusiness(string id, string name, WalletBusinessTypes walletBusinessType, string description, bool isStatic, bool isEnabled) : base(id)
        {
            //base.Id= id.ToLower();
            Name = name;
            Description = description;
            BusinessType = walletBusinessType;
            IsStatic = isStatic;
            IsEnabled = isEnabled;
        }
    }
}
