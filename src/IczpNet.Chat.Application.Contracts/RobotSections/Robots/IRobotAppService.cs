using IczpNet.Chat.RobotSections.Robots.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.RobotSections.Robots;

public interface IRobotAppService :
    ICrudAppService<
        RobotDetailDto,
        RobotDto,
        Guid,
        RobotGetListInput,
        RobotCreateInput,
        RobotUpdateInput>
{
}
