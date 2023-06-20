using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.Wallets.Dtos;

public class WalletGetListInput : GetListInput
{

    public virtual long? OwnerId { get; set; }


}
