using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.ShopWaiters.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

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

        [HttpGet]
        public virtual async Task<PagedResultDto<ShopWaiterDto>> GetListAsync(ShopWaiterGetListInput input)
        {
            var query = (await Repository.GetQueryableAsync())
                .Where(x => x.ParentId == input.ShopKeeperId)
                .Where(x => x.ObjectType == ChatObjectTypeEnums.ShopWaiter);

            return await GetPagedListAsync<ChatObject, ShopWaiterDto>(query, input, x => x.OrderBy(d => d.Name));

        }

        [HttpPost]
        public Task<PagedResultDto<ShopWaiterDto>> CreateAsync(ShopWaiterCreateInput input)
        {
            throw new System.NotImplementedException();
        }
       
        [HttpPost]
        public Task<PagedResultDto<ShopWaiterDto>> UpdateAsync(long id, ShopWaiterUpdateInput input)
        {
            throw new System.NotImplementedException();
        }
    }
}
