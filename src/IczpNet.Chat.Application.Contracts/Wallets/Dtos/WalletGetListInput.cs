using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.Wallets.Dtos;

public class WalletGetListInput : BaseGetListInput
{

    public virtual Guid? OwnerId { get; set; }


}
