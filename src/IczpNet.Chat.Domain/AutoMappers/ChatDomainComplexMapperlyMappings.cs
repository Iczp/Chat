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
using IczpNet.Chat.SessionUnits;
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
public partial class ChatDomainMessageToMessageInfoMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageInfo>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

    public override partial MessageInfo Map(Message source);
    public override partial void Map(Message source, MessageInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageAnyInfoMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageAnyInfo>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

    public override MessageAnyInfo Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageAnyInfo();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageAnyInfo destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.Content = source.Content;
    }

    [MapperIgnoreTarget(nameof(MessageAnyInfo.Content))]
    private partial void MapFields(Message source, MessageAnyInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoIContentInfoMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageInfo<IContentInfo>>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

    public override MessageInfo<IContentInfo> Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageInfo<IContentInfo>();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageInfo<IContentInfo> destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        var content = source.Content;
        destination.Content = content is null ? default! : (IContentInfo)content;
    }

    [MapperIgnoreTarget(nameof(MessageInfo<IContentInfo>.Content))]
    private partial void MapFields(Message source, MessageInfo<IContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageWithQuoteInfoMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageWithQuoteInfo>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

    public override partial MessageWithQuoteInfo Map(Message source);
    public override partial void Map(Message source, MessageWithQuoteInfo destination);
}

// Concrete generic Message mappings for all active content types
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoTextContentInfoMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageInfo<TextContentInfo>>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

    public override MessageInfo<TextContentInfo> Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageInfo<TextContentInfo>();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageInfo<TextContentInfo> destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        var content = source.Content;
        destination.Content = content is null ? default! : (TextContentInfo)content;
    }

    [MapperIgnoreTarget(nameof(MessageInfo<TextContentInfo>.Content))]
    private partial void MapFields(Message source, MessageInfo<TextContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoImageContentInfoMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageInfo<ImageContentInfo>>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

    public override MessageInfo<ImageContentInfo> Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageInfo<ImageContentInfo>();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageInfo<ImageContentInfo> destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        var content = source.Content;
        destination.Content = content is null ? default! : (ImageContentInfo)content;
    }

    [MapperIgnoreTarget(nameof(MessageInfo<ImageContentInfo>.Content))]
    private partial void MapFields(Message source, MessageInfo<ImageContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoSoundContentInfoMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageInfo<SoundContentInfo>>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

    public override MessageInfo<SoundContentInfo> Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageInfo<SoundContentInfo>();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageInfo<SoundContentInfo> destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        var content = source.Content;
        destination.Content = content is null ? default! : (SoundContentInfo)content;
    }

    [MapperIgnoreTarget(nameof(MessageInfo<SoundContentInfo>.Content))]
    private partial void MapFields(Message source, MessageInfo<SoundContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoVideoContentInfoMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageInfo<VideoContentInfo>>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

    public override MessageInfo<VideoContentInfo> Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageInfo<VideoContentInfo>();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageInfo<VideoContentInfo> destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        var content = source.Content;
        destination.Content = content is null ? default! : (VideoContentInfo)content;
    }

    [MapperIgnoreTarget(nameof(MessageInfo<VideoContentInfo>.Content))]
    private partial void MapFields(Message source, MessageInfo<VideoContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoFileContentInfoMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageInfo<FileContentInfo>>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

    public override MessageInfo<FileContentInfo> Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageInfo<FileContentInfo>();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageInfo<FileContentInfo> destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        var content = source.Content;
        destination.Content = content is null ? default! : (FileContentInfo)content;
    }

    [MapperIgnoreTarget(nameof(MessageInfo<FileContentInfo>.Content))]
    private partial void MapFields(Message source, MessageInfo<FileContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoLocationContentInfoMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageInfo<LocationContentInfo>>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

    public override MessageInfo<LocationContentInfo> Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageInfo<LocationContentInfo>();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageInfo<LocationContentInfo> destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        var content = source.Content;
        destination.Content = content is null ? default! : (LocationContentInfo)content;
    }

    [MapperIgnoreTarget(nameof(MessageInfo<LocationContentInfo>.Content))]
    private partial void MapFields(Message source, MessageInfo<LocationContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoContactsContentInfoMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageInfo<ContactsContentInfo>>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

    public override MessageInfo<ContactsContentInfo> Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageInfo<ContactsContentInfo>();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageInfo<ContactsContentInfo> destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        var content = source.Content;
        destination.Content = content is null ? default! : (ContactsContentInfo)content;
    }

    [MapperIgnoreTarget(nameof(MessageInfo<ContactsContentInfo>.Content))]
    private partial void MapFields(Message source, MessageInfo<ContactsContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoLinkContentInfoMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageInfo<LinkContentInfo>>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

    public override MessageInfo<LinkContentInfo> Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageInfo<LinkContentInfo>();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageInfo<LinkContentInfo> destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        var content = source.Content;
        destination.Content = content is null ? default! : (LinkContentInfo)content;
    }

    [MapperIgnoreTarget(nameof(MessageInfo<LinkContentInfo>.Content))]
    private partial void MapFields(Message source, MessageInfo<LinkContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoHistoryContentOutputMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageInfo<HistoryContentOutput>>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

    public override MessageInfo<HistoryContentOutput> Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageInfo<HistoryContentOutput>();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageInfo<HistoryContentOutput> destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        var content = source.Content;
        destination.Content = content is null ? default! : (HistoryContentOutput)content;
    }

    [MapperIgnoreTarget(nameof(MessageInfo<HistoryContentOutput>.Content))]
    private partial void MapFields(Message source, MessageInfo<HistoryContentOutput> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoRedEnvelopeContentOutputMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageInfo<RedEnvelopeContentOutput>>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

    public override MessageInfo<RedEnvelopeContentOutput> Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageInfo<RedEnvelopeContentOutput>();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageInfo<RedEnvelopeContentOutput> destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        var content = source.Content;
        destination.Content = content is null ? default! : (RedEnvelopeContentOutput)content;
    }

    [MapperIgnoreTarget(nameof(MessageInfo<RedEnvelopeContentOutput>.Content))]
    private partial void MapFields(Message source, MessageInfo<RedEnvelopeContentOutput> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoCmdContentInfoMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageInfo<CmdContentInfo>>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

    public override MessageInfo<CmdContentInfo> Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageInfo<CmdContentInfo>();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageInfo<CmdContentInfo> destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        var content = source.Content;
        destination.Content = content is null ? default! : (CmdContentInfo)content;
    }

    [MapperIgnoreTarget(nameof(MessageInfo<CmdContentInfo>.Content))]
    private partial void MapFields(Message source, MessageInfo<CmdContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoHtmlContentInfoMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageInfo<HtmlContentInfo>>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

    public override MessageInfo<HtmlContentInfo> Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageInfo<HtmlContentInfo>();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageInfo<HtmlContentInfo> destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        var content = source.Content;
        destination.Content = content is null ? default! : (HtmlContentInfo)content;
    }

    [MapperIgnoreTarget(nameof(MessageInfo<HtmlContentInfo>.Content))]
    private partial void MapFields(Message source, MessageInfo<HtmlContentInfo> destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToMessageInfoNoticeContentInfoMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageInfo<NoticeContentInfo>>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

    public override MessageInfo<NoticeContentInfo> Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageInfo<NoticeContentInfo>();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageInfo<NoticeContentInfo> destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        var content = source.Content;
        destination.Content = content is null ? default! : (NoticeContentInfo)content;
    }

    [MapperIgnoreTarget(nameof(MessageInfo<NoticeContentInfo>.Content))]
    private partial void MapFields(Message source, MessageInfo<NoticeContentInfo> destination);
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
