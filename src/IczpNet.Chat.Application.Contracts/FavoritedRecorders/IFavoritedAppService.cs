using IczpNet.Chat.FavoritedRecorders.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.FavoritedRecorders
{
    public interface IFavoritedAppService
    {
        Task<PagedResultDto<FavoritedRecorderDto>> GetListAsync(FavoritedRecorderGetListInput input);

        Task<long> GetSizeAsync(long ownerId);

        Task<int> GetCountAsync(long ownerId);

        Task<DateTime> CreateAsync(FavoritedRecorderCreateInput input);

        Task DeleteAsync(Guid sessionUnitId, long messageId);
    }
}
