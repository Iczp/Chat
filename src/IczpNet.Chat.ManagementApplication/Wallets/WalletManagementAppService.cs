using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Management.BaseAppServices;
using IczpNet.Chat.Management.Wallets.Dtos;
using IczpNet.Chat.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Management.Wallets
{
    public class WalletManagementAppService
        : CrudChatManagementAppService<
            Wallet,
            WalletDetailDto,
            WalletDto,
            Guid,
            WalletGetListInput,
            WalletCreateInput,
            WalletUpdateInput>,
        IWalletManagementAppService
    {

        protected IWalletManager WalletManager { get; }
        protected IChatObjectManager ChatObjectManager { get; }

        public WalletManagementAppService(
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

        public async Task<WalletDto> Recharge(RechargeInput input)
        {
            var owner = await ChatObjectManager.GetAsync(input.OwnerId);
            var wallet = await WalletManager.Recharge(owner, input.Amount, input.Description, input.ConcurrencyStamp);
            return ObjectMapper.Map<Wallet, WalletDto>(wallet);
        }
    }
}
