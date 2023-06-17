using IczpNet.AbpCommons;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Wallets;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.RedEnvelopes
{
    public class RedPacketManager : DomainService, IRedPacketManager
    {
        protected IRepository<RedEnvelopeContent> Repository { get; }
        protected IRedEnvelopeGenerator RedEnvelopeGenerator { get; }
        protected IWalletManager WalletManager { get; }

        public RedPacketManager(
            IRepository<RedEnvelopeContent> repository,
            IRedEnvelopeGenerator redEnvelopeGenerator,
            IWalletManager walletManager)
        {
            Repository = repository;
            RedEnvelopeGenerator = redEnvelopeGenerator;
            WalletManager = walletManager;
        }

        public async Task<RedEnvelopeContent> CreateAsync(long ownerId, GrantModes grantMode, decimal amount, int count, decimal totalAmount, string text)
        {
            var wallet = await WalletManager.GetWalletAsync(ownerId);

            Assert.If(wallet.AvailableAmount < totalAmount, $"可用余额不足:{totalAmount}");

            await WalletManager.ExpenditureAsync(wallet, ownerId, RedPacketConsts.Send, totalAmount, "");

            var redEnvelope = new RedEnvelopeContent(
                id: GuidGenerator.Create(),
                grantMode: grantMode,
                amount: amount,
                count: count,
                totalAmount: totalAmount,
                text: text
                );

            var redEnvelopeUnitList = await RedEnvelopeGenerator.MakeAsync(redEnvelope.GrantMode, redEnvelope.Id, redEnvelope.Amount, redEnvelope.Count, redEnvelope.TotalAmount);

            redEnvelope.SetRedEnvelopeUnitList(redEnvelopeUnitList);

            return await Repository.InsertAsync(redEnvelope, autoSave: true);
        }
    }
}
