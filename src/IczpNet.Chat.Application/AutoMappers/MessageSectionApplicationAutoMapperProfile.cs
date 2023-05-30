using AutoMapper;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;

namespace IczpNet.Chat.AutoMappers;

public class MessageSectionApplicationAutoMapperProfile : Profile
{
    public MessageSectionApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */


        CreateMap<Message, MessageSimpleDto>();

        //Message
        CreateMap<Message, MessageDto>()
            .ForMember(x => x.Content, o => o.MapFrom(x => x.GetContentDto()))
            .MaxDepth(1)
            //.ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()))
            ;

        CreateMap<Message, MessageOwnerDto>()
            .ForMember(x => x.Content, o => o.MapFrom(x => x.GetContentDto()))
            .MaxDepth(1)
            //.ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()))
            ;

        CreateMap<Message, MessageDetailDto>()
            //.ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()))
            ;

        CreateMap<Message, MessageFavoriteDto>()
            .ForMember(x => x.Content, o => o.MapFrom(x => x.GetContentDto()))
            .MaxDepth(1)
           ;

        CreateMap<MessageCreateInput, Message>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<MessageUpdateInput, Message>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
    }
}
