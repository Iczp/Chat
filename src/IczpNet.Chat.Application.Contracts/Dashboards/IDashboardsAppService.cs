using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Dashboards.Dtos;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Dashboards;

public interface IDashboardsAppService
{
    Task<DashboardsDto> GetProfileAsync();

    //Task<PagedResultDto<DbTableDto>> GetListTableRowAsync(GetListInput input);

    Task<PagedResultDto<DbTableDto>> GetListDbTablesAsync(GetListInput input);
}
