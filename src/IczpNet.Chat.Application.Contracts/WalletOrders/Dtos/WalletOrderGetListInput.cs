using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.WalletOrders.Dtos;

public class WalletOrderGetListInput : GetListInput
{
    //public virtual Guid? AppUserId { get; set; }

    public virtual long? OwnerId { get; set; }

    public virtual string BusinessId { get; set; }

    public virtual WalletOrderStatus? Status { get; set; }

    public virtual WalletBusinessTypes? BusinessType { get; set; }

    public virtual decimal? MinAmount { get; set; }

    public virtual decimal? MaxAmount { get; set; }

    public virtual bool? IsEnabled { get; set; }
}
