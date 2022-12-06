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
        CreateMap<ChatObject, ChatObjectSimpleInfo>();


        //MessageContent
        CreateMap<Message, MessageInfo>();


        //MessageContent
        CreateMap<TextContent, TextContentInfo>();





    }
}
