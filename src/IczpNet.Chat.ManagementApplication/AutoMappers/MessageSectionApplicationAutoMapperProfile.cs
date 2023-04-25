using AutoMapper;
using IczpNet.Chat.Management.MessageSections.Messages.Dtos;
using IczpNet.Chat.MessageSections.Messages;

namespace IczpNet.Chat.Management.AutoMappers;

public class MessageSectionApplicationAutoMapperProfile : Profile
{
    public MessageSectionApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        //Message
        CreateMap<Message, MessageDto>()
            //.ForMember(x => x.Content, o => o.MapFrom(x => x.GetContentDto()))
            .MaxDepth(1)
            //.ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()))
            ;

        CreateMap<Message, MessageDetailDto>()
            //.ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()))
            ;
        CreateMap<MessageCreateInput, Message>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<MessageUpdateInput, Message>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
    }
}
