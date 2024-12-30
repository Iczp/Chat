using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.WalletBusinesses;
using IczpNet.Chat.WalletRecorders;
using IczpNet.Chat.Wallets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.WalletOrders;

[Index(nameof(OrderNo), AllDescending = true)]
[Index(nameof(OwnerId))]
[Index(nameof(BusinessType))]
[Index(nameof(Amount))]
[Index(nameof(Status))]
[Index(nameof(IsExpired))]
[Index(nameof(IsEnabled))]
public class WalletOrder : BaseEntity<Guid>, IIsEnabled
{
    [MaxLength(40)]
    public virtual string OrderNo { get; protected set; }

    public virtual Guid? AppUserId { get; protected set; }

    public virtual long? OwnerId { get; protected set; }

    [ForeignKey(nameof(OwnerId))]
    public virtual ChatObject Owner { get; protected set; }

    [Required]
    [Comment("钱包Id")]
    public virtual Guid? WalletId { get; protected set; }

    [ForeignKey(nameof(WalletId))]
    public virtual Wallet Wallet { get; protected set; }

    [Required]
    [Comment("钱包业务Id")]
    public virtual string BusinessId { get; protected set; }

    [ForeignKey(nameof(BusinessId))]
    public virtual WalletBusiness Business { get; protected set; }

    public virtual WalletBusinessTypes BusinessType { get; protected set; }

    public virtual Guid? RecorderId { get; protected set; }

    [ForeignKey(nameof(RecorderId))]
    public virtual WalletRecorder Recorder { get; protected set; }

    [MaxLength(100)]
    public virtual string BusinessTypeName { get; protected set; }

    [MaxLength(100)]
    [Comment("标题")]
    public virtual string Title { get; internal protected set; }

    [MaxLength(100)]
    [Comment("说明")]
    public virtual string Description { get; internal protected set; }

    [Column(TypeName = "decimal(18,2)")]
    [Comment("变更金额")]
    public virtual decimal Amount { get; internal protected set; }

    [Comment("订单状态")]
    public virtual WalletOrderStatus Status { get; protected set; }

    [Comment("到期时间")]
    public virtual DateTime? ExpireTime { get; protected set; }

    [Comment("是否到期")]
    public virtual bool IsExpired { get; protected set; }

    [Comment("是否有效")]
    public virtual bool IsEnabled { get; protected set; }

    protected WalletOrder() { }

    public WalletOrder(Guid id,
        Wallet wallet,
        WalletBusiness business,
        string orderNo,
        string title,
        string description,
        decimal amount,
        DateTime expireTime) : base(id)
    {
        Wallet = wallet;
        Owner = wallet.Owner;
        OwnerId = wallet.OwnerId;
        AppUserId = wallet.AppUserId;
        Business = business;
        BusinessTypeName = business.Name;
        BusinessType = business.BusinessType;

        OrderNo = orderNo;
        Title = title;
        Description = description;
        Amount = amount;
        ExpireTime = expireTime;

        Status = WalletOrderStatus.Pending;
        IsEnabled = true;
        IsExpired = false;
    }

    internal void Close()
    {
        Status = WalletOrderStatus.Close;
        IsEnabled = false;
        IsExpired = true;
    }


    internal void Success(WalletRecorder recorder)
    {

        Recorder = recorder;
        Status = WalletOrderStatus.Success;

    }
    
}
