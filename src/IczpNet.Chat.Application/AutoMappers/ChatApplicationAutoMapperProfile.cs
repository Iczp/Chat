using AutoMapper;
using IczpNet.Chat.Articles;
using IczpNet.Chat.Articles.Dtos;
using IczpNet.Chat.Blobs;
using IczpNet.Chat.Blobs.Dtos;
using IczpNet.Chat.ChatObjectCategories;
using IczpNet.Chat.ChatObjectCategories.Dtos;
using IczpNet.Chat.ChatObjectEntryValues;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.ChatObjectTypes.Dtos;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.ConnectionPools.Dtos;
using IczpNet.Chat.Connections;
using IczpNet.Chat.Connections.Dtos;
using IczpNet.Chat.ContactTags;
using IczpNet.Chat.ContactTags.Dtos;
using IczpNet.Chat.Dashboards.Dtos;
using IczpNet.Chat.DbTables;
using IczpNet.Chat.Developers;
using IczpNet.Chat.Developers.Dtos;
using IczpNet.Chat.Entries.Dtos;
using IczpNet.Chat.EntryNames;
using IczpNet.Chat.EntryNames.Dtos;
using IczpNet.Chat.EntryValues;
using IczpNet.Chat.EntryValues.Dtos;
using IczpNet.Chat.Friends;
using IczpNet.Chat.InvitationCodes;
using IczpNet.Chat.InvitationCodes.Dtos;
using IczpNet.Chat.Menus;
using IczpNet.Chat.Menus.Dtos;
using IczpNet.Chat.Mottos;
using IczpNet.Chat.Mottos.Dtos;
using IczpNet.Chat.Robots.Dtos;
using IczpNet.Chat.ScanCodes;
using IczpNet.Chat.ScanLogins;
using IczpNet.Chat.SessionSections.SessionUnitEntryValues;
using IczpNet.Chat.ShopKeepers.Dtos;
using IczpNet.Chat.ShopWaiters.Dtos;
using IczpNet.Chat.Squares.Dtos;
using IczpNet.Chat.Words;
using IczpNet.Chat.Words.Dtos;
using Volo.Abp.AutoMapper;
using IczpNet.Chat.AppVersions;
using IczpNet.Chat.DeviceGroups;
using IczpNet.Chat.Devices;
/*---------code-generator-namespace: Do not modify or delete this line of comments--------*/

namespace IczpNet.Chat.AutoMappers;

public class ChatApplicationAutoMapperProfile : Profile
{
    public ChatApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        //ChatObject
        CreateMap<ChatObject, ChatObjectDto>()
            //.ForMember(x => x.ServiceStatus, opts => opts.MapFrom<ChatObjectDtoServiceStutusResolver>())
            .ForMember(x => x.DeviceTypes, opts => opts.MapFrom<ChatObjectDtoServiceStutusResolver>());

        CreateMap<ChatObject, ChatObjectSimpleDto>();
        CreateMap<ChatObject, ChatObjectDetailDto>()
            //.ForMember(x => x.SenderMessageCount, o => o.MapFrom(x => x.SenderMessageList.Count))
            //.ForMember(x => x.ReceiverMessageCount, o => o.MapFrom(x => x.ReceiverMessageList.Count))
            //.ForMember(x => x.FriendCount, o => o.MapFrom(x => x.OwnerFriendshipList.Count))
            //.ForMember(x => x.InFriendCount, o => o.MapFrom(x => x.DestinationFriendshipList.Count))
            ;
        CreateMap<ChatObjectCreateInput, ChatObject>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<ChatObjectUpdateInput, ChatObject>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        CreateMap<ChatObject, SquareDto>();
        CreateMap<ChatObject, RobotDto>();
        CreateMap<ChatObject, ShopKeeperDto>()
            .IncludeBase<ChatObject, ChatObjectDto>();
        CreateMap<ChatObject, ShopWaiterDto>()
            .IncludeBase<ChatObject, ChatObjectDto>();

        CreateMap<Developer, DeveloperDto>().ReverseMap();

        //Menu
        CreateMap<Menu, MenuDto>();
        CreateMap<Menu, MenuSimpleDto>();
        CreateMap<Menu, MenuDetailDto>();
        CreateMap<MenuCreateInput, Menu>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<MenuUpdateInput, Menu>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<Menu, MenuInfo>();

        //Motto
        CreateMap<Motto, MottoDto>();
        CreateMap<Motto, MottoSimpleDto>();
        CreateMap<Motto, MottoDetailDto>();
        CreateMap<MottoCreateInput, Motto>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<MottoUpdateInput, Motto>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        //InvitationCode
        CreateMap<InvitationCode, InvitationCodeDto>();
        CreateMap<InvitationCode, InvitationCodeSimpleDto>();
        CreateMap<InvitationCode, InvitationCodeDetailDto>();
        CreateMap<InvitationCodeCreateInput, InvitationCode>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<InvitationCodeUpdateInput, InvitationCode>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

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
        CreateMap<ConnectionUpdateInput, Connection>(MemberList.None).IgnoreAllPropertiesWithAnInaccessibleSetter();

        //Word
        CreateMap<Word, WordDto>();
        CreateMap<Word, WordDetailDto>();
        CreateMap<WordCreateInput, Word>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<WordUpdateInput, Word>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        //DbTable
        CreateMap<DbTable, DbTableDto>();


        //EntryName
        CreateMap<EntryName, EntryNameDto>();
        CreateMap<EntryName, EntryNameSimpleDto>();
        CreateMap<EntryName, EntryNameDetailDto>();
        CreateMap<EntryNameCreateInput, EntryName>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<EntryNameUpdateInput, EntryName>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        //EntryValue
        CreateMap<EntryValue, EntryValueDto>();
        CreateMap<EntryValue, EntryValueSimpleDto>();
        CreateMap<EntryValue, EntryValueDetailDto>();
        CreateMap<EntryValueCreateInput, EntryValue>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<EntryValueUpdateInput, EntryValue>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        //SessionUnitEntryValue
        CreateMap<SessionUnitEntryValue, EntryObjectDto>()
           .ForMember(x => x.EntryName, o => o.MapFrom(x => x.EntryValue.EntryName))
           ;

        //ChatObjectEntryValue
        CreateMap<ChatObjectEntryValue, EntryObjectDto>()
           .ForMember(x => x.EntryName, o => o.MapFrom(x => x.EntryValue.EntryName))
           ;

        CreateMap<ChatObjectEntryValue, EntrySimpleDto>()
           .ForMember(x => x.Name, o => o.MapFrom(x => x.EntryValue.EntryName.Name))
           .ForMember(x => x.Value, o => o.MapFrom(x => x.EntryValue.Value))
           ;

        ////ChatObjectTargetEntryValue
        //CreateMap<ChatObjectTargetEntryValue, EntryObjectDto>()
        //   .ForMember(x => x.EntryName, o => o.MapFrom(x => x.EntryValue.EntryName))
        //   ;

        //ContactTag
        CreateMap<ContactTag, ContactTagDto>();
        CreateMap<ContactTag, ContactTagSimpleDto>();
        CreateMap<ContactTag, ContactTagDetailDto>();
        CreateMap<ContactTagCreateInput, ContactTag>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<ContactTagUpdateInput, ContactTag>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        // Blob
        CreateMap<Blob, BlobDto>();
        CreateMap<Blob, BlobSimpleDto>();
        CreateMap<Blob, BlobDetailDto>();
        CreateMap<BlobCreateInput, Blob>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<BlobUpdateInput, Blob>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();


        // ConnectionPoolCacheItem
        CreateMap<ConnectionPoolCacheItem, ConnectionPoolDto>().ReverseMap();

        // FriendStatus
        CreateMap<FriendStatus, FriendStatusDto>().ReverseMap();

        // Scan Login
        CreateMap<GenerateInfo, ScannedDto>().Ignore(x => x.ConnectionPool);
        CreateMap<GenerateInfo, GeneratedDto>().ReverseMap();

        // ScanCode
        CreateMap<ScanCode, ScanCodeDto>();
        CreateMap<ScanCode, ScanCodeDetailDto>();
        CreateMap<ScanCode, ScanCodeResultDto>();
        CreateMap<ScanHandler, ScanHandlerDto>();
        CreateMap<ScanHandler, ScanHandlerResultDto>();

        
        //AppVersion
        CreateMap<AppVersion, AppVersionDto>();
        CreateMap<AppVersion, AppVersionSampleDto>();
        CreateMap<AppVersion, AppVersionDetailDto>();
        CreateMap<AppVersionCreateInput, AppVersion>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<AppVersionUpdateInput, AppVersion>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        

        
        //DeviceGroup
        CreateMap<DeviceGroup, DeviceGroupDto>();
        CreateMap<DeviceGroup, DeviceGroupSampleDto>();
        CreateMap<DeviceGroup, DeviceGroupDetailDto>();
        CreateMap<DeviceGroupCreateInput, DeviceGroup>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<DeviceGroupUpdateInput, DeviceGroup>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        

        
        //Device
        CreateMap<Device, DeviceDto>();
        CreateMap<Device, DeviceSampleDto>();
        CreateMap<Device, DeviceDetailDto>();
        CreateMap<DeviceCreateInput, Device>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<DeviceUpdateInput, Device>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        

        /*---------code-generator-mapper: Do not modify or delete this line of comments--------*/
    }
}
