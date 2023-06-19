using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.WalletOrders
{
    public interface IWalletOrderManager
    {
        Task<WalletOrder> CreateAsync(long ownerId, string businessId, string title, string description, decimal amount);

        Task<WalletOrder> UpdateAsync(Guid orderId, string title, string description, decimal amount);

        Task<WalletOrder> CloseAsync(Guid orderId);
    }
}
