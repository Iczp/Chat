using AutoMapper;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;

namespace IczpNet.Chat.AutoMappers;

public class MessageSectionApplicationAutoMapperProfile : Profile
{
    public MessageSectionApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */


        CreateMap<Message, MessageSimpleDto>();
        CreateMap<Message, MessageQuoteDto>().MapExtraProperties();
        //Message
        CreateMap<Message, MessageDto>()
            .MapExtraProperties()
            .ForMember(x => x.Content, o => o.MapFrom(x => x.GetContentDto()))
            .MaxDepth(5)
            //.ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()))
            ;

        CreateMap<Message, MessageOwnerDto>().MapExtraProperties()
            .ForMember(x => x.Content, o => o.MapFrom(x => x.GetContentDto()))
            .MaxDepth(5)
            //.ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()))
            //.AfterMap<MessageOwnerDtoMappingAction>()
            ;

        CreateMap<Message, MessageDetailDto>().MapExtraProperties()
            //.ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()))
            ;

        CreateMap<Message, MessageFavoriteDto>()
            .ForMember(x => x.Content, o => o.MapFrom(x => x.GetContentDto()))
            .MaxDepth(5)
           ;

        CreateMap<MessageCreateInput, Message>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<MessageUpdateInput, Message>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();

        // cache to dto
        CreateMap<MessageCacheItem, MessageOwnerDto>(MemberList.None).MapExtraProperties();
        CreateMap<MessageQuoteCacheItem, MessageQuoteDto>().MapExtraProperties();
        CreateMap<SessionUnitSenderInfo, SessionUnitSenderDto>(MemberList.None);//.MapExtraProperties();
        

    }
}
