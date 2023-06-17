using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Wallets
{
    public class WalletRecorder : BaseEntity<Guid>, IChatOwner<long?>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long AutoId { get; protected set; }

        public virtual long? OwnerId { get; protected set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; protected set; }

        [Comment("钱包Id")]
        public virtual Guid? WalletId { get; protected set; }

        [ForeignKey(nameof(WalletId))]
        public virtual Wallet Wallet { get; protected set; }

        [Comment("钱包业务Id")]
        public virtual string WalletBusinessId { get; protected set; }

        [ForeignKey(nameof(WalletBusinessId))]
        public virtual WalletBusiness WalletBusiness { get; protected set; }

        public virtual WalletBusinessTypes WalletBusinessType { get; protected set; }

        [Column(TypeName = "decimal(18,2)")]
        [Comment("变化前-可用金额")]
        public virtual decimal AvailableAmountBeforeChange { get; protected set; }

        [Column(TypeName = "decimal(18,2)")]
        [Comment("变化前-总金额")]
        public virtual decimal TotalAmountBeforeChange { get; protected set; }

        [Column(TypeName = "decimal(18,2)")]
        [Comment("变化前-冻结金额")]
        public virtual decimal LockAmountBeforeChange { get; protected set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.0, (double)decimal.MaxValue)]
        [Comment("可用金额")]
        public virtual decimal AvailableAmount { get; protected set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.0, (double)decimal.MaxValue)]
        [Comment("总金额")]
        public virtual decimal TotalAmount { get; protected set; }

        [Column(TypeName = "decimal(18,2)")]
        [Comment("冻结金额")]
        public virtual decimal LockAmount { get; protected set; }

        [Column(TypeName = "decimal(18,2)")]
        [Comment("变更金额")]
        public virtual decimal Amount { get; protected set; }

        [StringLength(100)]
        [Comment("说明")]
        public virtual string Description { get; protected set; }

        protected WalletRecorder() { }

        public WalletRecorder(Guid id, ChatObject owner, Wallet wallet) : base(id)
        {
            Owner = owner;
            AvailableAmountBeforeChange = wallet.AvailableAmount;
            TotalAmountBeforeChange = wallet.TotalAmount;
            LockAmountBeforeChange = wallet.LockAmount;
        }

        public void SetChangedAfter(WalletBusiness walletBusiness, Wallet wallet, decimal amount, string description)
        {
            WalletBusiness = walletBusiness;
            WalletBusinessType = WalletBusiness.BusinessType;

            Amount = amount;
            AvailableAmount = wallet.AvailableAmount;
            TotalAmount = wallet.TotalAmount;
            LockAmount = wallet.LockAmount;
            Description = description;
        }
    }
}
