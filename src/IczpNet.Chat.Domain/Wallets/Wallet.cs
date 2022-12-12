using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Wallets
{
    public class Wallet : BaseEntity<Guid>, IChatOwner<Guid>
    {
        public readonly decimal OriginalAvailableAmount;

        public virtual Guid OwnerId { get; protected set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; protected set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.0, (double)decimal.MaxValue)]
        public virtual decimal AvailableAmount { get; protected set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.0, (double)decimal.MaxValue)]
        public virtual decimal LockAmount { get; protected set; }

        [Column(TypeName = "decimal(18,2)")]
        public virtual decimal TotalAmount { get; protected set; }

        public virtual List<WalletRecorder> WalletRecorderList { get; protected set; } = new List<WalletRecorder>();

        protected Wallet()
        {
            OriginalAvailableAmount = AvailableAmount;
        }

        public Wallet(Guid id, Guid ownerId) : base(id)
        {
            OwnerId = ownerId;
        }

        internal void Expenditure(decimal amount)
        {
            AvailableAmount -= amount;
        }

        internal void Income(decimal amount)
        {
            AvailableAmount += amount;
        }

        ///// <summary>
        ///// 支出
        ///// </summary>
        ///// <param name="amount"></param>
        ///// <param name="description"></param>
        //public virtual void Expenditure(Guid recordId, decimal amount, string description)
        //{
        //    WalletRecorderList.Add(new WalletRecorder(recordId, this, Owner, amount, description));
        //    AvailableAmount -= amount;
        //}

        ///// <summary>
        ///// 收入
        ///// </summary>
        ///// <param name="amount"></param>
        ///// <param name="description"></param>
        //public virtual void Income(Guid recordId, decimal amount, string description)
        //{
        //    WalletRecorderList.Add(new WalletRecorder(recordId, this, Owner, amount, description));
        //    AvailableAmount += amount;
        //}
    }
}
