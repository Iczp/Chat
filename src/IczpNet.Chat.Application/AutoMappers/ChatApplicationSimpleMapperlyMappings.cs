// Generated from conventional CreateMap declarations. Keep complex maps in the profile until migrated.
using IczpNet.Chat.AiRuns;
using IczpNet.Chat.AppVersions;
using TargetAppVersion = IczpNet.AppDevice.AppVersions.AppVersion;
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
using IczpNet.Chat.DeviceGroups;
using IczpNet.Chat.Devices;
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
using IczpNet.Chat.MessageReports;
using IczpNet.Chat.Mottos;
using IczpNet.Chat.Mottos.Dtos;
using IczpNet.Chat.Robots.Dtos;
using IczpNet.Chat.ScanCodes;
using IczpNet.Chat.ScanLogins;
using IczpNet.Chat.ShopKeepers.Dtos;
using IczpNet.Chat.ShopWaiters.Dtos;
using IczpNet.Chat.Squares.Dtos;
using IczpNet.Chat.Words;
using IczpNet.Chat.Words.Dtos;
using IczpNet.Chat.SessionBoxes;
using IczpNet.Chat.SessionUnitEntryValues;
using IczpNet.Chat.UrlNormalizers;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationChatObjectToSquareDtoMapper : MapperBase<ChatObject, SquareDto>
{
    public override partial SquareDto Map(ChatObject source);

    public override partial void Map(ChatObject source, SquareDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationChatObjectToRobotDtoMapper : MapperBase<ChatObject, RobotDto>
{
    public override partial RobotDto Map(ChatObject source);

    public override partial void Map(ChatObject source, RobotDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationMenuToMenuDtoMapper : MapperBase<Menu, MenuDto>
{
    public override partial MenuDto Map(Menu source);

    public override partial void Map(Menu source, MenuDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationMenuToMenuSimpleDtoMapper : MapperBase<Menu, MenuSimpleDto>
{
    public override partial MenuSimpleDto Map(Menu source);

    public override partial void Map(Menu source, MenuSimpleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationMenuToMenuDetailDtoMapper : MapperBase<Menu, MenuDetailDto>
{
    public override partial MenuDetailDto Map(Menu source);

    public override partial void Map(Menu source, MenuDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationMenuToMenuInfoMapper : MapperBase<Menu, MenuInfo>
{
    public override partial MenuInfo Map(Menu source);

    public override partial void Map(Menu source, MenuInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationMottoToMottoDtoMapper : MapperBase<Motto, MottoDto>
{
    public override partial MottoDto Map(Motto source);

    public override partial void Map(Motto source, MottoDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationMottoToMottoSimpleDtoMapper : MapperBase<Motto, MottoSimpleDto>
{
    public override partial MottoSimpleDto Map(Motto source);

    public override partial void Map(Motto source, MottoSimpleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationMottoToMottoDetailDtoMapper : MapperBase<Motto, MottoDetailDto>
{
    public override partial MottoDetailDto Map(Motto source);

    public override partial void Map(Motto source, MottoDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationInvitationCodeToInvitationCodeDtoMapper : MapperBase<InvitationCode, InvitationCodeDto>
{
    public override partial InvitationCodeDto Map(InvitationCode source);

    public override partial void Map(InvitationCode source, InvitationCodeDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationInvitationCodeToInvitationCodeSimpleDtoMapper : MapperBase<InvitationCode, InvitationCodeSimpleDto>
{
    public override partial InvitationCodeSimpleDto Map(InvitationCode source);

    public override partial void Map(InvitationCode source, InvitationCodeSimpleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationInvitationCodeToInvitationCodeDetailDtoMapper : MapperBase<InvitationCode, InvitationCodeDetailDto>
{
    public override partial InvitationCodeDetailDto Map(InvitationCode source);

    public override partial void Map(InvitationCode source, InvitationCodeDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationChatObjectCategoryToChatObjectCategoryDtoMapper : MapperBase<ChatObjectCategory, ChatObjectCategoryDto>
{
    public override partial ChatObjectCategoryDto Map(ChatObjectCategory source);

    public override partial void Map(ChatObjectCategory source, ChatObjectCategoryDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationChatObjectCategoryToChatObjectCategoryDetailDtoMapper : MapperBase<ChatObjectCategory, ChatObjectCategoryDetailDto>
{
    public override partial ChatObjectCategoryDetailDto Map(ChatObjectCategory source);

    public override partial void Map(ChatObjectCategory source, ChatObjectCategoryDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationChatObjectCategoryToChatObjectCategoryInfoMapper : MapperBase<ChatObjectCategory, ChatObjectCategoryInfo>
{
    public override partial ChatObjectCategoryInfo Map(ChatObjectCategory source);

    public override partial void Map(ChatObjectCategory source, ChatObjectCategoryInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationArticleToArticleDtoMapper : MapperBase<Article, ArticleDto>
{
    public override partial ArticleDto Map(Article source);

    public override partial void Map(Article source, ArticleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationArticleToArticleDetailDtoMapper : MapperBase<Article, ArticleDetailDto>
{
    public override partial ArticleDetailDto Map(Article source);

    public override partial void Map(Article source, ArticleDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationConnectionToConnectionDtoMapper : MapperBase<Connection, ConnectionDto>
{
    public override partial ConnectionDto Map(Connection source);

    public override partial void Map(Connection source, ConnectionDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationConnectionToConnectionDetailDtoMapper : MapperBase<Connection, ConnectionDetailDto>
{
    public override partial ConnectionDetailDto Map(Connection source);

    public override partial void Map(Connection source, ConnectionDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationWordToWordDtoMapper : MapperBase<Word, WordDto>
{
    public override partial WordDto Map(Word source);

    public override partial void Map(Word source, WordDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationWordToWordDetailDtoMapper : MapperBase<Word, WordDetailDto>
{
    public override partial WordDetailDto Map(Word source);

    public override partial void Map(Word source, WordDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationDbTableToDbTableDtoMapper : MapperBase<DbTable, DbTableDto>
{
    public override partial DbTableDto Map(DbTable source);

    public override partial void Map(DbTable source, DbTableDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationEntryNameToEntryNameDtoMapper : MapperBase<EntryName, EntryNameDto>
{
    public override partial EntryNameDto Map(EntryName source);

    public override partial void Map(EntryName source, EntryNameDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationEntryNameToEntryNameSimpleDtoMapper : MapperBase<EntryName, EntryNameSimpleDto>
{
    public override partial EntryNameSimpleDto Map(EntryName source);

    public override partial void Map(EntryName source, EntryNameSimpleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationEntryNameToEntryNameDetailDtoMapper : MapperBase<EntryName, EntryNameDetailDto>
{
    public override partial EntryNameDetailDto Map(EntryName source);

    public override partial void Map(EntryName source, EntryNameDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationEntryValueToEntryValueDtoMapper : MapperBase<EntryValue, EntryValueDto>
{
    public override partial EntryValueDto Map(EntryValue source);

    public override partial void Map(EntryValue source, EntryValueDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationEntryValueToEntryValueSimpleDtoMapper : MapperBase<EntryValue, EntryValueSimpleDto>
{
    public override partial EntryValueSimpleDto Map(EntryValue source);

    public override partial void Map(EntryValue source, EntryValueSimpleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationEntryValueToEntryValueDetailDtoMapper : MapperBase<EntryValue, EntryValueDetailDto>
{
    public override partial EntryValueDetailDto Map(EntryValue source);

    public override partial void Map(EntryValue source, EntryValueDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationContactTagToContactTagDtoMapper : MapperBase<ContactTag, ContactTagDto>
{
    public override partial ContactTagDto Map(ContactTag source);

    public override partial void Map(ContactTag source, ContactTagDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationContactTagToContactTagSimpleDtoMapper : MapperBase<ContactTag, ContactTagSimpleDto>
{
    public override partial ContactTagSimpleDto Map(ContactTag source);

    public override partial void Map(ContactTag source, ContactTagSimpleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationContactTagToContactTagDetailDtoMapper : MapperBase<ContactTag, ContactTagDetailDto>
{
    public override partial ContactTagDetailDto Map(ContactTag source);

    public override partial void Map(ContactTag source, ContactTagDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationBlobToBlobDtoMapper : MapperBase<Blob, BlobDto>
{
    public override partial BlobDto Map(Blob source);

    public override partial void Map(Blob source, BlobDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationBlobToBlobSimpleDtoMapper : MapperBase<Blob, BlobSimpleDto>
{
    public override partial BlobSimpleDto Map(Blob source);

    public override partial void Map(Blob source, BlobSimpleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationBlobToBlobDetailDtoMapper : MapperBase<Blob, BlobDetailDto>
{
    public override partial BlobDetailDto Map(Blob source);

    public override partial void Map(Blob source, BlobDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationConnectionPoolCacheItemToConnectionPoolDtoMapper : TwoWayMapperBase<ConnectionPoolCacheItem, ConnectionPoolDto>
{
    public override partial ConnectionPoolDto Map(ConnectionPoolCacheItem source);

    public override partial void Map(ConnectionPoolCacheItem source, ConnectionPoolDto destination);

    public override partial ConnectionPoolCacheItem ReverseMap(ConnectionPoolDto source);

    public override partial void ReverseMap(ConnectionPoolDto source, ConnectionPoolCacheItem destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationFriendStatusToFriendStatusDtoMapper : TwoWayMapperBase<FriendStatus, FriendStatusDto>
{
    public override partial FriendStatusDto Map(FriendStatus source);

    public override partial void Map(FriendStatus source, FriendStatusDto destination);

    public override partial FriendStatus ReverseMap(FriendStatusDto source);

    public override partial void ReverseMap(FriendStatusDto source, FriendStatus destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationScanCodeToScanCodeDtoMapper : MapperBase<ScanCode, ScanCodeDto>
{
    public override partial ScanCodeDto Map(ScanCode source);

    public override partial void Map(ScanCode source, ScanCodeDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationScanCodeToScanCodeDetailDtoMapper : MapperBase<ScanCode, ScanCodeDetailDto>
{
    public override partial ScanCodeDetailDto Map(ScanCode source);

    public override partial void Map(ScanCode source, ScanCodeDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationScanCodeToScanCodeResultDtoMapper : MapperBase<ScanCode, ScanCodeResultDto>
{
    public override partial ScanCodeResultDto Map(ScanCode source);

    public override partial void Map(ScanCode source, ScanCodeResultDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationScanHandlerToScanHandlerDtoMapper : MapperBase<ScanHandler, ScanHandlerDto>
{
    public override partial ScanHandlerDto Map(ScanHandler source);

    public override partial void Map(ScanHandler source, ScanHandlerDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationScanHandlerToScanHandlerResultDtoMapper : MapperBase<ScanHandler, ScanHandlerResultDto>
{
    public override partial ScanHandlerResultDto Map(ScanHandler source);

    public override partial void Map(ScanHandler source, ScanHandlerResultDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationAppVersionToAppVersionDtoMapper : MapperBase<TargetAppVersion, AppVersionDto>
{
    [MapperIgnoreTarget(nameof(AppVersionDto.Groups))]
    public override partial AppVersionDto Map(TargetAppVersion source);

    [MapperIgnoreTarget(nameof(AppVersionDto.Groups))]
    public override partial void Map(TargetAppVersion source, AppVersionDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationAppVersionToAppVersionSampleDtoMapper : MapperBase<TargetAppVersion, AppVersionSampleDto>
{
    public override partial AppVersionSampleDto Map(TargetAppVersion source);

    public override partial void Map(TargetAppVersion source, AppVersionSampleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationAppVersionToAppVersionDetailDtoMapper : MapperBase<TargetAppVersion, AppVersionDetailDto>
{
    [MapperIgnoreTarget(nameof(AppVersionDto.Groups))]
    public override partial AppVersionDetailDto Map(TargetAppVersion source);

    [MapperIgnoreTarget(nameof(AppVersionDto.Groups))]
    public override partial void Map(TargetAppVersion source, AppVersionDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationDeviceCreateOrUpdateInputToDeviceCreateInputMapper : MapperBase<DeviceCreateOrUpdateInput, DeviceCreateInput>
{
    public override partial DeviceCreateInput Map(DeviceCreateOrUpdateInput source);

    public override partial void Map(DeviceCreateOrUpdateInput source, DeviceCreateInput destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationDeviceCreateOrUpdateInputToDeviceUpdateInputMapper : MapperBase<DeviceCreateOrUpdateInput, DeviceUpdateInput>
{
    public override partial DeviceUpdateInput Map(DeviceCreateOrUpdateInput source);

    public override partial void Map(DeviceCreateOrUpdateInput source, DeviceUpdateInput destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationMessageReportBaseToMessageReportDtoMapper : MapperBase<MessageReportBase, MessageReportDto>
{
    public override partial MessageReportDto Map(MessageReportBase source);

    public override partial void Map(MessageReportBase source, MessageReportDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationBoxToBoxDtoMapper : MapperBase<Box, BoxDto>
{
    public override partial BoxDto Map(Box source);

    public override partial void Map(Box source, BoxDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationBoxToBoxSampleDtoMapper : MapperBase<Box, BoxSampleDto>
{
    public override partial BoxSampleDto Map(Box source);

    public override partial void Map(Box source, BoxSampleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationBoxToBoxDetailDtoMapper : MapperBase<Box, BoxDetailDto>
{
    public override partial BoxDetailDto Map(Box source);

    public override partial void Map(Box source, BoxDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationOnlineFriendInfoToOnlineFriendDtoMapper : MapperBase<OnlineFriendInfo, OnlineFriendDto>
{
    public override partial OnlineFriendDto Map(OnlineFriendInfo source);

    public override partial void Map(OnlineFriendInfo source, OnlineFriendDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationAiRunToAiRunDtoMapper : MapperBase<AiRun, AiRunDto>
{
    public override partial AiRunDto Map(AiRun source);

    public override partial void Map(AiRun source, AiRunDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatApplicationAiRunToAiRunDetailDtoMapper : MapperBase<AiRun, AiRunDetailDto>
{
    public override partial AiRunDetailDto Map(AiRun source);

    public override partial void Map(AiRun source, AiRunDetailDto destination);
}
