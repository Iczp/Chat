using IczpNet.Chat.ShopWaiters.Dtos;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ShopWaiters
{
    public interface IShopWaiterAppService
    {

        Task<PagedResultDto<ShopWaiterDto>> GetListAsync(ShopWaiterGetListInput input);

        Task<PagedResultDto<ShopWaiterDto>> CreateAsync(ShopWaiterCreateInput input);

        Task<PagedResultDto<ShopWaiterDto>> UpdateAsync(long id, ShopWaiterUpdateInput input);
    }
}
