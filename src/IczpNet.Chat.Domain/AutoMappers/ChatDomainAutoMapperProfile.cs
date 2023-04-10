using AutoMapper;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.ContentOutputs;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.SessionSections.SessionOrganizations;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionSections.SessionUnits;

namespace IczpNet.Chat.AutoMappers;

public class ChatApplicationAutoMapperProfile : Profile
{
    public ChatApplicationAutoMapperProfile()
    {
        //SessionUnit
        CreateMap<SessionUnit, SessionUnitCacheItem>();

        //ChatObject
        CreateMap<ChatObject, ChatObjectInfo>();

        //Message
        CreateMap<Message, MessageInfo>().MaxDepth(1);
        CreateMap<Message, MessageWithQuoteInfo>().MaxDepth(1);
        CreateMap(typeof(Message), typeof(MessageInfo<>)).MaxDepth(1);
        CreateMap(typeof(Message), typeof(MessageWithQuoteInfo<>)).MaxDepth(1);

        //CreateMap<MessageInput<>, Message>();
        CreateMap<Message, TextMessageOuput>().ForMember(x => x.Content, o => o.MapFrom(x => x.GetContent()));

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
        CreateMap<RedEnvelopeContent, RedEnvelopeContentOutput>();
        //RedEnvelope
        CreateMap<RedEnvelopeContent, RedEnvelopeContentOutput>()
          //.ForMember(d => d.Detail, options => options.MapFrom<DetailResolver>())
          //.ForMember(d => d.IsFinished, options => options.MapFrom<IsFinishedResolver>())
          ;


        CreateMap<SessionUnit, SessionUnitSenderInfo>();

        CreateMap<SessionTag, SessionTagInfo>();

        CreateMap<SessionOrganization, SessionOrganizationInfo>();

    }
}
