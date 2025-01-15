using IczpNet.Chat.Robots.Dtos;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Robots;

public interface IRobotAppService
{
    Task<PagedResultDto<RobotDto>> GetListAsync(RobotGetListInput input);
}
