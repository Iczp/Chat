using AutoMapper;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.FriendshipRequests.Dtos;
using IczpNet.Chat.SessionSections.Friendships.Dtos;
using IczpNet.Chat.SessionSections.FriendshipRequests;

namespace IczpNet.Chat.AutoMappers;

public class SessionSectionApplicationAutoMapperProfile : Profile
{
    public SessionSectionApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        //Friendship
        CreateMap<Friendship, FriendshipDto>();
        CreateMap<Friendship, FriendshipDetailDto>();
        CreateMap<FriendshipCreateInput, Friendship>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<FriendshipUpdateInput, Friendship>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();


        //FriendshipRequest
        CreateMap<FriendshipRequest, FriendshipRequestDto>();
        CreateMap<FriendshipRequest, FriendshipRequestDetailDto>();
        CreateMap<FriendshipRequestCreateInput, FriendshipRequest>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<FriendshipRequestUpdateInput, FriendshipRequest>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
    }
}
