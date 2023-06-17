using IczpNet.Chat.ChatObjects;
using System.Threading.Tasks;

namespace IczpNet.Chat.Wallets
{
    public interface IWalletManager
    {
        /// <summary>
        /// 获取钱包
        /// </summary>
        /// <param name="ownerId"></param>
        /// <returns></returns>
        Task<Wallet> GetWalletAsync(long ownerId);

        /// <summary>
        /// 获取钱包
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        Task<Wallet> GetWalletAsync(ChatObject owner);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletBusinessId"></param>
        /// <returns></returns>
        Task<WalletBusiness> GetWalletBusinessAsync(string walletBusinessId);

        /// <summary>
        /// 支出
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="amount"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        Task<Wallet> ExpenditureAsync(ChatObject owner, string walletBusinessId, decimal amount, string description, string concurrencyStamp);

        /// <summary>
        /// 收入
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="amount"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        Task<Wallet> IncomeAsync(ChatObject owner, string walletBusinessId, decimal amount, string description, string concurrencyStamp);

        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="amount"></param>
        /// <param name="description"></param>
        /// <param name="concurrencyStamp"></param>
        /// <returns></returns>
        Task<Wallet> RechargeAsync(ChatObject owner, decimal amount, string description, string concurrencyStamp);

        /// <summary>
        /// 冻结金额
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="amount"></param>
        /// <param name="description"></param>
        /// <param name="concurrencyStamp"></param>
        /// <returns></returns>
        Task<Wallet> LockAmountAsync(Wallet wallet, decimal amount, string description, string concurrencyStamp);

        /// <summary>
        /// 解除冻结金额
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="amount"></param>
        /// <param name="description"></param>
        /// <param name="concurrencyStamp"></param>
        /// <returns></returns>
        Task<Wallet> UnlockAmountAsync(Wallet wallet, decimal amount, string description, string concurrencyStamp);

    }
}
