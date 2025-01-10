using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.WalletBusinesses;
using System.Threading.Tasks;

namespace IczpNet.Chat.Wallets;

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
    /// <param name="wallet"></param>
    /// <param name="ownerId"></param>
    /// <param name="walletBusinessId"></param>
    /// <param name="amount"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    Task<Wallet> ExpenditureAsync(Wallet wallet, long ownerId, string walletBusinessId, decimal amount, string description);

    /// <summary>
    /// 收入
    /// </summary>
    /// <param name="wallet"></param>
    /// <param name="ownerId"></param>
    /// <param name="walletBusinessId"></param>
    /// <param name="amount"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    Task<Wallet> IncomeAsync(Wallet wallet, long ownerId, string walletBusinessId, decimal amount, string description);

    /// <summary>
    /// 充值
    /// </summary>
    /// <param name="wallet"></param>
    /// <param name="ownerId"></param>
    /// <param name="amount"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    Task<Wallet> RechargeAsync(Wallet wallet, long ownerId, decimal amount, string description);

    /// <summary>
    /// 冻结金额
    /// </summary>
    /// <param name="wallet"></param>
    /// <param name="ownerId"></param>
    /// <param name="amount"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    Task<Wallet> LockAmountAsync(Wallet wallet, long ownerId, decimal amount, string description);

    /// <summary>
    /// 解除冻结金额
    /// </summary>
    /// <param name="wallet"></param>
    /// <param name="ownerId"></param>
    /// <param name="amount"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    Task<Wallet> UnlockAmountAsync(Wallet wallet, long ownerId, decimal amount, string description);

}
