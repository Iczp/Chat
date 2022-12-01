using AutoMapper;
using IczpNet.Chat.RobotSections.Robots;
using IczpNet.Chat.RobotSections.Robots.Dtos;

namespace IczpNet.Chat.AutoMappers;

public class RobotSectionApplicationAutoMapperProfile : Profile
{
    public RobotSectionApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        //Robot
        CreateMap<Robot, RobotDto>();
        CreateMap<Robot, RobotDetailDto>();
        CreateMap<RobotCreateInput, Robot>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<RobotUpdateInput, Robot>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
    }
}
