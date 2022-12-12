using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.Wallets
{
    public interface IWalletManager
    {
        Task<Wallet> GetWalletAsync(Guid ownerId);

        Task<WalletBusiness> GetWalletBusinessAsync(string walletBusinessId);

        Task<Wallet> GetWalletAsync(ChatObject owner);

        /// <summary>
        /// 支出
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="amount"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        Task<Wallet> Expenditure(ChatObject owner, string walletBusinessCode, decimal amount, string description, string concurrencyStamp);

        /// <summary>
        /// 收入
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="amount"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        Task<Wallet> Income(ChatObject owner, string walletBusinessCode, decimal amount, string description, string concurrencyStamp);

        Task<Wallet> Recharge(ChatObject owner, decimal amount, string description, string concurrencyStamp);

    }
}
