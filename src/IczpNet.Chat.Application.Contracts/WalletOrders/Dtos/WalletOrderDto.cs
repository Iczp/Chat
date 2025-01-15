using IczpNet.Chat.Enums;
using System;
using Volo.Abp.Application.Dtos;
using IczpNet.Chat.ChatObjects.Dtos;

namespace IczpNet.Chat.WalletOrders.Dtos;

public class WalletOrderDto : EntityDto<Guid>
{
    public virtual Guid? AppUserId { get; set; }

    public virtual long? OwnerId { get; set; }

    //public virtual ChatObjectDto Owner { get; set; }

    public virtual Guid? WalletId { get; set; }

    public virtual string BusinessId { get; set; }

    public virtual WalletBusinessTypes BusinessType { get; set; }

    public virtual string BusinessTypeName { get; set; }

    public virtual string OrderNo { get; set; }

    public virtual string Title { get; set; }

    public virtual string Description { get; set; }

    public virtual decimal Amount { get; set; }

    public virtual WalletOrderStatus Status { get; set; }

    public virtual DateTime? ExpireTime { get; set; }

    public virtual bool IsExpired { get; set; }

    public virtual bool IsEnabled { get; set; } = true;
}
