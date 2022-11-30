using AutoMapper;
using IczpNet.Chat.Messages;
using IczpNet.Chat.Messages.Dtos;

namespace IczpNet.Chat.AutoMappers;

public class MessageSectionApplicationAutoMapperProfile : Profile
{
    public MessageSectionApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        //Message
        CreateMap<Message, MessageDto>()
            //.ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()))
            ;
        CreateMap<Message, MessageDetailDto>()
            //.ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()))
            ;
        CreateMap<MessageCreateInput, Message>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<MessageUpdateInput, Message>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
    }
}
