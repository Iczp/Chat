using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.WalletOrders.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using IczpNet.Chat.Permissions;
using Microsoft.AspNetCore.Mvc;
using IczpNet.AbpCommons.Dtos;

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
        protected IWalletOrderManager WalletOrderManager { get; }

        public WalletOrderAppService(
            IRepository<WalletOrder, Guid> repository,
            IWalletOrderManager walletOrderManager) : base(repository)
        {
            WalletOrderManager = walletOrderManager;
        }

        protected override async Task<IQueryable<WalletOrder>> CreateFilteredQueryAsync(WalletOrderGetListInput input)
        {
            return (await ReadOnlyRepository.GetQueryableAsync())
                .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
                .WhereIf(input.MinAmount.HasValue, x => x.Amount >= input.MinAmount)
                .WhereIf(input.MaxAmount.HasValue, x => x.Amount == input.MaxAmount)
                .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
                .WhereIf(input.IsEnabled.HasValue, x => x.IsEnabled == input.IsEnabled)
                .WhereIf(input.BusinessType.HasValue, x => x.BusinessType == input.BusinessType)
                .WhereIf(!input.BusinessId.IsNullOrEmpty(), x => x.BusinessId == input.BusinessId)
                .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Description.Contains(input.Keyword));
        }

        [HttpPost]
        public override async Task<WalletOrderDetailDto> CreateAsync(WalletOrderCreateInput input)
        {
            var entity = await WalletOrderManager.CreateAsync(
                ownerId: input.OwnerId,
                businessId: input.BusinessId,
                title: input.Title,
                description: input.Title,
                amount: input.Amount);

            return await MapToGetOutputDtoAsync(entity);
        }

        [HttpPost]
        public override async Task<WalletOrderDetailDto> UpdateAsync(Guid id, WalletOrderUpdateInput input)
        {
            var entity = await WalletOrderManager.UpdateAsync(
                orderId: id,
                title: input.Title,
                description: input.Title,
                amount: input.Amount);

            return await MapToGetOutputDtoAsync(entity);
        }

        protected override async Task CheckUpdateAsync(Guid id, WalletOrder entity, WalletOrderUpdateInput input)
        {
            //Assert.If(await Repository.AnyAsync(x => x.Id.Equals(id) && x.Id.Equals(id)), $"Already exists [{input.Value}] name:{id}");
            await base.CheckUpdateAsync(id, entity, input);
        }

        [HttpPost]
        public virtual async Task<WalletOrderDetailDto> CloseAsync(Guid id)
        {
            var entity = await WalletOrderManager.CloseAsync(id);

            return await MapToGetOutputDtoAsync(entity);
        }
    }
}
