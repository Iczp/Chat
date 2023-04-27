using IczpNet.Chat.ShopWaiters.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ShopWaiters
{
    public interface IShopWaiterAppService
    {

        Task<PagedResultDto<ShopWaiterDto>> GetListAsync(ShopWaiterGetListInput input);

        Task<ShopWaiterDto> CreateAsync(ShopWaiterCreateInput input);

        Task<ShopWaiterDto> UpdateAsync(long id, ShopWaiterUpdateInput input);

        Task<ShopWaiterDto> BingUserAsync(long id, Guid userId);
    }
}
