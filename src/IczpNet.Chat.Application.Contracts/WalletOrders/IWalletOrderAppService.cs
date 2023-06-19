using IczpNet.Chat.WalletOrders.Dtos;
using System;
using System.Threading.Tasks;

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
        Task<WalletOrderDetailDto> CloseAsync(Guid id);

    }
}
