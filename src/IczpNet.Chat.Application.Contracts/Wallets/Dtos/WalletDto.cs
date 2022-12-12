using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.Wallets.Dtos;

public class WalletDto : BaseDto<Guid>
{
    public virtual Guid OwnerId { get; set; }

    public virtual decimal AvailableAmount { get; set; }

    public virtual decimal LockAmount { get; set; }

    public virtual decimal TotalAmount { get; set; }
}
