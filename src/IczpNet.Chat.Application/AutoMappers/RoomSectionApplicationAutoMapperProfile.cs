using AutoMapper;
using IczpNet.Chat.RoomSections.RoomMembers;
using IczpNet.Chat.RoomSections.RoomMembers.Dtos;
using IczpNet.Chat.RoomSections.RoomRoles;
using IczpNet.Chat.RoomSections.RoomRoles.Dtos;
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
        CreateMap<Room, RoomDto>();
        CreateMap<Room, RoomDetailDto>();
        //CreateMap<RoomCreateInput, Room>(MemberList.None).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<RoomUpdateInput, Room>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        //RoomMember
        CreateMap<RoomMember, RoomMemberDto>().ForMember(x => x.RoleList, o => o.MapFrom(x => x.GetRoleList()));
        CreateMap<RoomMember, RoomMemberDetailDto>().ForMember(x => x.RoleList, o => o.MapFrom(x => x.GetRoleList()));

        CreateMap<RoomMemberCreateInput, RoomMember>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<RoomMemberUpdateInput, RoomMember>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        //RoomRole
        CreateMap<RoomRole, RoomRoleDto>().ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()));
        CreateMap<RoomRole, RoomRoleDetailDto>().ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()));
        CreateMap<RoomRole, RoomRoleSampleDto>();
        CreateMap<RoomRoleCreateInput, RoomRole>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<RoomRoleUpdateInput, RoomRole>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
    }
}
