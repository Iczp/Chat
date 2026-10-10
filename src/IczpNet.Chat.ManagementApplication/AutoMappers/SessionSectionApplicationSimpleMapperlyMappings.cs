using IczpNet.Chat.SessionPermissionUnitGrants;
// Generated from conventional CreateMap declarations. Keep complex maps in the profile until migrated.
using IczpNet.Chat.SessionRoles;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionTags;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionOrganizations;
using IczpNet.Chat.SessionPermissionDefinitions;
using IczpNet.Chat.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionPermissions;
using IczpNet.Chat.SessionRequests;
using IczpNet.Chat.SessionPermissionGroups;
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
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.Management.AutoMappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionToSessionDtoMapper : MapperBase<Session, SessionDto>
{
    public override partial SessionDto Map(Session source);

    public override partial void Map(Session source, SessionDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionToSessionDetailDtoMapper : MapperBase<Session, SessionDetailDto>
{
    public override partial SessionDetailDto Map(Session source);

    public override partial void Map(Session source, SessionDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionUnitToSessionUnitDtoMapper : MapperBase<SessionUnit, SessionUnitDto>
{
    public override partial SessionUnitDto Map(SessionUnit source);

    public override partial void Map(SessionUnit source, SessionUnitDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionUnitToSessionUnitOwnerDtoMapper : MapperBase<SessionUnit, SessionUnitOwnerDto>
{
    public override partial SessionUnitOwnerDto Map(SessionUnit source);

    public override partial void Map(SessionUnit source, SessionUnitOwnerDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionUnitToSessionUnitDestinationDtoMapper : MapperBase<SessionUnit, SessionUnitDestinationDto>
{
    public override partial SessionUnitDestinationDto Map(SessionUnit source);

    public override partial void Map(SessionUnit source, SessionUnitDestinationDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionUnitToSessionUnitWithDestinationDtoMapper : MapperBase<SessionUnit, SessionUnitWithDestinationDto>
{
    public override partial SessionUnitWithDestinationDto Map(SessionUnit source);

    public override partial void Map(SessionUnit source, SessionUnitWithDestinationDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionUnitModelToSessionUnitOwnerDtoMapper : MapperBase<SessionUnitModel, SessionUnitOwnerDto>
{
    public override partial SessionUnitOwnerDto Map(SessionUnitModel source);

    public override partial void Map(SessionUnitModel source, SessionUnitOwnerDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionOrganizationToSessionOrganizationManagementDtoMapper : MapperBase<SessionOrganization, SessionOrganizationManagementDto>
{
    public override partial SessionOrganizationManagementDto Map(SessionOrganization source);

    public override partial void Map(SessionOrganization source, SessionOrganizationManagementDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionOrganizationToSessionOrganizationDetailManagementDtoMapper : MapperBase<SessionOrganization, SessionOrganizationDetailManagementDto>
{
    public override partial SessionOrganizationDetailManagementDto Map(SessionOrganization source);

    public override partial void Map(SessionOrganization source, SessionOrganizationDetailManagementDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionOrganizationToSessionOrganizationInfoMapper : MapperBase<SessionOrganization, SessionOrganizationInfo>
{
    public override partial SessionOrganizationInfo Map(SessionOrganization source);

    public override partial void Map(SessionOrganization source, SessionOrganizationInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionTagToSessionTagDtoMapper : MapperBase<SessionTag, SessionTagDto>
{
    public override partial SessionTagDto Map(SessionTag source);

    public override partial void Map(SessionTag source, SessionTagDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionTagToSessionTagDetailDtoMapper : MapperBase<SessionTag, SessionTagDetailDto>
{
    public override partial SessionTagDetailDto Map(SessionTag source);

    public override partial void Map(SessionTag source, SessionTagDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionTagToSessionTagSimpleDtoMapper : MapperBase<SessionTag, SessionTagSimpleDto>
{
    public override partial SessionTagSimpleDto Map(SessionTag source);

    public override partial void Map(SessionTag source, SessionTagSimpleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionRoleToSessionRoleDtoMapper : MapperBase<SessionRole, SessionRoleDto>
{
    public override partial SessionRoleDto Map(SessionRole source);

    public override partial void Map(SessionRole source, SessionRoleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionRoleToSessionRolePermissionDtoMapper : MapperBase<SessionRole, SessionRolePermissionDto>
{
    public override partial SessionRolePermissionDto Map(SessionRole source);

    public override partial void Map(SessionRole source, SessionRolePermissionDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionRoleToSessionRoleDetailDtoMapper : MapperBase<SessionRole, SessionRoleDetailDto>
{
    public override partial SessionRoleDetailDto Map(SessionRole source);

    public override partial void Map(SessionRole source, SessionRoleDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionRoleToSessionRoleSimpleDtoMapper : MapperBase<SessionRole, SessionRoleSimpleDto>
{
    public override partial SessionRoleSimpleDto Map(SessionRole source);

    public override partial void Map(SessionRole source, SessionRoleSimpleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionPermissionRoleGrantToPermissionGrantValueMapper : MapperBase<SessionPermissionRoleGrant, PermissionGrantValue>
{
    public override partial PermissionGrantValue Map(SessionPermissionRoleGrant source);

    public override partial void Map(SessionPermissionRoleGrant source, PermissionGrantValue destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionPermissionUnitGrantToPermissionGrantValueMapper : MapperBase<SessionPermissionUnitGrant, PermissionGrantValue>
{
    public override partial PermissionGrantValue Map(SessionPermissionUnitGrant source);

    public override partial void Map(SessionPermissionUnitGrant source, PermissionGrantValue destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionPermissionRoleGrantToSessionPermissionRoleGrantDtoMapper : MapperBase<SessionPermissionRoleGrant, SessionPermissionRoleGrantDto>
{
    public override partial SessionPermissionRoleGrantDto Map(SessionPermissionRoleGrant source);

    public override partial void Map(SessionPermissionRoleGrant source, SessionPermissionRoleGrantDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionPermissionUnitGrantToSessionPermissionUnitGrantDtoMapper : MapperBase<SessionPermissionUnitGrant, SessionPermissionUnitGrantDto>
{
    public override partial SessionPermissionUnitGrantDto Map(SessionPermissionUnitGrant source);

    public override partial void Map(SessionPermissionUnitGrant source, SessionPermissionUnitGrantDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionPermissionDefinitionToSessionPermissionDefinitionDtoMapper : MapperBase<SessionPermissionDefinition, SessionPermissionDefinitionDto>
{
    public override partial SessionPermissionDefinitionDto Map(SessionPermissionDefinition source);

    public override partial void Map(SessionPermissionDefinition source, SessionPermissionDefinitionDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionPermissionDefinitionToSessionPermissionDefinitionDetailDtoMapper : MapperBase<SessionPermissionDefinition, SessionPermissionDefinitionDetailDto>
{
    public override partial SessionPermissionDefinitionDetailDto Map(SessionPermissionDefinition source);

    public override partial void Map(SessionPermissionDefinition source, SessionPermissionDefinitionDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionPermissionDefinitionToSessionPermissionDefinitionSimpleDtoMapper : MapperBase<SessionPermissionDefinition, SessionPermissionDefinitionSimpleDto>
{
    public override partial SessionPermissionDefinitionSimpleDto Map(SessionPermissionDefinition source);

    public override partial void Map(SessionPermissionDefinition source, SessionPermissionDefinitionSimpleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionPermissionGroupToSessionPermissionGroupDtoMapper : MapperBase<SessionPermissionGroup, SessionPermissionGroupDto>
{
    public override partial SessionPermissionGroupDto Map(SessionPermissionGroup source);

    public override partial void Map(SessionPermissionGroup source, SessionPermissionGroupDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionPermissionGroupToSessionPermissionGroupDetailDtoMapper : MapperBase<SessionPermissionGroup, SessionPermissionGroupDetailDto>
{
    public override partial SessionPermissionGroupDetailDto Map(SessionPermissionGroup source);

    public override partial void Map(SessionPermissionGroup source, SessionPermissionGroupDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionPermissionGroupToSessionPermissionGroupInfoMapper : MapperBase<SessionPermissionGroup, SessionPermissionGroupInfo>
{
    public override partial SessionPermissionGroupInfo Map(SessionPermissionGroup source);

    public override partial void Map(SessionPermissionGroup source, SessionPermissionGroupInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionRequestToSessionRequestDtoMapper : MapperBase<SessionRequest, SessionRequestDto>
{
    public override partial SessionRequestDto Map(SessionRequest source);

    public override partial void Map(SessionRequest source, SessionRequestDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionRequestToSessionRequestDetailDtoMapper : MapperBase<SessionRequest, SessionRequestDetailDto>
{
    public override partial SessionRequestDetailDto Map(SessionRequest source);

    public override partial void Map(SessionRequest source, SessionRequestDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationOpenedRecorderToOpenedRecorderManagementDtoMapper : MapperBase<OpenedRecorder, OpenedRecorderManagementDto>
{
    public override partial OpenedRecorderManagementDto Map(OpenedRecorder source);

    public override partial void Map(OpenedRecorder source, OpenedRecorderManagementDto destination);
}
