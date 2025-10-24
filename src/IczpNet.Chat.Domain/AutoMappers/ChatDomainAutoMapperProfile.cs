using AutoMapper;
using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnits;

namespace IczpNet.Chat.AutoMappers;

public class ChatApplicationAutoMapperProfile : Profile
{
    public ChatApplicationAutoMapperProfile()
    {

        CreateMap(typeof(TreeEntity<,>),typeof(TreeInfo<>));

        //SessionUnit
        CreateMap<SessionUnit, SessionUnitCacheItem>();

        //ChatObject
        CreateMap<ChatObject, ChatObjectInfo>()
            .ForMember(x => x.ServiceStatus, opts => opts.MapFrom<ChatObjectInfoServiceStutusResolver>()); ;

        //Message
        CreateMap<Message, MessageInfo>().MaxDepth(3);
        CreateMap<Message, MessageAnyInfo>()
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

        CreateMap<TextContentInfo, TextContent>().UsingMessageTemplate().ReverseMap();
        CreateMap<CmdContentInfo, CmdContent>().UsingMessageTemplate().ReverseMap();
        CreateMap<HtmlContentInfo, HtmlContent>().UsingMessageTemplate().ReverseMap();
        CreateMap<ContactsContentInfo, ContactsContent>().UsingMessageTemplate().ReverseMap();
        CreateMap<FileContentInfo, FileContent>().UsingMessageTemplate().ReverseMap();
        CreateMap<ImageContentInfo, ImageContent>().UsingMessageTemplate().ReverseMap();
        CreateMap<LinkContentInfo, LinkContent>().UsingMessageTemplate().ReverseMap();
        CreateMap<LocationContentInfo, LocationContent>().UsingMessageTemplate().ReverseMap();
        CreateMap<SoundContentInfo, SoundContent>().UsingMessageTemplate().ReverseMap();
        CreateMap<VideoContentInfo, VideoContent>().UsingMessageTemplate().ReverseMap();
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

        CreateMap<SessionTag, SessionTagInfo>();


        //ConnectionPool
        CreateMap<ConnectionPoolCacheItem, DisconnectedEto>().ReverseMap();
        CreateMap<ConnectionPoolCacheItem, ConnectedEto>().ReverseMap();
        CreateMap<ConnectionPoolCacheItem, ActivedEto>().ReverseMap();

        CreateMap<DisconnectedEto, ConnectedEto>().ReverseMap();
        CreateMap<DisconnectedEto, ActivedEto>().ReverseMap();
        CreateMap<ConnectedEto, ActivedEto>().ReverseMap();

    }
}
