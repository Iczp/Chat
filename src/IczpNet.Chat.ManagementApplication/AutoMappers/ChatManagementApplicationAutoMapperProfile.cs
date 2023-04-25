using AutoMapper;
using IczpNet.Chat.Articles;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Connections;
using IczpNet.Chat.ChatObjectCategorys;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Mottos;
using IczpNet.Chat.Management.ChatObjects.Dtos;
using IczpNet.Chat.Management.Mottos.Dtos;
using IczpNet.Chat.Management.ChatObjectCategorys.Dtos;
using IczpNet.Chat.Management.ChatObjectTypes.Dtos;
using IczpNet.Chat.Management.Articles.Dtos;
using IczpNet.Chat.Management.Management.Connections.Dtos;
using IczpNet.Chat.Management.Connections.Dtos;

namespace IczpNet.Chat.Management.AutoMappers;

public class ChatManagementApplicationAutoMapperProfile : Profile
{
    public ChatManagementApplicationAutoMapperProfile()
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
            .ForMember(x => x.FriendCount, o => o.MapFrom(x => x.OwnerFriendshipList.Count))
            .ForMember(x => x.InFriendCount, o => o.MapFrom(x => x.DestinationFriendshipList.Count))

            ;
        CreateMap<ChatObjectCreateInput, ChatObject>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<ChatObjectUpdateInput, ChatObject>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        //Motto
        CreateMap<Motto, MottoDto>();
        CreateMap<Motto, MottoSimpleDto>();
        CreateMap<Motto, MottoDetailDto>();
        CreateMap<MottoCreateInput, Motto>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<MottoUpdateInput, Motto>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        //ChatObjectCategory
        CreateMap<ChatObjectCategory, ChatObjectCategoryDto>();
        CreateMap<ChatObjectCategory, ChatObjectCategoryDetailDto>();
        CreateMap<ChatObjectCategoryCreateInput, ChatObjectCategory>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<ChatObjectCategoryUpdateInput, ChatObjectCategory>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<ChatObjectCategory, ChatObjectCategoryInfo>();


        //ChatObjectType
        CreateMap<ChatObjectType, ChatObjectTypeDto>().ForMember(x => x.ChatObjectCount, o => o.MapFrom(x => x.GetChatObjectCount()));
        CreateMap<ChatObjectType, ChatObjectTypeDetailDto>().ForMember(x => x.ChatObjectCount, o => o.MapFrom(x => x.GetChatObjectCount()));
        CreateMap<ChatObjectTypeCreateInput, ChatObjectType>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<ChatObjectTypeUpdateInput, ChatObjectType>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        //Article
        CreateMap<Article, ArticleDto>();
        CreateMap<Article, ArticleDetailDto>();
        CreateMap<ArticleCreateInput, Article>(MemberList.None).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<ArticleUpdateInput, Article>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        //Connection
        CreateMap<Connection, ConnectionDto>();
        CreateMap<Connection, ConnectionDetailDto>();
        CreateMap<ConnectionCreateInput, Connection>(MemberList.None).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();




    }
}
