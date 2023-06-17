using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Wallets.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Wallets
{
    public class WalletAppService
        : CrudChatAppService<
            Wallet,
            WalletDetailDto,
            WalletDto,
            Guid,
            WalletGetListInput,
            WalletCreateInput,
            WalletUpdateInput>,
        IWalletAppService
    {

        protected IWalletManager WalletManager { get; }
        protected IChatObjectManager ChatObjectManager { get; }

        public WalletAppService(
            IRepository<Wallet, Guid> repository,
            IChatObjectManager chatObjectManager,
            IWalletManager walletManager) : base(repository)
        {
            ChatObjectManager = chatObjectManager;
            WalletManager = walletManager;
        }

        protected override async Task<IQueryable<Wallet>> CreateFilteredQueryAsync(WalletGetListInput input)
        {
            return (await base.CreateFilteredQueryAsync(input))
                .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
                ;
        }

        protected override Task CheckDeleteAsync(Wallet entity)
        {
            var count = entity.WalletRecorderList.Count;

            Assert.If(count != 0, $"Wallet's Recorder count: {count}");

            return base.CheckDeleteAsync(entity);
        }

        public async Task<WalletDto> RechargeAsync(RechargeInput input)
        {
            var wallet = await WalletManager.GetWalletAsync(input.OwnerId);

            wallet = await WalletManager.RechargeAsync(wallet, input.OwnerId, input.Amount, input.Description);

            return ObjectMapper.Map<Wallet, WalletDto>(wallet);
        }
    }
}
