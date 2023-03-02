using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.Wallets.Dtos;

public class WalletGetListInput : BaseGetListInput
{

    public virtual long? OwnerId { get; set; }


}
