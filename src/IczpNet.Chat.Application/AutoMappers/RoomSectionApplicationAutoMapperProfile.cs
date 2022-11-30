using AutoMapper;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.RoomSections.Rooms.Dtos;

namespace IczpNet.Chat.AutoMappers;

public class RoomSectionApplicationAutoMapperProfile : Profile
{
    public RoomSectionApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        //Room
        CreateMap<Room, RoomDto>().ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()));
        CreateMap<Room, RoomDetailDto>().ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()));
        CreateMap<RoomCreateInput, Room>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<RoomUpdateInput, Room>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
    }
}
