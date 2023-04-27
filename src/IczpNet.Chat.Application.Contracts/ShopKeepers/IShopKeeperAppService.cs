using IczpNet.Chat.ShopKeepers.Dtos;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ShopKeepers
{
    public interface IShopKeeperAppService
    {

        Task<PagedResultDto<ShopKeeperDto>> GetListAsync(ShopKeeperGetListInput input);

        Task<PagedResultDto<ShopKeeperDto>> CreateAsync(ShopKeeperCreateInput input);
    }
}
