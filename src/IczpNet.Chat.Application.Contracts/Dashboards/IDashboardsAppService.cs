using IczpNet.Chat.Dashboards.Dtos;
using System.Threading.Tasks;

namespace IczpNet.Chat.Dashboards
{
    public interface IDashboardsAppService
    {
        Task<DashboardsDto> GetProfileAsync();
    }
}
