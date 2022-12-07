using AutoMapper;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using Volo.Abp.AutoMapper;

namespace IczpNet.Chat.AutoMappers;

public class ChatApplicationAutoMapperProfile : Profile
{
    public ChatApplicationAutoMapperProfile()
    {
        //ChatObject
        CreateMap<ChatObject, ChatObjectInfo>();


        //Message
        CreateMap<Message, MessageInfo>();

        //CreateMap<MessageInput<>, Message>();


        //MessageContent

        //CreateMap<RedEnvelopeContent, RedEnvelopeContentResult>()
        //  //.ForMember(d => d.Detail, options => options.MapFrom<DetailResolver>())
        //  //.ForMember(d => d.IsFinished, options => options.MapFrom<IsFinishedResolver>())
        //  ;

        //CreateMap<TextContentInfo, TextContent>().IgnoreAllPropertiesWithAnInaccessibleSetter().ReverseMap();
        //CreateMap<CmdContentInfo, CmdContent>().IgnoreAllPropertiesWithAnInaccessibleSetter().ReverseMap();
        //CreateMap<HtmlContentInfo, HtmlContent>().IgnoreAllPropertiesWithAnInaccessibleSetter().ReverseMap();

        //CreateMap<ContactsContentInfo, ContactsContent>().IgnoreAllPropertiesWithAnInaccessibleSetter().ReverseMap();

        //CreateMap<FileContentInfo, FileContent>().IgnoreAllPropertiesWithAnInaccessibleSetter().ReverseMap();
        //CreateMap<ImageContentInfo, ImageContent>().IgnoreAllPropertiesWithAnInaccessibleSetter().ReverseMap();
        //CreateMap<LinkContentInfo, LinkContent>().IgnoreAllPropertiesWithAnInaccessibleSetter().ReverseMap();
        //CreateMap<LocationContentInfo, LocationContent>().IgnoreAllPropertiesWithAnInaccessibleSetter().ReverseMap();
        //CreateMap<SoundContentInfo, SoundContent>().IgnoreAllPropertiesWithAnInaccessibleSetter().ReverseMap();
        //CreateMap<VideoContentInfo, VideoContent>().IgnoreAllPropertiesWithAnInaccessibleSetter().ReverseMap();

        //CreateMap<HistoryContentInput, HistoryContent>().IgnoreAllPropertiesWithAnInaccessibleSetter();
        //CreateMap<HistoryContent, HistoryContentOutput>().IgnoreAllPropertiesWithAnInaccessibleSetter();

    }
}
