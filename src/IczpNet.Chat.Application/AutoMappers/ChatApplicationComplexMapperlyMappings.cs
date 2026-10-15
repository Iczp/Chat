using IczpNet.Chat.AiRuns;
using IczpNet.Chat.AppVersions;
using IczpNet.Chat.Articles;
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
using IczpNet.Chat.Connections;
using IczpNet.Chat.Connections.Dtos;
using IczpNet.Chat.ContactTags;
using IczpNet.Chat.Articles.Dtos;
using IczpNet.Chat.ContactTags.Dtos;
using IczpNet.Chat.Developers;
using IczpNet.Chat.Developers.Dtos;
using IczpNet.Chat.DeviceGroups;
using IczpNet.Chat.Devices;
using IczpNet.Chat.Entries.Dtos;
using IczpNet.Chat.EntryNames;
using IczpNet.Chat.EntryNames.Dtos;
using IczpNet.Chat.EntryValues;
using IczpNet.Chat.EntryValues.Dtos;
using IczpNet.Chat.InvitationCodes;
using IczpNet.Chat.InvitationCodes.Dtos;
using IczpNet.Chat.Mottos;
using IczpNet.Chat.Mottos.Dtos;
using IczpNet.Chat.ScanLogins;
using IczpNet.Chat.SessionBoxes;
using IczpNet.Chat.ShopKeepers.Dtos;
using IczpNet.Chat.ShopWaiters.Dtos;
using IczpNet.Chat.UrlNormalizers;
using IczpNet.Chat.Words;
using IczpNet.Chat.Words.Dtos;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationAiRunCreateInputToAiRunMapper
    : MapperBase<AiRunCreateInput, AiRun>
{
    public override AiRun Map(AiRunCreateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<AiRun>();
        Map(source, destination);
        return destination;
    }

    public override void Map(AiRunCreateInput source, AiRun destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(AiRunCreateInput source, AiRun destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationAiRunUpdateInputToAiRunMapper
    : MapperBase<AiRunUpdateInput, AiRun>
{
    public override AiRun Map(AiRunUpdateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<AiRun>();
        Map(source, destination);
        return destination;
    }

    public override void Map(AiRunUpdateInput source, AiRun destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(AiRunUpdateInput source, AiRun destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationAppVersionCreateInputToAppVersionMapper
    : MapperBase<AppVersionCreateInput, AppVersion>
{
    public override AppVersion Map(AppVersionCreateInput source)
    {
        if (source is null) return default!;
        var destination = new AppVersion();
        Map(source, destination);
        return destination;
    }

    public override void Map(AppVersionCreateInput source, AppVersion destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(AppVersionCreateInput source, AppVersion destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationAppVersionUpdateInputToAppVersionMapper
    : MapperBase<AppVersionUpdateInput, AppVersion>
{
    public override AppVersion Map(AppVersionUpdateInput source)
    {
        if (source is null) return default!;
        var destination = new AppVersion();
        Map(source, destination);
        return destination;
    }

    public override void Map(AppVersionUpdateInput source, AppVersion destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(AppVersionUpdateInput source, AppVersion destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationArticleCreateInputToArticleMapper
    : MapperBase<ArticleCreateInput, Article>
{
    public override Article Map(ArticleCreateInput source)
    {
        if (source is null) return default!;
        var destination = new Article();
        Map(source, destination);
        return destination;
    }

    public override void Map(ArticleCreateInput source, Article destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(ArticleCreateInput source, Article destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationArticleUpdateInputToArticleMapper
    : MapperBase<ArticleUpdateInput, Article>
{
    public override Article Map(ArticleUpdateInput source)
    {
        if (source is null) return default!;
        var destination = new Article();
        Map(source, destination);
        return destination;
    }

    public override void Map(ArticleUpdateInput source, Article destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(ArticleUpdateInput source, Article destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationBlobCreateInputToBlobMapper
    : MapperBase<BlobCreateInput, Blob>
{
    public override Blob Map(BlobCreateInput source)
    {
        if (source is null) return default!;
        var destination = new Blob();
        Map(source, destination);
        return destination;
    }

    public override void Map(BlobCreateInput source, Blob destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(BlobCreateInput source, Blob destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationBlobUpdateInputToBlobMapper
    : MapperBase<BlobUpdateInput, Blob>
{
    public override Blob Map(BlobUpdateInput source)
    {
        if (source is null) return default!;
        var destination = new Blob();
        Map(source, destination);
        return destination;
    }

    public override void Map(BlobUpdateInput source, Blob destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(BlobUpdateInput source, Blob destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationBoxCreateInputToBoxMapper
    : MapperBase<BoxCreateInput, Box>
{
    public override Box Map(BoxCreateInput source)
    {
        if (source is null) return default!;
        var destination = new Box();
        Map(source, destination);
        return destination;
    }

    public override void Map(BoxCreateInput source, Box destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(BoxCreateInput source, Box destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationBoxUpdateInputToBoxMapper
    : MapperBase<BoxUpdateInput, Box>
{
    public override Box Map(BoxUpdateInput source)
    {
        if (source is null) return default!;
        var destination = new Box();
        Map(source, destination);
        return destination;
    }

    public override void Map(BoxUpdateInput source, Box destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(BoxUpdateInput source, Box destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationBoxInfoToBoxDtoMapper
    : MapperBase<BoxInfo, BoxDto>
{
    public override BoxDto Map(BoxInfo source)
    {
        if (source is null) return default!;
        var destination = new BoxDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(BoxInfo source, BoxDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        ExtraPropertiesMapperHelper.CopyExtraProperties(source, destination);
    }

    [MapperIgnoreTarget(nameof(BoxDto.ExtraProperties))]
    private partial void MapFields(BoxInfo source, BoxDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationChatObjectToChatObjectDtoMapper(IUrlNormalizer urlNormalizer, IConnectionPoolManager connectionPoolManager)
    : MapperBase<ChatObject, ChatObjectDto>
{
    public override ChatObjectDto Map(ChatObject source)
    {
        if (source is null) return default!;
        var destination = new ChatObjectDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObject source, ChatObjectDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.Thumbnail = urlNormalizer.Normalize(source.Thumbnail);
        destination.Portrait = urlNormalizer.Normalize(source.Portrait);
        destination.DeviceTypes = connectionPoolManager.GetDeviceTypesAsync(source.Id).Result;
    }

    [MapperIgnoreTarget(nameof(ChatObjectDto.Thumbnail))]
    [MapperIgnoreTarget(nameof(ChatObjectDto.Portrait))]
    [MapperIgnoreTarget(nameof(ChatObjectDto.DeviceTypes))]
    private partial void MapFields(ChatObject source, ChatObjectDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationChatObjectToChatObjectSimpleDtoMapper(IUrlNormalizer urlNormalizer)
    : MapperBase<ChatObject, ChatObjectSimpleDto>
{
    public override ChatObjectSimpleDto Map(ChatObject source)
    {
        if (source is null) return default!;
        var destination = new ChatObjectSimpleDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObject source, ChatObjectSimpleDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.Thumbnail = urlNormalizer.Normalize(source.Thumbnail);
        destination.Portrait = urlNormalizer.Normalize(source.Portrait);
    }

    [MapperIgnoreTarget(nameof(ChatObjectSimpleDto.Thumbnail))]
    [MapperIgnoreTarget(nameof(ChatObjectSimpleDto.Portrait))]
    private partial void MapFields(ChatObject source, ChatObjectSimpleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationChatObjectToChatObjectDetailDtoMapper(IUrlNormalizer urlNormalizer)
    : MapperBase<ChatObject, ChatObjectDetailDto>
{
    public override ChatObjectDetailDto Map(ChatObject source)
    {
        if (source is null) return default!;
        var destination = new ChatObjectDetailDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObject source, ChatObjectDetailDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.Thumbnail = urlNormalizer.Normalize(source.Thumbnail);
        destination.Portrait = urlNormalizer.Normalize(source.Portrait);
    }

    [MapperIgnoreTarget(nameof(ChatObjectDetailDto.Thumbnail))]
    [MapperIgnoreTarget(nameof(ChatObjectDetailDto.Portrait))]
    private partial void MapFields(ChatObject source, ChatObjectDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationChatObjectToShopKeeperDtoMapper(IUrlNormalizer urlNormalizer, IConnectionPoolManager connectionPoolManager)
    : MapperBase<ChatObject, ShopKeeperDto>
{
    public override ShopKeeperDto Map(ChatObject source)
    {
        if (source is null) return default!;
        var destination = new ShopKeeperDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObject source, ShopKeeperDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.Thumbnail = urlNormalizer.Normalize(source.Thumbnail);
        destination.Portrait = urlNormalizer.Normalize(source.Portrait);
        destination.DeviceTypes = connectionPoolManager.GetDeviceTypesAsync(source.Id).Result;
    }

    [MapperIgnoreTarget(nameof(ShopKeeperDto.Thumbnail))]
    [MapperIgnoreTarget(nameof(ShopKeeperDto.Portrait))]
    [MapperIgnoreTarget(nameof(ShopKeeperDto.DeviceTypes))]
    private partial void MapFields(ChatObject source, ShopKeeperDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationChatObjectToShopWaiterDtoMapper(IUrlNormalizer urlNormalizer, IConnectionPoolManager connectionPoolManager)
    : MapperBase<ChatObject, ShopWaiterDto>
{
    public override ShopWaiterDto Map(ChatObject source)
    {
        if (source is null) return default!;
        var destination = new ShopWaiterDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObject source, ShopWaiterDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.Thumbnail = urlNormalizer.Normalize(source.Thumbnail);
        destination.Portrait = urlNormalizer.Normalize(source.Portrait);
        destination.DeviceTypes = connectionPoolManager.GetDeviceTypesAsync(source.Id).Result;
    }

    [MapperIgnoreTarget(nameof(ShopWaiterDto.Thumbnail))]
    [MapperIgnoreTarget(nameof(ShopWaiterDto.Portrait))]
    [MapperIgnoreTarget(nameof(ShopWaiterDto.DeviceTypes))]
    private partial void MapFields(ChatObject source, ShopWaiterDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationChatObjectCategoryCreateInputToChatObjectCategoryMapper
    : MapperBase<ChatObjectCategoryCreateInput, ChatObjectCategory>
{
    public override ChatObjectCategory Map(ChatObjectCategoryCreateInput source)
    {
        if (source is null) return default!;
        var destination = new ChatObjectCategory();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObjectCategoryCreateInput source, ChatObjectCategory destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(ChatObjectCategoryCreateInput source, ChatObjectCategory destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationChatObjectCategoryUpdateInputToChatObjectCategoryMapper
    : MapperBase<ChatObjectCategoryUpdateInput, ChatObjectCategory>
{
    public override ChatObjectCategory Map(ChatObjectCategoryUpdateInput source)
    {
        if (source is null) return default!;
        var destination = new ChatObjectCategory();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObjectCategoryUpdateInput source, ChatObjectCategory destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(ChatObjectCategoryUpdateInput source, ChatObjectCategory destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationChatObjectEntryValueToEntryObjectDtoMapper
    : MapperBase<ChatObjectEntryValue, EntryObjectDto>
{
    public override EntryObjectDto Map(ChatObjectEntryValue source)
    {
        if (source is null) return default!;
        return new EntryObjectDto
        {
            EntryName = MapEntryName(source.EntryValue?.EntryName),
            EntryValue = MapEntryValue(source.EntryValue)
        };
    }

    public override void Map(ChatObjectEntryValue source, EntryObjectDto destination)
    {
        if (source is null || destination is null) return;
        destination.EntryName = MapEntryName(source.EntryValue?.EntryName);
        destination.EntryValue = MapEntryValue(source.EntryValue);
    }

    private partial EntryNameSimpleDto MapEntryName(EntryName source);
    private partial EntryValueSimpleDto MapEntryValue(EntryValue source);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationChatObjectEntryValueToEntrySimpleDtoMapper
    : MapperBase<ChatObjectEntryValue, EntrySimpleDto>
{
    public override EntrySimpleDto Map(ChatObjectEntryValue source)
    {
        if (source is null) return default!;
        return new EntrySimpleDto
        {
            Name = source.EntryValue?.EntryName?.Name,
            Value = source.EntryValue?.Value
        };
    }

    public override void Map(ChatObjectEntryValue source, EntrySimpleDto destination)
    {
        if (source is null || destination is null) return;
        destination.Name = source.EntryValue?.EntryName?.Name;
        destination.Value = source.EntryValue?.Value;
    }
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationChatObjectTypeToChatObjectTypeDtoMapper
    : MapperBase<ChatObjectType, ChatObjectTypeDto>
{
    public override ChatObjectTypeDto Map(ChatObjectType source)
    {
        if (source is null) return default!;
        var destination = new ChatObjectTypeDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObjectType source, ChatObjectTypeDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.ChatObjectCount = source.GetChatObjectCount();
    }

    [MapperIgnoreTarget(nameof(ChatObjectTypeDto.ChatObjectCount))]
    private partial void MapFields(ChatObjectType source, ChatObjectTypeDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationChatObjectTypeToChatObjectTypeDetailDtoMapper
    : MapperBase<ChatObjectType, ChatObjectTypeDetailDto>
{
    public override ChatObjectTypeDetailDto Map(ChatObjectType source)
    {
        if (source is null) return default!;
        var destination = new ChatObjectTypeDetailDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObjectType source, ChatObjectTypeDetailDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.ChatObjectCount = source.GetChatObjectCount();
    }

    [MapperIgnoreTarget(nameof(ChatObjectTypeDetailDto.ChatObjectCount))]
    private partial void MapFields(ChatObjectType source, ChatObjectTypeDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationConnectionCreateInputToConnectionMapper
    : MapperBase<ConnectionCreateInput, Connection>
{
    public override Connection Map(ConnectionCreateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<Connection>();
        Map(source, destination);
        return destination;
    }

    public override void Map(ConnectionCreateInput source, Connection destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(ConnectionCreateInput source, Connection destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationConnectionUpdateInputToConnectionMapper
    : MapperBase<ConnectionUpdateInput, Connection>
{
    public override Connection Map(ConnectionUpdateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<Connection>();
        Map(source, destination);
        return destination;
    }

    public override void Map(ConnectionUpdateInput source, Connection destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(ConnectionUpdateInput source, Connection destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationContactTagCreateInputToContactTagMapper
    : MapperBase<ContactTagCreateInput, ContactTag>
{
    public override ContactTag Map(ContactTagCreateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<ContactTag>();
        Map(source, destination);
        return destination;
    }

    public override void Map(ContactTagCreateInput source, ContactTag destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(ContactTagCreateInput source, ContactTag destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationContactTagUpdateInputToContactTagMapper
    : MapperBase<ContactTagUpdateInput, ContactTag>
{
    public override ContactTag Map(ContactTagUpdateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<ContactTag>();
        Map(source, destination);
        return destination;
    }

    public override void Map(ContactTagUpdateInput source, ContactTag destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(ContactTagUpdateInput source, ContactTag destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationDeveloperToDeveloperDtoMapper
    : TwoWayMapperBase<Developer, DeveloperDto>
{
    public override partial DeveloperDto Map(Developer source);
    public override partial void Map(Developer source, DeveloperDto destination);
    public override partial Developer ReverseMap(DeveloperDto source);
    public override partial void ReverseMap(DeveloperDto source, Developer destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationDeviceCreateInputToDeviceMapper
    : MapperBase<DeviceCreateInput, Device>
{
    public override Device Map(DeviceCreateInput source)
    {
        if (source is null) return default!;
        var destination = new Device();
        Map(source, destination);
        return destination;
    }

    public override void Map(DeviceCreateInput source, Device destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(DeviceCreateInput source, Device destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationDeviceUpdateInputToDeviceMapper
    : MapperBase<DeviceUpdateInput, Device>
{
    public override Device Map(DeviceUpdateInput source)
    {
        if (source is null) return default!;
        var destination = new Device();
        Map(source, destination);
        return destination;
    }

    public override void Map(DeviceUpdateInput source, Device destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    [MapperIgnoreTarget(nameof(Device.ConcurrencyStamp))]
    private partial void MapPublicProperties(DeviceUpdateInput source, Device destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationDeviceGroupCreateInputToDeviceGroupMapper
    : MapperBase<DeviceGroupCreateInput, DeviceGroup>
{
    public override DeviceGroup Map(DeviceGroupCreateInput source)
    {
        if (source is null) return default!;
        var destination = new DeviceGroup();
        Map(source, destination);
        return destination;
    }

    public override void Map(DeviceGroupCreateInput source, DeviceGroup destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    [MapperIgnoreTarget(nameof(DeviceGroup.ConcurrencyStamp))]
    private partial void MapPublicProperties(DeviceGroupCreateInput source, DeviceGroup destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationDeviceGroupUpdateInputToDeviceGroupMapper
    : MapperBase<DeviceGroupUpdateInput, DeviceGroup>
{
    public override DeviceGroup Map(DeviceGroupUpdateInput source)
    {
        if (source is null) return default!;
        var destination = new DeviceGroup();
        Map(source, destination);
        return destination;
    }

    public override void Map(DeviceGroupUpdateInput source, DeviceGroup destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    [MapperIgnoreTarget(nameof(DeviceGroup.ConcurrencyStamp))]
    private partial void MapPublicProperties(DeviceGroupUpdateInput source, DeviceGroup destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationEntryNameCreateInputToEntryNameMapper
    : MapperBase<EntryNameCreateInput, EntryName>
{
    public override EntryName Map(EntryNameCreateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<EntryName>();
        Map(source, destination);
        return destination;
    }

    public override void Map(EntryNameCreateInput source, EntryName destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(EntryNameCreateInput source, EntryName destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationEntryNameUpdateInputToEntryNameMapper
    : MapperBase<EntryNameUpdateInput, EntryName>
{
    public override EntryName Map(EntryNameUpdateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<EntryName>();
        Map(source, destination);
        return destination;
    }

    public override void Map(EntryNameUpdateInput source, EntryName destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(EntryNameUpdateInput source, EntryName destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationEntryValueCreateInputToEntryValueMapper
    : MapperBase<EntryValueCreateInput, EntryValue>
{
    public override EntryValue Map(EntryValueCreateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<EntryValue>();
        Map(source, destination);
        return destination;
    }

    public override void Map(EntryValueCreateInput source, EntryValue destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(EntryValueCreateInput source, EntryValue destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationEntryValueUpdateInputToEntryValueMapper
    : MapperBase<EntryValueUpdateInput, EntryValue>
{
    public override EntryValue Map(EntryValueUpdateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<EntryValue>();
        Map(source, destination);
        return destination;
    }

    public override void Map(EntryValueUpdateInput source, EntryValue destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(EntryValueUpdateInput source, EntryValue destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationGenerateInfoToGeneratedDtoMapper
    : TwoWayMapperBase<GenerateInfo, GeneratedDto>
{
    public override partial GeneratedDto Map(GenerateInfo source);
    public override partial void Map(GenerateInfo source, GeneratedDto destination);
    public override partial GenerateInfo ReverseMap(GeneratedDto source);
    public override partial void ReverseMap(GeneratedDto source, GenerateInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationGenerateInfoToScannedDtoMapper
    : MapperBase<GenerateInfo, ScannedDto>
{
    public override partial ScannedDto Map(GenerateInfo source);
    public override partial void Map(GenerateInfo source, ScannedDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationInvitationCodeCreateInputToInvitationCodeMapper
    : MapperBase<InvitationCodeCreateInput, InvitationCode>
{
    public override InvitationCode Map(InvitationCodeCreateInput source)
    {
        if (source is null) return default!;
        var destination = new InvitationCode();
        Map(source, destination);
        return destination;
    }

    public override void Map(InvitationCodeCreateInput source, InvitationCode destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(InvitationCodeCreateInput source, InvitationCode destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationInvitationCodeUpdateInputToInvitationCodeMapper
    : MapperBase<InvitationCodeUpdateInput, InvitationCode>
{
    public override InvitationCode Map(InvitationCodeUpdateInput source)
    {
        if (source is null) return default!;
        var destination = new InvitationCode();
        Map(source, destination);
        return destination;
    }

    public override void Map(InvitationCodeUpdateInput source, InvitationCode destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(InvitationCodeUpdateInput source, InvitationCode destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationMottoCreateInputToMottoMapper
    : MapperBase<MottoCreateInput, Motto>
{
    public override Motto Map(MottoCreateInput source)
    {
        if (source is null) return default!;
        var destination = new Motto();
        Map(source, destination);
        return destination;
    }

    public override void Map(MottoCreateInput source, Motto destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(MottoCreateInput source, Motto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationMottoUpdateInputToMottoMapper
    : MapperBase<MottoUpdateInput, Motto>
{
    public override Motto Map(MottoUpdateInput source)
    {
        if (source is null) return default!;
        var destination = new Motto();
        Map(source, destination);
        return destination;
    }

    public override void Map(MottoUpdateInput source, Motto destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(MottoUpdateInput source, Motto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationWordCreateInputToWordMapper
    : MapperBase<WordCreateInput, Word>
{
    public override Word Map(WordCreateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<Word>();
        Map(source, destination);
        return destination;
    }

    public override void Map(WordCreateInput source, Word destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(WordCreateInput source, Word destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationWordUpdateInputToWordMapper
    : MapperBase<WordUpdateInput, Word>
{
    public override Word Map(WordUpdateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<Word>();
        Map(source, destination);
        return destination;
    }

    public override void Map(WordUpdateInput source, Word destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(WordUpdateInput source, Word destination);
}
