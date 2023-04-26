using IczpNet.Chat.Robots.Dtos;
using System.Threading.Tasks;

namespace IczpNet.Chat.Robots
{
    public interface IRobotManagementAppService
    {
        Task<RobotDto> CreateAsync(RobotCreateInput input);

        Task<RobotDto> UpdateAsync(long id, RobotUpdateInput input);
    }
}
