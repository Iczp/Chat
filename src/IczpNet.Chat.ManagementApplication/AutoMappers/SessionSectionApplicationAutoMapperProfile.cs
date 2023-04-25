using AutoMapper;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionOrganizations;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionSections.SessionRequests;
using IczpNet.Chat.SessionSections.SessionPermissionGroups;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.Management.SessionSections.SessionUnits.Dtos;
using IczpNet.Chat.Management.SessionServices;
using IczpNet.Chat.Management.SessionSections.SessionOrganiztions.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionTags.Dtos;
using IczpNet.Chat.Management.SessionSections.Sessions.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionPermissions.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionPermissionDefinitions.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionRequests.Dtos;
using IczpNet.Chat.Management.SessionSections.OpenedRecordes.Dtos;

namespace IczpNet.Chat.Management.AutoMappers;

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
        CreateMap<SessionUnit, SessionUnitOwnerDto>();
        CreateMap<SessionUnit, SessionUnitDestinationDetailDto>();
        CreateMap<SessionUnit, SessionUnitDestinationDto>();
        CreateMap<SessionUnit, SessionUnitWithDestinationDto>();
        CreateMap<SessionUnitModel, SessionUnitOwnerDto>();

        //SessionOrganization
        CreateMap<SessionOrganization, SessionOrganizationManagementDto>();
        CreateMap<SessionOrganization, SessionOrganizationDetailManagementDto>();
        CreateMap<SessionOrganizationCreateManagementInput, SessionOrganization>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<SessionOrganizationCreateBySessionUnitManagementInput, SessionOrganization>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<SessionOrganizationUpdateManagementInput, SessionOrganization>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
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
        CreateMap<OpenedRecorder, OpenedRecorderManagementDto>();
    }
}
