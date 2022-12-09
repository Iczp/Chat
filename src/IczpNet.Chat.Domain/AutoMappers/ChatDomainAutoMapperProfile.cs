using AutoMapper;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Outputs;
using IczpNet.Chat.MessageSections.Templates;
using Volo.Abp.AutoMapper;
using System.Linq.Dynamic.Core;
using System.Linq;

namespace IczpNet.Chat.AutoMappers;

public class ChatApplicationAutoMapperProfile : Profile
{
    public ChatApplicationAutoMapperProfile()
    {
        //ChatObject
        CreateMap<ChatObject, ChatObjectInfo>();


        //Message
        CreateMap<Message, MessageInfo>();

        CreateMap(typeof(Message), typeof(MessageInfo<>));

        //CreateMap<MessageInput<>, Message>();
        CreateMap<Message, TextMessageOuput>().ForMember(x => x.Content, o => o.MapFrom(x => x.GetContent()));

        //MessageContent

        //CreateMap<RedEnvelopeContent, RedEnvelopeContentResult>()
        //  //.ForMember(d => d.Detail, options => options.MapFrom<DetailResolver>())
        //  //.ForMember(d => d.IsFinished, options => options.MapFrom<IsFinishedResolver>())
        //  ;

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
    }
}
