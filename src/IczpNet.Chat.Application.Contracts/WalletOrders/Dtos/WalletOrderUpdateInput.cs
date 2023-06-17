using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.WalletOrders.Dtos;

public class WalletOrderUpdateInput : BaseInput
{
    public virtual string Title { get; set; }

    public virtual string Description { get; set; }

    public virtual decimal Amount { get; set; }
}
