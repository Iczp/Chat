using AutoMapper;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;

namespace IczpNet.Chat.AutoMappers;

public class ChatApplicationAutoMapperProfile : Profile
{
    public ChatApplicationAutoMapperProfile()
    {
        //ChatObject
        CreateMap<ChatObject, ChatObjectInfo>();


        //Message
        CreateMap<Message, MessageInfo>();


        //MessageContent

        CreateMap<RedEnvelopeContent, RedEnvelopeContentResult>()
          //.ForMember(d => d.Detail, options => options.MapFrom<DetailResolver>())
          //.ForMember(d => d.IsFinished, options => options.MapFrom<IsFinishedResolver>())
          ;

        CreateMap<TextContentInfo, TextContent>().ReverseMap();
        CreateMap<CmdContentInfo, CmdContent>().ReverseMap();
        CreateMap<HtmlContentInfo, HtmlContent>().ReverseMap();

        CreateMap<ContactsContentInfo, ContactsContent>().ReverseMap();

        CreateMap<FileContentInfo, FileContent>().ReverseMap();
        CreateMap<ImageContentInfo, ImageContent>().ReverseMap();
        CreateMap<LinkContentInfo, LinkContent>().ReverseMap();
        CreateMap<LocationContentInfo, LocationContent>().ReverseMap();
        CreateMap<SoundContentInfo, SoundContent>().ReverseMap();
        CreateMap<VideoContentInfo, VideoContent>().ReverseMap();

        CreateMap<HistoryContentInput, HistoryContent>();
        CreateMap<HistoryContent, HistoryContentOutput>();

    }
}
