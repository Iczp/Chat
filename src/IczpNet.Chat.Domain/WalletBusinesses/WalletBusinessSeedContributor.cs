using IczpNet.Chat.Enums;
using IczpNet.Chat.RedEnvelopes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace IczpNet.Chat.WalletBusinesses
{
    public class WalletBusinessSeedContributor : IDataSeedContributor, ITransientDependency
    {
        protected IConfiguration Configuration { get; }
        protected ICurrentTenant CurrentTenant { get; }
        protected ILogger<WalletBusinessSeedContributor> Logger { get; }
        protected IRepository<WalletBusiness, string> Repository { get; }
        protected IGuidGenerator GuidGenerator { get; }

        public WalletBusinessSeedContributor(
            IConfiguration configuration,
            ICurrentTenant currentTenant,
            ILogger<WalletBusinessSeedContributor> logger,
            IRepository<WalletBusiness, string> repository,
            IGuidGenerator guidGenerator)
        {
            Configuration = configuration;
            CurrentTenant = currentTenant;
            Logger = logger;
            Repository = repository;
            GuidGenerator = guidGenerator;
        }

        [UnitOfWork]
        public virtual async Task SeedAsync(DataSeedContext context)
        {
            using (CurrentTenant.Change(context?.TenantId))
            {
                await CreateAsync();
            }
        }

        private async Task CreateAsync()
        {
            var list = new List<WalletBusiness>(){
                new WalletBusiness(
                        id: RedPacketConsts.Recharge,
                        name: "充值",
                        walletBusinessType: WalletBusinessTypes.Income,
                        description: "账户充值",
                        isEnabled: true,
                        isStatic: true),
                 new WalletBusiness(
                        id: RedPacketConsts.Withdrawal,
                        name: "提现",
                        walletBusinessType: WalletBusinessTypes.Expenditure,
                        description: "红包提现",
                        isEnabled: true,
                        isStatic: true),
                new WalletBusiness(
                        id: RedPacketConsts.Send,
                        name: "发红包",
                        walletBusinessType: WalletBusinessTypes.Expenditure,
                        description: "发红包给其他人",
                        isEnabled: true,
                        isStatic: true),
                new WalletBusiness(
                        id: RedPacketConsts.Received,
                        name: "领红包",
                        walletBusinessType: WalletBusinessTypes.Income,
                        description: "领取自己或他人发的红包",
                        isEnabled: true,
                        isStatic: true),
                new WalletBusiness(
                        id: RedPacketConsts.Refund,
                        name: "退回红包",
                        walletBusinessType: WalletBusinessTypes.Income,
                        description: "未领取的红包退回发红包人的账户",
                        isEnabled: true,
                        isStatic: true)
            };

            foreach (var item in list)
            {
                var entity = await Repository.FindAsync(x => x.Id.Equals(item.Id));

                if (entity != null)
                {
                    Logger.LogDebug($"WalletBusiness is ready. item:{item},entity:{entity}");
                    continue;
                }
                entity = await Repository.InsertAsync(item);
                Logger.LogDebug($"Insert WalletBusiness item:{item},entity:{entity}");
            }
        }
    }
}
