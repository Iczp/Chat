using AutoMapper;
using IczpNet.Chat.Articles;
using IczpNet.Chat.Articles.Dtos;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Connections;
using IczpNet.Chat.Connections.Dtos;
using IczpNet.Chat.ChatObjectCategorys;
using IczpNet.Chat.ChatObjectCategorys.Dtos;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.ChatObjectTypes.Dtos;
using IczpNet.Chat.Mottos.Dtos;
using IczpNet.Chat.Mottos;
using IczpNet.Chat.Squares.Dtos;
using IczpNet.Chat.Robots.Dtos;
using IczpNet.Chat.ShopKeepers.Dtos;
using IczpNet.Chat.ShopWaiters.Dtos;
using IczpNet.Chat.Words;
using IczpNet.Chat.Words.Dtos;
using IczpNet.Chat.Menus.Dtos;
using IczpNet.Chat.Menus;
using IczpNet.Chat.Developers;
using IczpNet.Chat.Developers.Dtos;

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
            .ForMember(x => x.FriendCount, o => o.MapFrom(x => x.OwnerFriendshipList.Count))
            .ForMember(x => x.InFriendCount, o => o.MapFrom(x => x.DestinationFriendshipList.Count))

            ;
        CreateMap<ChatObjectCreateInput, ChatObject>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<ChatObjectUpdateInput, ChatObject>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        CreateMap<ChatObject, SquareDto>();
        CreateMap<ChatObject, RobotDto>();
        CreateMap<ChatObject, ShopKeeperDto>();
        CreateMap<ChatObject, ShopWaiterDto>();

        CreateMap<Developer, DeveloperDto>().ReverseMap();

        //Menu
        CreateMap<Menu, MenuDto>();
        CreateMap<Menu, MenuSimpleDto>();
        CreateMap<Menu, MenuDetailDto>();
        CreateMap<MenuCreateInput, Menu>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<MenuUpdateInput, Menu>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

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
        //CreateMap<ConnectionUpdateInput, Connection>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();

        //Word
        CreateMap<Word, WordDto>();
        CreateMap<Word, WordDetailDto>();
        CreateMap<WordCreateInput, Word>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<WordUpdateInput, Word>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
    }
}
