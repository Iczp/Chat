using IczpNet.Chat.SessionSections.SessionUnits;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionUnits;

public interface ISessionUnitCacheAppService
{
    Task<PagedResultDto<SessionUnitCacheItem>> GetListAsync(SessionUnitCacheItemGetListInput input);

   
}
