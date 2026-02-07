using AutoMapper;
using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.SessionBoxes;
using IczpNet.Chat.SessionTags;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.UrlNormalizers;

namespace IczpNet.Chat.AutoMappers;

public class ChatApplicationAutoMapperProfile : Profile
{
    public ChatApplicationAutoMapperProfile()
    {

        CreateMap(typeof(TreeEntity<,>), typeof(TreeInfo<>));

        //SessionUnit
        CreateMap<SessionUnit, SessionUnitCacheItem>();

        //ChatObject
        CreateMap<ChatObject, ChatObjectInfo>()
            //.ForMember(x => x.ServiceStatus, opts => opts.MapFrom<ChatObjectInfoServiceStutusResolver>())
            .ForMember(x => x.DeviceTypes, opts => opts.MapFrom<ChatObjectInfoServiceStutusResolver>())
            .ForMember(x => x.Thumbnail, opts => opts.MapFrom<UrlResolver, string>(x => x.Thumbnail))
            .ForMember(x => x.Portrait, opts => opts.MapFrom<UrlResolver, string>(x => x.Portrait))
            ;

        //Message
        CreateMap<Message, MessageInfo>().MaxDepth(3);
        CreateMap<Message, MessageAnyInfo>()
            .MaxDepth(3)
            .ForMember(x => x.Content, o => o.MapFrom(x => x.GetContentDto()))
            //.ForMember(x => x.SenderSessionUnit, o => o.MapFrom<SenderSessionUnitResolver<MessageAnyInfo>>())
            ;

        CreateMap<Message, MessageCacheItem>()
            .MaxDepth(3)
            .ForMember(x => x.Content, o => o.MapFrom(x => x.GetContentDto()))
            //.ForMember(x => x.SenderSessionUnit, o => o.MapFrom<SenderSessionUnitResolver<MessageAnyInfo>>())
            ;
        CreateMap<Message, MessageQuoteCacheItem>()
            .MaxDepth(3)
            .ForMember(x => x.Content, o => o.MapFrom(x => x.GetContentDto()))
            //.ForMember(x => x.SenderSessionUnit, o => o.MapFrom<SenderSessionUnitResolver<MessageAnyInfo>>())
            ;

        CreateMap<Message, MessageInfo<IContentInfo>>()
            .MaxDepth(3)
            .ForMember(x => x.Content, o => o.MapFrom(x => x.GetContentDto()))
            //.ForMember(x => x.SenderSessionUnit, o => o.MapFrom<SenderSessionUnitResolver<MessageInfo<IContentInfo>>>())
            ;
        CreateMap<Message, MessageInfo<dynamic>>()
            .MaxDepth(3)
            .ForMember(x => x.Content, o => o.MapFrom(x => x.GetContentDto()))
            //.ForMember(x => x.SenderSessionUnit, o => o.MapFrom<SenderSessionUnitResolver<MessageInfo<dynamic>>>())
            ;

        CreateMap<Message, MessageInfo<object>>()
            .MaxDepth(3)
            .ForMember(x => x.Content, o => o.MapFrom(x => x.GetContentDto()))
            //.ForMember(x => x.SenderSessionUnit, o => o.MapFrom<SenderSessionUnitResolver<MessageInfo<dynamic>>>())
            ;

        CreateMap<Message, MessageWithQuoteInfo>().MaxDepth(3);
        CreateMap(typeof(Message), typeof(MessageInfo<>)).MaxDepth(3);
        CreateMap(typeof(Message), typeof(MessageWithQuoteInfo<>)).MaxDepth(3);

        //MessageContent

        CreateMap<TextContentInfo, TextContent>().UsingMessageTemplate()
            .ReverseMap()

            ;
        CreateMap<CmdContentInfo, CmdContent>().UsingMessageTemplate().ReverseMap()

            ;
        CreateMap<HtmlContentInfo, HtmlContent>().UsingMessageTemplate().ReverseMap();
        CreateMap<ContactsContentInfo, ContactsContent>().UsingMessageTemplate().ReverseMap();
        CreateMap<FileContentInfo, FileContent>().UsingMessageTemplate()
            .ReverseMap()
            .ForMember(x => x.ActionUrl, opts => opts.MapFrom<UrlResolver, string>(x => x.ActionUrl))
            .ForMember(x => x.Url, opts => opts.MapFrom<UrlResolver, string>(x => x.Url))
            ;
        CreateMap<ImageContentInfo, ImageContent>().UsingMessageTemplate().ReverseMap()
            .ForMember(x => x.ThumbnailActionUrl, opts => opts.MapFrom<UrlResolver, string>(x => x.ThumbnailActionUrl))
            .ForMember(x => x.Url, opts => opts.MapFrom<UrlResolver, string>(x => x.Url))
            .ForMember(x => x.ActionUrl, opts => opts.MapFrom<UrlResolver, string>(x => x.ActionUrl))
            .ForMember(x => x.ThumbnailUrl, opts => opts.MapFrom<UrlResolver, string>(x => x.ThumbnailUrl))
            ;
        CreateMap<LinkContentInfo, LinkContent>().UsingMessageTemplate()
            .ReverseMap();
        CreateMap<LocationContentInfo, LocationContent>().UsingMessageTemplate().ReverseMap();
        CreateMap<SoundContentInfo, SoundContent>().UsingMessageTemplate().ReverseMap()
            .ForMember(x => x.Url, opts => opts.MapFrom<UrlResolver, string>(x => x.Url))
            ;
        CreateMap<VideoContentInfo, VideoContent>().UsingMessageTemplate().ReverseMap()
            .ForMember(x => x.Url, opts => opts.MapFrom<UrlResolver, string>(x => x.Url))
            .ForMember(x => x.SnapshotUrl, opts => opts.MapFrom<UrlResolver, string>(x => x.SnapshotUrl))
            .ForMember(x => x.SnapshotThumbnailUrl, opts => opts.MapFrom<UrlResolver, string>(x => x.SnapshotThumbnailUrl))
            ;

        CreateMap<HistoryContentInput, HistoryContent>(MemberList.None).UsingMessageTemplate();
        CreateMap<HistoryContent, HistoryContentOutput>();

        CreateMap<RedEnvelopeContentInput, RedEnvelopeContent>().UsingMessageTemplate();
        //RedEnvelope
        CreateMap<RedEnvelopeContent, RedEnvelopeContentOutput>()
          //.ForMember(d => d.Detail, distributedCacheEntryOptions => distributedCacheEntryOptions.MapFrom<DetailResolver>())
          //.ForMember(d => d.IsFinished, distributedCacheEntryOptions => distributedCacheEntryOptions.MapFrom<IsFinishedResolver>())
          ;


        CreateMap<SessionUnit, SessionUnitSenderInfo>();

        CreateMap<SessionUnit, SessionUnitCacheItem>();

        CreateMap<SessionTag, SessionTagCacheItem>();


        //ConnectionPool
        CreateMap<ConnectionPoolCacheItem, DisconnectedEto>().ReverseMap();
        CreateMap<ConnectionPoolCacheItem, ConnectedEto>().ReverseMap();
        CreateMap<ConnectionPoolCacheItem, ActivedEto>().ReverseMap();

        CreateMap<DisconnectedEto, ConnectedEto>().ReverseMap();
        CreateMap<DisconnectedEto, ActivedEto>().ReverseMap();
        CreateMap<ConnectedEto, ActivedEto>().ReverseMap();


        CreateMap<SessionUnitSetting, SessionUnitSettingCacheItem>();

        CreateMap<Box, BoxInfo>();

    }
}
