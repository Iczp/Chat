using AutoMapper;
using IczpNet.Chat.FriendshipTagSections.FriendshipTags.Dtos;
using IczpNet.Chat.SessionSections.FriendshipRequests;
using IczpNet.Chat.SessionSections.FriendshipRequests.Dtos;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.Friendships.Dtos;
using IczpNet.Chat.SessionSections.OpenedRecorders;
using IczpNet.Chat.SessionSections.OpenedRecordes.Dtos;

namespace IczpNet.Chat.AutoMappers;

public class SessionSectionApplicationAutoMapperProfile : Profile
{
    public SessionSectionApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        //Friendship
        CreateMap<Friendship, FriendshipDto>().ForMember(x => x.TagList, o => o.MapFrom(x => x.GetTagList()));
        CreateMap<Friendship, FriendshipDetailDto>().ForMember(x => x.TagList, o => o.MapFrom(x => x.GetTagList()));
        CreateMap<FriendshipCreateInput, Friendship>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<FriendshipUpdateInput, Friendship>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();


        //FriendshipRequest
        CreateMap<FriendshipRequest, FriendshipRequestDto>();
        CreateMap<FriendshipRequest, FriendshipRequestDetailDto>().ForMember(x => x.FriendshipIdList, o => o.MapFrom(x => x.GetFriendshipIdList()));
        CreateMap<FriendshipRequestCreateInput, FriendshipRequest>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<FriendshipRequestUpdateInput, FriendshipRequest>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();

        //FriendshipTag
        CreateMap<FriendshipTag, FriendshipTagDto>().ForMember(x => x.FriendshipCount, o => o.MapFrom(x => x.GetFriendshipCount()));
        CreateMap<FriendshipTag, FriendshipTagDetailDto>().ForMember(x => x.FriendshipCount, o => o.MapFrom(x => x.GetFriendshipCount()));
        CreateMap<FriendshipTag, FriendshipTagSimpleDto>();
        CreateMap<FriendshipTagCreateInput, FriendshipTag>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<FriendshipTagUpdateInput, FriendshipTag>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();


        //OpenedRecorder
        CreateMap<OpenedRecorder, OpenedRecorderDto>();
    }
}
