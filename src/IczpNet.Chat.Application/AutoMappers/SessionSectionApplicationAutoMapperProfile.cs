using AutoMapper;
using IczpNet.Chat.Contacts.Dtos;
using IczpNet.Chat.DeletedRecorders;
using IczpNet.Chat.DeletedRecorders.Dtos;
using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.FavoritedRecorders.Dtos;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.OpenedRecorders.Dtos;
using IczpNet.Chat.SessionSections.SessionOrganizations;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissionGroups;
using IczpNet.Chat.SessionSections.SessionPermissionGroups.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionSections.SessionPermissions.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissionUnitGrants;
using IczpNet.Chat.SessionSections.SessionRequests;
using IczpNet.Chat.SessionSections.SessionRequests.Dtos;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.Sessions.Dtos;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionSections.SessionTags.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.SessionUnitSettings.Dtos;
using System.Linq;
using Volo.Abp.AutoMapper;

namespace IczpNet.Chat.AutoMappers;

public class SessionSectionApplicationAutoMapperProfile : Profile
{
    public SessionSectionApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */


        //Session
        CreateMap<Session, SessionDto>();
        CreateMap<Session, SessionDetailDto>();

        //SessionUnit
        CreateMap<SessionUnit, SessionUnitDto>();
        CreateMap<SessionUnit, ContactsDto>();
        CreateMap<SessionUnit, SessionUnitOwnerDto>();//.AfterMap<SessionUnitOwnerDtoMappingAction>();
        CreateMap<SessionUnit, SessionUnitOwnerDetailDto>().Ignore(x => x.SessionUnitCount);
        CreateMap<SessionUnit, SessionUnitDetailDto>().Ignore(x => x.SessionUnitCount);
        CreateMap<SessionUnit, SessionUnitDestinationDetailDto>();
        CreateMap<SessionUnit, SessionUnitDestinationDto>();
        CreateMap<SessionUnit, SessionUnitWithDestinationDto>();
        
        CreateMap<SessionUnit, SessionUnitSenderDto>()
            .Ignore(x => x.IsFriendship)
            .Ignore(x => x.FriendshipName)
            .Ignore(x => x.FriendshipSessionUnitId)
            ;

        CreateMap<SessionUnit, SessionUnitCacheItem>();

        CreateMap<SessionUnitCacheItem, SessionUnitCacheDto>(MemberList.None)
            //.Ignore(x => x.Settings)
            //.Ignore(x => x.Destination)
            ;
        CreateMap<SessionUnitCacheDto, SessionUnitCacheDto>()
            .ForMember(x => x.Settings, opt => opt.MapFrom(y => y.Settings))
            .ForMember(x => x.Destination, opt => opt.MapFrom(y => y.Destination))
            .PreserveReferences()
            .MaxDepth(3);


        //CreateMap<SessionUnitModel, SessionUnitOwnerDto>();

        CreateMap<SessionUnitSetting, SessionUnitSettingDto>();
        CreateMap<SessionUnitSetting, SessionUnitSettingSimpleDto>();

        //SessionOrganization
        CreateMap<SessionOrganization, SessionOrganizationDto>();
        CreateMap<SessionOrganization, SessionOrganizationDetailDto>();
        CreateMap<SessionOrganizationCreateInput, SessionOrganization>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<SessionOrganizationCreateBySessionUnitInput, SessionOrganization>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<SessionOrganizationUpdateInput, SessionOrganization>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<SessionOrganization, SessionOrganizationInfo>();

        //SessionTag
        CreateMap<SessionTag, SessionTagDto>();
        CreateMap<SessionTag, SessionTagDetailDto>();
        CreateMap<SessionTag, SessionTagSimpleDto>();
        CreateMap<SessionTagCreateInput, SessionTag>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<SessionTagUpdateInput, SessionTag>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        //SessionRole
        CreateMap<SessionRole, SessionRoleDto>();
        CreateMap<SessionRole, SessionRolePermissionDto>();
        CreateMap<SessionRole, SessionRoleDetailDto>();
        CreateMap<SessionRole, SessionRoleSimpleDto>();
        CreateMap<SessionRoleCreateInput, SessionRole>(MemberList.None)
            .IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
            .IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<SessionRoleCreateBySessionUnitInput, SessionRole>(MemberList.None)
            .IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
            .IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<SessionRoleUpdateInput, SessionRole>(MemberList.None)
            .IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
            .IgnoreAllPropertiesWithAnInaccessibleSetter();

        //PermissionGrant
        CreateMap<SessionPermissionRoleGrant, PermissionGrantValue>();
        CreateMap<SessionPermissionUnitGrant, PermissionGrantValue>();
        CreateMap<SessionPermissionRoleGrant, SessionPermissionRoleGrantDto>();
        CreateMap<SessionPermissionUnitGrant, SessionPermissionUnitGrantDto>();

        //SessionPermissionDefinition
        CreateMap<SessionPermissionDefinition, SessionPermissionDefinitionDto>();
        CreateMap<SessionPermissionDefinition, SessionPermissionDefinitionDetailDto>();
        CreateMap<SessionPermissionDefinition, SessionPermissionDefinitionSimpleDto>();
        CreateMap<SessionPermissionDefinitionCreateInput, SessionPermissionDefinition>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<SessionPermissionDefinitionUpdateInput, SessionPermissionDefinition>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        //SessionPermissionGroup
        CreateMap<SessionPermissionGroup, SessionPermissionGroupDto>();
        CreateMap<SessionPermissionGroup, SessionPermissionGroupDetailDto>();
        CreateMap<SessionPermissionGroupCreateInput, SessionPermissionGroup>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<SessionPermissionGroupUpdateInput, SessionPermissionGroup>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<SessionPermissionGroup, SessionPermissionGroupInfo>();

        //SessionRequest
        CreateMap<SessionRequest, SessionRequestDto>();
        CreateMap<SessionRequest, SessionRequestDetailDto>();
        CreateMap<SessionRequestCreateInput, SessionRequest>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<SessionRequestUpdateInput, SessionRequest>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        //OpenedRecorder
        CreateMap<OpenedRecorder, OpenedRecorderDto>();

        //FavoritedRecorder
        CreateMap<FavoritedRecorder, FavoritedRecorderDto>();

        //DeletedRecorder
        CreateMap<DeletedRecorder, DeletedRecorderDto>();
    }
}
