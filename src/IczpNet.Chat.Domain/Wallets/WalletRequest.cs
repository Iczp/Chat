using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.WalletBusinesses;
using IczpNet.Chat.WalletRecorders;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Wallets;

public class WalletRequest : BaseEntity<Guid>, IChatOwner<long>
{
    [StringLength(64)]
    public override Guid Id { get => base.Id; protected set => base.Id = value; }
    public virtual long OwnerId { get; protected set; }

    [ForeignKey(nameof(OwnerId))]
    public virtual ChatObject Owner { get; protected set; }

    public virtual Guid WalletRecorderId { get; protected set; }

    [ForeignKey(nameof(WalletRecorderId))]
    public virtual WalletRecorder WalletRecorder { get; protected set; }

    public virtual string WalletBusinessId { get; protected set; }

    [ForeignKey(nameof(WalletBusinessId))]
    public virtual WalletBusiness WalletBusiness { get; protected set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0.0, (double)decimal.MaxValue)]
    [Display(Name = "Amount")]
    public virtual decimal Amount { get; protected set; }

    public virtual string PaymentPlatformId { get; protected set; }

    [ForeignKey(nameof(PaymentPlatformId))]
    public virtual PaymentPlatform PaymentPlatform { get; protected set; }

    [StringLength(100)]
    public virtual string Descripton { get; protected set; }

    /// <summary>
    /// 是否入账
    /// </summary>
    public virtual bool IsPosting { get; protected set; }

    public virtual DateTime? PostDate { get; protected set; }

    public void Posting(WalletRecorder walletRecorder)
    {
        WalletRecorder = walletRecorder;
        IsPosting = true;
        PostDate = DateTime.Now;
    }
}
