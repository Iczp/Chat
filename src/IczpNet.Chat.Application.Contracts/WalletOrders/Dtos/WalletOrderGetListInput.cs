using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.WalletOrders.Dtos;

public class WalletOrderGetListInput : BaseGetListInput
{
    //public virtual Guid? AppUserId { get; set; }

    public virtual long? OwnerId { get; set; }

    public virtual string WalletBusinessId { get; set; }

    public virtual WalletOrderStatus? Status { get; set; }

    public virtual WalletBusinessTypes? WalletBusinessType { get; set; }

    public virtual decimal? MinAmount { get; set; }

    public virtual decimal? MaxAmount { get; set; }

    public virtual bool? IsEnabled { get; set; }
}
