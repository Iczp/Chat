using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjectCategories;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.EntryNames;
using IczpNet.Chat.Menus;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.SessionOrganizations;
using IczpNet.Chat.SessionPermissionGroups;
using IczpNet.Chat.UrlNormalizers;
using Riok.Mapperly.Abstractions;
using System;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainChatObjectToChatObjectInfoMapper(IUrlNormalizer urlNormalizer, IConnectionPoolManager connectionPoolManager)
    : MapperBase<ChatObject, ChatObjectInfo>
{
    public override ChatObjectInfo Map(ChatObject source)
    {
        if (source is null) return default!;
        var destination = new ChatObjectInfo();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObject source, ChatObjectInfo destination)
    {
        if (source is null) return;
        MapFields(source, destination);
        destination.Thumbnail = urlNormalizer.Normalize(source.Thumbnail);
        destination.Portrait = urlNormalizer.Normalize(source.Portrait);
        destination.DeviceTypes = connectionPoolManager.GetDeviceTypesAsync(source.Id).Result;
    }

    [MapperIgnoreTarget(nameof(ChatObjectInfo.Thumbnail))]
    [MapperIgnoreTarget(nameof(ChatObjectInfo.Portrait))]
    [MapperIgnoreTarget(nameof(ChatObjectInfo.DeviceTypes))]
    private partial void MapFields(ChatObject source, ChatObjectInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainHistoryContentInputToHistoryContentMapper
    : MapperBase<HistoryContentInput, HistoryContent>
{
    public override HistoryContent Map(HistoryContentInput source)
    {
        if (source is null) return default!;
        return new HistoryContent(Guid.NewGuid(), string.Empty, string.Empty);
    }

    public override void Map(HistoryContentInput source, HistoryContent destination)
    {
        if (source is null || destination is null) return;
    }
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainRedEnvelopeContentInputToRedEnvelopeContentMapper
    : MapperBase<RedEnvelopeContentInput, RedEnvelopeContent>
{
    public override RedEnvelopeContent Map(RedEnvelopeContentInput source)
    {
        if (source is null) return default!;
        return new RedEnvelopeContent(Guid.NewGuid(), source.GrantMode, source.Amount, source.Count, source.TotalAmount, source.Text);
    }

    public override void Map(RedEnvelopeContentInput source, RedEnvelopeContent destination)
    {
        if (source is null || destination is null) return;
        destination.GrantMode = source.GrantMode;
        destination.Amount = source.Amount;
        destination.Count = source.Count;
        destination.TotalAmount = source.TotalAmount;
        destination.Text = source.Text;
    }
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainRedEnvelopeContentToRedEnvelopeContentOutputMapper
    : MapperBase<RedEnvelopeContent, RedEnvelopeContentOutput>
{
    public override partial RedEnvelopeContentOutput Map(RedEnvelopeContent source);
    public override partial void Map(RedEnvelopeContent source, RedEnvelopeContentOutput destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoMapper
    : MapperBase<Message, MessageInfo>
{
    public override partial MessageInfo Map(Message source);
    public override partial void Map(Message source, MessageInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageAnyInfoMapper
    : MapperBase<Message, MessageAnyInfo>
{
    public override partial MessageAnyInfo Map(Message source);
    public override partial void Map(Message source, MessageAnyInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoIContentInfoMapper
    : MapperBase<Message, MessageInfo<IContentInfo>>
{
    public override partial MessageInfo<IContentInfo> Map(Message source);
    public override partial void Map(Message source, MessageInfo<IContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoDynamicMapper
    : MapperBase<Message, MessageInfo<dynamic>>
{
    public override partial MessageInfo<dynamic> Map(Message source);
    public override partial void Map(Message source, MessageInfo<dynamic> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageWithQuoteInfoMapper
    : MapperBase<Message, MessageWithQuoteInfo>
{
    public override partial MessageWithQuoteInfo Map(Message source);
    public override partial void Map(Message source, MessageWithQuoteInfo destination);
}

// Concrete generic Message mappings for all active content types
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoTextContentInfoMapper
    : MapperBase<Message, MessageInfo<TextContentInfo>>
{
    public override partial MessageInfo<TextContentInfo> Map(Message source);
    public override partial void Map(Message source, MessageInfo<TextContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoImageContentInfoMapper
    : MapperBase<Message, MessageInfo<ImageContentInfo>>
{
    public override partial MessageInfo<ImageContentInfo> Map(Message source);
    public override partial void Map(Message source, MessageInfo<ImageContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoSoundContentInfoMapper
    : MapperBase<Message, MessageInfo<SoundContentInfo>>
{
    public override partial MessageInfo<SoundContentInfo> Map(Message source);
    public override partial void Map(Message source, MessageInfo<SoundContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoVideoContentInfoMapper
    : MapperBase<Message, MessageInfo<VideoContentInfo>>
{
    public override partial MessageInfo<VideoContentInfo> Map(Message source);
    public override partial void Map(Message source, MessageInfo<VideoContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoFileContentInfoMapper
    : MapperBase<Message, MessageInfo<FileContentInfo>>
{
    public override partial MessageInfo<FileContentInfo> Map(Message source);
    public override partial void Map(Message source, MessageInfo<FileContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoLocationContentInfoMapper
    : MapperBase<Message, MessageInfo<LocationContentInfo>>
{
    public override partial MessageInfo<LocationContentInfo> Map(Message source);
    public override partial void Map(Message source, MessageInfo<LocationContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoContactsContentInfoMapper
    : MapperBase<Message, MessageInfo<ContactsContentInfo>>
{
    public override partial MessageInfo<ContactsContentInfo> Map(Message source);
    public override partial void Map(Message source, MessageInfo<ContactsContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoLinkContentInfoMapper
    : MapperBase<Message, MessageInfo<LinkContentInfo>>
{
    public override partial MessageInfo<LinkContentInfo> Map(Message source);
    public override partial void Map(Message source, MessageInfo<LinkContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoHistoryContentOutputMapper
    : MapperBase<Message, MessageInfo<HistoryContentOutput>>
{
    public override partial MessageInfo<HistoryContentOutput> Map(Message source);
    public override partial void Map(Message source, MessageInfo<HistoryContentOutput> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoRedEnvelopeContentOutputMapper
    : MapperBase<Message, MessageInfo<RedEnvelopeContentOutput>>
{
    public override partial MessageInfo<RedEnvelopeContentOutput> Map(Message source);
    public override partial void Map(Message source, MessageInfo<RedEnvelopeContentOutput> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoCmdContentInfoMapper
    : MapperBase<Message, MessageInfo<CmdContentInfo>>
{
    public override partial MessageInfo<CmdContentInfo> Map(Message source);
    public override partial void Map(Message source, MessageInfo<CmdContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoHtmlContentInfoMapper
    : MapperBase<Message, MessageInfo<HtmlContentInfo>>
{
    public override partial MessageInfo<HtmlContentInfo> Map(Message source);
    public override partial void Map(Message source, MessageInfo<HtmlContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoNoticeContentInfoMapper
    : MapperBase<Message, MessageInfo<NoticeContentInfo>>
{
    public override partial MessageInfo<NoticeContentInfo> Map(Message source);
    public override partial void Map(Message source, MessageInfo<NoticeContentInfo> destination);
}

// Tree entities mappings
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainChatObjectCategoryToChatObjectCategoryInfoMapper
    : MapperBase<ChatObjectCategory, ChatObjectCategoryInfo>
{
    public override partial ChatObjectCategoryInfo Map(ChatObjectCategory source);
    public override partial void Map(ChatObjectCategory source, ChatObjectCategoryInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainSessionOrganizationToSessionOrganizationInfoMapper
    : MapperBase<SessionOrganization, SessionOrganizationInfo>
{
    public override partial SessionOrganizationInfo Map(SessionOrganization source);
    public override partial void Map(SessionOrganization source, SessionOrganizationInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainSessionPermissionGroupToSessionPermissionGroupInfoMapper
    : MapperBase<SessionPermissionGroup, SessionPermissionGroupInfo>
{
    public override partial SessionPermissionGroupInfo Map(SessionPermissionGroup source);
    public override partial void Map(SessionPermissionGroup source, SessionPermissionGroupInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMenuToMenuInfoMapper
    : MapperBase<Menu, MenuInfo>
{
    public override partial MenuInfo Map(Menu source);
    public override partial void Map(Menu source, MenuInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainEntryNameToTreeInfoGuidMapper
    : MapperBase<EntryName, TreeInfo<Guid>>
{
    public override partial TreeInfo<Guid> Map(EntryName source);
    public override partial void Map(EntryName source, TreeInfo<Guid> destination);
}
