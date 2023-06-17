using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.WalletOrders.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using IczpNet.AbpCommons;
using IczpNet.Chat.Permissions;

namespace IczpNet.Chat.WalletOrders
{
    public class WalletOrderAppService
        : CrudChatAppService<
            WalletOrder,
            WalletOrderDetailDto,
            WalletOrderDto,
            Guid,
            WalletOrderGetListInput,
            WalletOrderCreateInput,
            WalletOrderUpdateInput>,
        IWalletOrderAppService
    {
        protected override string GetPolicyName { get; set; } = ChatPermissions.WalletOrderPermission.Default;
        protected override string GetListPolicyName { get; set; } = ChatPermissions.WalletOrderPermission.Default;
        protected override string CreatePolicyName { get; set; } = ChatPermissions.WalletOrderPermission.Create;
        protected override string UpdatePolicyName { get; set; } = ChatPermissions.WalletOrderPermission.Update;
        protected override string DeletePolicyName { get; set; } = ChatPermissions.WalletOrderPermission.Delete;

        public WalletOrderAppService(IRepository<WalletOrder, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<WalletOrder>> CreateFilteredQueryAsync(WalletOrderGetListInput input)
        {
            return (await ReadOnlyRepository.GetQueryableAsync())
                .WhereIf(input.OwnerId.HasValue, x => x.OwnerId== input.OwnerId)
                .WhereIf(input.MinAmount.HasValue, x => x.Amount >= input.MinAmount)
                .WhereIf(input.MaxAmount.HasValue, x => x.Amount == input.MaxAmount)
                .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
                .WhereIf(input.IsEnabled.HasValue, x => x.IsEnabled == input.IsEnabled)
                .WhereIf(input.WalletBusinessType.HasValue, x => x.WalletBusinessType == input.WalletBusinessType)
                .WhereIf(!input.WalletBusinessId.IsNullOrEmpty(), x => x.WalletBusinessId == input.WalletBusinessId)
                .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Description.Contains(input.Keyword));
        }


        protected override async Task CheckCreateAsync(WalletOrderCreateInput input)
        {
            //Assert.If(await Repository.AnyAsync(x => x.Description.Equals(input.Description)), $"Already exists [{input.Description}] ");
            await base.CheckCreateAsync(input);
        }

        protected override async Task CheckUpdateAsync(Guid id, WalletOrder entity, WalletOrderUpdateInput input)
        {
            //Assert.If(await Repository.AnyAsync(x => x.Id.Equals(id) && x.Id.Equals(id)), $"Already exists [{input.Value}] name:{id}");
            await base.CheckUpdateAsync(id, entity, input);
        }
    }
}
