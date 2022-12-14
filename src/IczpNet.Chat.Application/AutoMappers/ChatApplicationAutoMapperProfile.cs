using AutoMapper;
using IczpNet.Chat.Articles;
using IczpNet.Chat.Articles.Dtos;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Connections;
using IczpNet.Chat.Connections.Dtos;

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
        CreateMap<ChatObject, ChatObjectSimpleDto>();
        CreateMap<ChatObject, ChatObjectDetailDto>()
            .ForMember(x => x.SenderMessageCount, o => o.MapFrom(x => x.SenderMessageList.Count))
            .ForMember(x => x.ReceiverMessageCount, o => o.MapFrom(x => x.ReceiverMessageList.Count))
            .ForMember(x => x.InSquareMemberCount, o => o.MapFrom(x => x.InSquareMemberList.Count))
            .ForMember(x => x.InRoomMemberCount, o => o.MapFrom(x => x.InRoomMemberList.Count))
            .ForMember(x => x.InRoomForbiddenMemberCount, o => o.MapFrom(x => x.InRoomForbiddenMemberList.Count))
            .ForMember(x => x.InOfficialMemberCount, o => o.MapFrom(x => x.InOfficialMemberList.Count))
            .ForMember(x => x.InOfficialGroupMemberCount, o => o.MapFrom(x => x.InOfficialGroupMemberList.Count))
            .ForMember(x => x.InOfficalExcludedMemberCount, o => o.MapFrom(x => x.InOfficalExcludedMemberList.Count))
            .ForMember(x => x.FriendCount, o => o.MapFrom(x => x.OwnerFriendshipList.Count))
            .ForMember(x => x.InFriendCount, o => o.MapFrom(x => x.DestinationFriendshipList.Count))
            .ForMember(x => x.ProxyShopKeeperCount, o => o.MapFrom(x => x.ProxyShopKeeperList.Count))
            .ForMember(x => x.ProxyShopWaiterCount, o => o.MapFrom(x => x.ProxyShopWaiterList.Count))
            ;
        CreateMap<ChatObjectCreateInput, ChatObject>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<ChatObjectUpdateInput, ChatObject>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();



        //Article
        CreateMap<Article, ArticleDto>();
        CreateMap<Article, ArticleDetailDto>();
        CreateMap<ArticleCreateInput, Article>(MemberList.None).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<ArticleUpdateInput, Article>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();

        //Connection
        CreateMap<Connection, ConnectionDto>();
        CreateMap<Connection, ConnectionDetailDto>();
        CreateMap<ConnectionCreateInput, Connection>(MemberList.None).IgnoreAllPropertiesWithAnInaccessibleSetter();
        //CreateMap<ConnectionUpdateInput, Connection>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();

    }
}
