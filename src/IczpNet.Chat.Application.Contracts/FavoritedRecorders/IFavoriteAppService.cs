using IczpNet.Chat.FavoritedRecorders.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.FavoritedRecorders
{
    public interface IFavoriteAppService
    {
        Task<PagedResultDto<FavoritedRecorderDto>> GetListAsync(FavoritedRecorderGetListInput input);

        Task<long> GetSizeAsync(long ownerId);

        Task<int> GetCountAsync(long ownerId);

        Task<DateTime> CreateAsync(FavoritedRecorderCreateInput input);

        Task DeleteAsync(FavoritedRecorderDeleteInput input);

        Task<bool> SetAsync(FavoritedRecorderInput input);
    }
}
