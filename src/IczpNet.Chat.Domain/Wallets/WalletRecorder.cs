using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Wallets
{
    public class WalletRecorder : BaseEntity<Guid>, IChatOwner<Guid?>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long AutoId { get; protected set; }

        public virtual Guid? OwnerId { get; protected set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; protected set; }

        public virtual Guid? WalletId { get; protected set; }

        [ForeignKey(nameof(WalletId))]
        public virtual Wallet Wallet { get; protected set; }

        public virtual string WalletBusinessId { get; protected set; }

        [ForeignKey(nameof(WalletBusinessId))]
        public virtual WalletBusiness WalletBusiness { get; protected set; }

        public virtual WalletBusinessTypes WalletBusinessType { get; protected set; }


        [Column(TypeName = "decimal(18,2)")]
        public virtual decimal AmountBeforeChange { get; protected set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.0, (double)decimal.MaxValue)]
        public virtual decimal Amount { get; protected set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.0, (double)decimal.MaxValue)]
        public virtual decimal CurrentAmount { get; protected set; }

        [StringLength(100)]
        public virtual string Description { get; protected set; }

        protected WalletRecorder() { }

        public WalletRecorder(Guid id,  ChatObject owner, WalletBusiness walletBusiness, decimal amount, string description) : base(id)
        {
            Owner = owner;
            Amount = amount;
            Description = description;
            WalletBusiness = walletBusiness;
            WalletBusinessType = WalletBusiness.BusinessType;
        }
    }
}
