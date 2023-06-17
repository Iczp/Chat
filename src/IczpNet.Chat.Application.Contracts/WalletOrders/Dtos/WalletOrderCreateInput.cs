using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.WalletOrders.Dtos;

public class WalletOrderCreateInput : WalletOrderUpdateInput
{
    public virtual long? OwnerId { get; set; }

    public virtual string BusinessId { get; set; }

    public virtual WalletBusinessTypes BusinessType { get; set; }
}
