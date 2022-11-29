using AutoMapper;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Rooms.Dtos;
using IczpNet.Chat.RoomSections.Rooms;

namespace IczpNet.Chat.AutoMappers;

public class ChatApplicationAutoMapperProfile : Profile
{
    public ChatApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        //ChatObject
        CreateMap<ChatObject, ChatObjectDto>();
        CreateMap<ChatObject, ChatObjectDetailDto>();
        CreateMap<ChatObjectCreateInput, ChatObject>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<ChatObjectUpdateInput, ChatObject>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();

        //Room
        CreateMap<Room, RoomDto>();
        CreateMap<Room, RoomDetailDto>();
        CreateMap<RoomCreateInput, Room>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<RoomUpdateInput, Room>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
    }
}
