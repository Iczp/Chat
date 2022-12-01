using AutoMapper;
using IczpNet.Chat.OfficialSections.Officials;
using IczpNet.Chat.OfficialSections.Officials.Dtos;

namespace IczpNet.Chat.AutoMappers;

public class OfficialSectionApplicationAutoMapperProfile : Profile
{
    public OfficialSectionApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        //Official
        CreateMap<Official, OfficialDto>().ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()));
        CreateMap<Official, OfficialDetailDto>().ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()));
        CreateMap<OfficialCreateInput, Official>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<OfficialUpdateInput, Official>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
    }
}
