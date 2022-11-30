using AutoMapper;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;

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
        CreateMap<ChatObject, ChatObjectDetailDto>()
            .ForMember(x => x.SenderMessageCount, o => o.MapFrom(x => x.SenderMessageList.Count))
            .ForMember(x => x.ReceiverMessageCount, o => o.MapFrom(x => x.ReceiverMessageList.Count))
            .ForMember(x => x.InSquareMemberCount, o => o.MapFrom(x => x.InSquareMemberList.Count))
            .ForMember(x => x.InOfficialMemberCount, o => o.MapFrom(x => x.InOfficialMemberList.Count))
            .ForMember(x => x.InOfficialGroupMemberCount, o => o.MapFrom(x => x.InOfficialGroupMemberList.Count))
            .ForMember(x => x.InOfficalExcludedMemberCount, o => o.MapFrom(x => x.InOfficalExcludedMemberList.Count))
            .ForMember(x => x.OwnerFriendCount, o => o.MapFrom(x => x.OwnerFriendList.Count))
            .ForMember(x => x.DestinationFriendCount, o => o.MapFrom(x => x.DestinationFriendList.Count))
            .ForMember(x => x.ProxyShopKeeperCount, o => o.MapFrom(x => x.ProxyShopKeeperList.Count))
            .ForMember(x => x.ProxyShopWaiterCount, o => o.MapFrom(x => x.ProxyShopWaiterList.Count))
            ;
        CreateMap<ChatObjectCreateInput, ChatObject>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<ChatObjectUpdateInput, ChatObject>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();

        

        


    }
}
