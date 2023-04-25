using IczpNet.Chat.Management.BaseDtos;
using System;

namespace IczpNet.Chat.Management.Wallets.Dtos;

public class WalletGetListInput : BaseGetListInput
{

    public virtual long? OwnerId { get; set; }


}
