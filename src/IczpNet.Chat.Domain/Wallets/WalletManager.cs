using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.WalletBusinesses;
using IczpNet.Chat.WalletRecorders;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Wallets;

public class WalletManager : DomainService, IWalletManager
{
    protected IRepository<Wallet, Guid> Repository { get; }
    protected IRepository<WalletBusiness, string> WalletBusinessRepository { get; }
    protected IRepository<WalletRecorder, Guid> WalletRecorderRepository { get; }
    protected IChatObjectManager ChatObjectManager { get; }
    public WalletManager(
        IRepository<Wallet, Guid> repository,
        IRepository<WalletBusiness, string> walletBusinessRepository,
        IRepository<WalletRecorder, Guid> walletRecorderRepository,
        IChatObjectManager chatObjectManager)
    {
        Repository = repository;
        WalletBusinessRepository = walletBusinessRepository;
        WalletRecorderRepository = walletRecorderRepository;
        ChatObjectManager = chatObjectManager;
    }

    /// <inheritdoc/>
    public Task<Wallet> GetWalletAsync(ChatObject owner)
    {
        return GetWalletAsync(owner.Id);
    }

    /// <inheritdoc/>
    public async Task<WalletBusiness> GetWalletBusinessAsync(string walletBusinessId)
    {
        return Assert.NotNull(await WalletBusinessRepository.FirstOrDefaultAsync(x => x.Id.Equals(walletBusinessId)), $"No such WalletBusiness by id:{walletBusinessId}");
    }

    /// <inheritdoc/>
    public async Task<Wallet> GetWalletAsync(long ownerId)
    {
        var wallet = await Repository.FirstOrDefaultAsync(x => x.OwnerId == ownerId);

        wallet ??= await Repository.InsertAsync(new Wallet(GuidGenerator.Create(), ownerId), autoSave: true);

        return wallet;
    }

    /// <inheritdoc/>
    protected virtual async Task<Wallet> UpdateAndRecordAsync(Wallet wallet, long ownerId, string walletBusinessId, decimal amount, string description, Action<Wallet, WalletRecorder> action)
    {
        Assert.If(wallet.IsEnabled, $"Wallet is disabled", code: "Wallet.Disabled");

        Assert.If(wallet.IsLocked, $"Wallet is locked", code: "Wallet.Lock");

        Assert.If(wallet.OwnerId != ownerId, $"Fail ownerId:{ownerId}", code: "Wallet.Fail");

        var owner = await ChatObjectManager.GetAsync(ownerId);

        var walletBusiness = await GetWalletBusinessAsync(walletBusinessId);

        var walletRecorder = new WalletRecorder(GuidGenerator.Create(), walletBusiness, owner, wallet, amount, description);

        action?.Invoke(wallet, walletRecorder);

        walletRecorder.SetChangedAfter(wallet);

        //wallet.ConcurrencyStamp = concurrencyStamp;

        return await Repository.UpdateAsync(wallet, autoSave: true);
    }

    /// <inheritdoc/>
    public Task<Wallet> ExpenditureAsync(Wallet wallet, long ownerId, string walletBusinessId, decimal amount, string description)
    {
        return UpdateAndRecordAsync(wallet, ownerId, walletBusinessId, amount, description, (wallet, walletRecorder) =>
        {
            wallet.Expenditure(amount, walletRecorder);
        });
    }

    /// <inheritdoc/>
    public Task<Wallet> IncomeAsync(Wallet wallet, long ownerId, string walletBusinessId, decimal amount, string description)
    {
        return UpdateAndRecordAsync(wallet, ownerId, walletBusinessId, amount, description, (wallet, walletRecorder) =>
        {
            wallet.Income(amount, walletRecorder);
        });
    }

    /// <inheritdoc/>
    public Task<Wallet> RechargeAsync(Wallet wallet, long ownerId, decimal amount, string description)
    {
        return IncomeAsync(wallet, ownerId, RedPacketConsts.Recharge, amount, description);
    }

    /// <inheritdoc/>
    public Task<Wallet> LockAmountAsync(Wallet wallet, long ownerId, decimal amount, string description)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<Wallet> UnlockAmountAsync(Wallet wallet, long ownerId, decimal amount, string description)
    {
        throw new NotImplementedException();
    }
}
