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

        public Task<Wallet> GetWalletAsync(ChatObject owner)
        {
            return GetWalletAsync(owner.Id);
        }

        public async Task<WalletBusiness> GetWalletBusinessAsync(string walletBusinessId)
        {
            return Assert.NotNull(await WalletBusinessRepository.FindAsync(x => x.Id.Equals(walletBusinessId)), $"No such WalletBusiness by id:{walletBusinessId}");
        }

        public async Task<Wallet> GetWalletAsync(Guid ownerId)
        {
            var wallet = await Repository.FindAsync(x => x.OwnerId == ownerId);

            wallet ??= await Repository.InsertAsync(new Wallet(GuidGenerator.Create(), ownerId), autoSave: true);

            return wallet;
        }

        public Task<Wallet> Expenditure(ChatObject owner, string walletBusinessCode, decimal amount, string description)
        {
            throw new NotImplementedException();
        }

        public async Task<Wallet> Income(ChatObject owner, string walletBusinessCode, decimal amount, string description)
        {
            var wallet = await GetWalletAsync(owner.Id);

            var walletBusiness = await GetWalletBusinessAsync(walletBusinessCode);

            var walletRecorder = new WalletRecorder(GuidGenerator.Create(), owner, wallet);

            wallet.Income(amount, walletRecorder);

            walletRecorder.SetChangedAfter(walletBusiness, wallet, amount, description);

            wallet.ConcurrencyStamp = wallet.ConcurrencyStamp;

            return await Repository.UpdateAsync(wallet, autoSave: true);
        }

        public Task<Wallet> Recharge(ChatObject owner, decimal amount, string description)
        {
            return Income(owner, RedEnvelopeConsts.Recharge, amount, description);
        }
    }
}
