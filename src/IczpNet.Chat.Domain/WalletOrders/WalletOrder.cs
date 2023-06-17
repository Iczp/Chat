using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.WalletBusinesses;
using IczpNet.Chat.Wallets;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IczpNet.Chat.BaseEntities;
using IczpNet.AbpCommons.DataFilters;

namespace IczpNet.Chat.WalletOrders
{
    public class WalletOrder : BaseEntity<Guid>, IIsEnabled
    {
        [MaxLength(40)]
        public virtual string OrderNo { get; protected set; }

        public virtual Guid? AppUserId { get; protected set; }

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

        [MaxLength(100)]
        [Comment("标题")]
        public virtual string Title { get; protected set; }

        [MaxLength(100)]
        [Comment("说明")]
        public virtual string Description { get; protected set; }

        [Column(TypeName = "decimal(18,2)")]
        [Comment("变更金额")]
        public virtual decimal Amount { get; protected set; }

        [Comment("订单状态")]
        public virtual WalletOrderStatus Status { get; protected set; }

        [Comment("到期时间")] 
        public virtual DateTime? ExpireTime { get; protected set; }

        [Comment("是否到期")]
        public virtual bool IsExpired { get; protected set; }

        [Comment("是否有效")]
        public virtual bool IsEnabled { get; protected set; }
    }
}
