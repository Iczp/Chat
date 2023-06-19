using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.WalletRecorders.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.WalletRecorders
{
    public class WalletRecorderAppService
        :
        CrudChatAppService<WalletRecorder,
        WalletRecorderDto,
            WalletRecorderDto,
            Guid,
            WalletRecorderGetListInput>,
        IWalletRecorderAppService
    {
        public WalletRecorderAppService(
            IRepository<WalletRecorder, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<WalletRecorder>> CreateFilteredQueryAsync(WalletRecorderGetListInput input)
        {
            return (await base.CreateFilteredQueryAsync(input))
                .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
                .WhereIf(input.MinAmount.HasValue, x => x.Amount >= input.MinAmount)
                .WhereIf(input.MaxAmount.HasValue, x => x.Amount == input.MaxAmount)
                .WhereIf(!input.BusinessId.IsNullOrEmpty(), x => x.BusinessId == input.BusinessId)
                .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Description.Contains(input.Keyword));
        }


    }
}
