using IczpNet.Chat.WalletOrders.Dtos;
using System;

namespace IczpNet.Chat.WalletOrders
{
    public interface IWalletOrderAppService
        : ICrudChatAppService<
            WalletOrderDetailDto,
            WalletOrderDto,
            Guid,
            WalletOrderGetListInput,
            WalletOrderCreateInput,
            WalletOrderUpdateInput>
    {


    }
}
