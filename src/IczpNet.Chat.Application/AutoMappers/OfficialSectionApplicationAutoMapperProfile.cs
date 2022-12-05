using AutoMapper;
using IczpNet.Chat.OfficialSections.OfficialGroupMembers;
using IczpNet.Chat.OfficialSections.OfficialGroupMembers.Dtos;
using IczpNet.Chat.OfficialSections.OfficialGroups;
using IczpNet.Chat.OfficialSections.OfficialGroups.Dtos;
using IczpNet.Chat.OfficialSections.OfficialMembers;
using IczpNet.Chat.OfficialSections.OfficialMembers.Dtos;
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
        CreateMap<Official, OfficialDto>()
            .ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()))
            .ForMember(x => x.GroupCount, o => o.MapFrom(x => x.GetGroupCount()))
            ;
        CreateMap<Official, OfficialDetailDto>()
            .ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()))
            .ForMember(x => x.GroupCount, o => o.MapFrom(x => x.GetGroupCount()))
            ;
        CreateMap<OfficialCreateInput, Official>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<OfficialUpdateInput, Official>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();

        //OfficialGroup
        CreateMap<OfficialGroup, OfficialGroupDto>()
            .ForMember(x => x.GroupMemberCount, o => o.MapFrom(x => x.GetGroupMemberCount()))

            ;
        CreateMap<OfficialGroup, OfficialGroupDetailDto>()
            .ForMember(x => x.GroupMemberCount, o => o.MapFrom(x => x.GetGroupMemberCount()))
            ;
        CreateMap<OfficialGroupCreateInput, OfficialGroup>(MemberList.None).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<OfficialGroupUpdateInput, OfficialGroup>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();

        //OfficialMember
        CreateMap<OfficialMember, OfficialMemberDto>();
        CreateMap<OfficialMember, OfficialMemberDetailDto>();
        CreateMap<OfficialMemberCreateInput, OfficialMember>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<OfficialMemberUpdateInput, OfficialMember>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();

        //OfficialGroupMember
        CreateMap<OfficialGroupMember, OfficialGroupMemberDto>();
        CreateMap<OfficialGroupMember, OfficialGroupMemberDetailDto>();
        CreateMap<OfficialGroupMemberCreateInput, OfficialGroupMember>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<OfficialGroupMemberUpdateInput, OfficialGroupMember>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
    }
}
