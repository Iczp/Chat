using IczpNet.Chat.Management.BaseDtos;
using System;

namespace IczpNet.Chat.Management.Wallets.Dtos;

public class WalletDto : BaseDto<Guid>
{
    public virtual long OwnerId { get; set; }

    public virtual decimal AvailableAmount { get; set; }

    public virtual decimal LockAmount { get; set; }

    public virtual decimal TotalAmount { get; set; }
}
