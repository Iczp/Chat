using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.ShopWaiters.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.ShopWaiters
{
    public class ShopWaiterAppService : ChatAppService, IShopWaiterAppService
    {
        protected IChatObjectRepository Repository { get; }
        protected IShopWaiterManager ShopWaiterManager { get; }

        public ShopWaiterAppService(IChatObjectRepository repository,
            IShopWaiterManager shopKeeperManager)
        {
            Repository = repository;
            ShopWaiterManager = shopKeeperManager;
        }

        protected virtual Task<ShopWaiterDto> MapToDtoAsync(ChatObject entity)
        {
            return Task.FromResult(MapToDto(entity));
        }

        protected virtual ShopWaiterDto MapToDto(ChatObject entity)
        {
            return ObjectMapper.Map<ChatObject, ShopWaiterDto>(entity);
        }

        [HttpGet]
        public virtual async Task<PagedResultDto<ShopWaiterDto>> GetListAsync(ShopWaiterGetListInput input)
        {
            var query = (await Repository.GetQueryableAsync())
                .Where(x => x.ParentId == input.ShopKeeperId)
                .Where(x => x.ObjectType == ChatObjectTypeEnums.ShopWaiter);

            return await GetPagedListAsync<ChatObject, ShopWaiterDto>(query, input, x => x.OrderBy(d => d.Name));

        }

        [HttpPost]
        public virtual async Task<ShopWaiterDto> CreateAsync(ShopWaiterCreateInput input)
        {
            var entity = await ShopWaiterManager.CreateAsync(input.ShopKeeperId, input.Name);

            return await MapToDtoAsync(entity);
        }

        [HttpPost]
        public virtual async Task<ShopWaiterDto> UpdateAsync(long id, ShopWaiterUpdateInput input)
        {
            var entity = await ShopWaiterManager.UpdateAsync(id, input.Name);

            return await MapToDtoAsync(entity);
        }

        [HttpPost]
        public virtual Task<ShopWaiterDto> BingUserAsync(long id, Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
