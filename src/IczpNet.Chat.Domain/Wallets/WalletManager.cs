using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.RedEnvelopes;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Wallets
{
    public class WalletManager : DomainService, IWalletManager
    {
        protected IRepository<Wallet, Guid> Repository { get; }
        protected IRepository<WalletBusiness, string> WalletBusinessRepository { get; }
        protected IRepository<WalletRecorder, Guid> WalletRecorderRepository { get; }

        public WalletManager(
            IRepository<Wallet, Guid> repository,
            IRepository<WalletBusiness, string> walletBusinessRepository,
            IRepository<WalletRecorder, Guid> walletRecorderRepository)
        {
            Repository = repository;
            WalletBusinessRepository = walletBusinessRepository;
            WalletRecorderRepository = walletRecorderRepository;
        }

        /// <inheritdoc/>
        public Task<Wallet> GetWalletAsync(ChatObject owner)
        {
            return GetWalletAsync(owner.Id);
        }

        /// <inheritdoc/>
        public async Task<WalletBusiness> GetWalletBusinessAsync(string walletBusinessId)
        {
            return Assert.NotNull(await WalletBusinessRepository.FindAsync(x => x.Id.Equals(walletBusinessId)), $"No such WalletBusiness by id:{walletBusinessId}");
        }

        /// <inheritdoc/>
        public async Task<Wallet> GetWalletAsync(long ownerId)
        {
            var wallet = await Repository.FindAsync(x => x.OwnerId == ownerId);

            wallet ??= await Repository.InsertAsync(new Wallet(GuidGenerator.Create(), ownerId), autoSave: true);

            return wallet;
        }

        /// <inheritdoc/>
        public Task<Wallet> ExpenditureAsync(ChatObject owner, string walletBusinessId, decimal amount, string description, string concurrencyStamp)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<Wallet> IncomeAsync(ChatObject owner, string walletBusinessId, decimal amount, string description, string concurrencyStamp)
        {
            var wallet = await GetWalletAsync(owner.Id);

            var walletBusiness = await GetWalletBusinessAsync(walletBusinessId);

            var walletRecorder = new WalletRecorder(GuidGenerator.Create(), owner, wallet);

            wallet.Income(amount, walletRecorder);

            walletRecorder.SetChangedAfter(walletBusiness, wallet, amount, description);

            //var env = context.GetEnvironment();
            //wallet.ConcurrencyStamp = concurrencyStamp;

            return await Repository.UpdateAsync(wallet, autoSave: true);
        }

        /// <inheritdoc/>
        public Task<Wallet> RechargeAsync(ChatObject owner, decimal amount, string description, string concurrencyStamp)
        {
            return IncomeAsync(owner, RedPacketConsts.Recharge, amount, description, concurrencyStamp);
        }

        public Task<Wallet> LockAmountAsync(Wallet wallet, decimal amount, string description, string concurrencyStamp)
        {
            throw new NotImplementedException();
        }

        public Task<Wallet> UnlockAmountAsync(Wallet wallet, decimal amount, string description, string concurrencyStamp)
        {
            throw new NotImplementedException();
        }
    }
}
