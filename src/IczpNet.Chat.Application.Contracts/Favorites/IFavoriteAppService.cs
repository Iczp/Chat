using IczpNet.Chat.Favorites.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Favorites
{
    public interface IFavoriteAppService
    {
        Task<PagedResultDto<FavoriteDto>> GetListAsync(FavoriteGetListInput input);

        Task<long> GetSizeAsync(long ownerId);

        Task<int> GetCountAsync(long ownerId);

        Task<DateTime> CreateAsync(FavoriteCreateInput input);

        Task DeleteAsync(Guid sessionUnitId, long messageId);
    }
}
