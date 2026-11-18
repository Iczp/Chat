// Generated from conventional CreateMap declarations. Keep complex maps in the profile until migrated.
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.Contacts.Dtos;
using IczpNet.Chat.DeletedRecorders;
using IczpNet.Chat.DeletedRecorders.Dtos;
using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.FavoritedRecorders.Dtos;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.OpenedRecorders.Dtos;
using IczpNet.Chat.SessionOrganizations;
using IczpNet.Chat.SessionPermissionDefinitions;
using IczpNet.Chat.SessionPermissionGroups;
using IczpNet.Chat.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionPermissions;
using IczpNet.Chat.SessionPermissionUnitGrants;
using IczpNet.Chat.SessionRequests;
using IczpNet.Chat.SessionRoles;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissionGroups.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissions.Dtos;
using IczpNet.Chat.SessionSections.SessionRequests.Dtos;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.Sessions.Dtos;
using IczpNet.Chat.SessionSections.SessionTags.Dtos;
using IczpNet.Chat.SessionTags;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.SessionUnitSettings.Dtos;
using System.Linq;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

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
public partial class SessionSectionApplicationSessionUnitToContactsDtoMapper : MapperBase<SessionUnit, ContactsDto>
{
    public override partial ContactsDto Map(SessionUnit source);

    public override partial void Map(SessionUnit source, ContactsDto destination);
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
public partial class SessionSectionApplicationSessionUnitSettingToSessionUnitSettingDtoMapper : MapperBase<SessionUnitSetting, SessionUnitSettingDto>
{
    public override partial SessionUnitSettingDto Map(SessionUnitSetting source);

    public override partial void Map(SessionUnitSetting source, SessionUnitSettingDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionUnitSettingToSessionUnitSettingSimpleDtoMapper : MapperBase<SessionUnitSetting, SessionUnitSettingSimpleDto>
{
    public override partial SessionUnitSettingSimpleDto Map(SessionUnitSetting source);

    public override partial void Map(SessionUnitSetting source, SessionUnitSettingSimpleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionUnitSettingCacheItemToSessionUnitSettingSimpleDtoMapper : MapperBase<SessionUnitSettingCacheItem, SessionUnitSettingSimpleDto>
{
    public override partial SessionUnitSettingSimpleDto Map(SessionUnitSettingCacheItem source);

    public override partial void Map(SessionUnitSettingCacheItem source, SessionUnitSettingSimpleDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionUnitSettingCacheItemToSessionUnitMemberSettingDtoMapper : MapperBase<SessionUnitSettingCacheItem, SessionUnitMemberSettingDto>
{
    // The settings cache has no join timestamp or description.
    [MapperIgnoreTarget(nameof(SessionUnitMemberSettingDto.JoinTime))]
    [MapperIgnoreTarget(nameof(SessionUnitMemberSettingDto.JoinWayDescription))]
    public override partial SessionUnitMemberSettingDto Map(SessionUnitSettingCacheItem source);

    [MapperIgnoreTarget(nameof(SessionUnitMemberSettingDto.JoinTime))]
    [MapperIgnoreTarget(nameof(SessionUnitMemberSettingDto.JoinWayDescription))]
    public override partial void Map(SessionUnitSettingCacheItem source, SessionUnitMemberSettingDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionOrganizationToSessionOrganizationDtoMapper : MapperBase<SessionOrganization, SessionOrganizationDto>
{
    public override partial SessionOrganizationDto Map(SessionOrganization source);

    public override partial void Map(SessionOrganization source, SessionOrganizationDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationSessionOrganizationToSessionOrganizationDetailDtoMapper : MapperBase<SessionOrganization, SessionOrganizationDetailDto>
{
    public override partial SessionOrganizationDetailDto Map(SessionOrganization source);

    public override partial void Map(SessionOrganization source, SessionOrganizationDetailDto destination);
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
public partial class SessionSectionApplicationOpenedRecorderToOpenedRecorderDtoMapper : MapperBase<OpenedRecorder, OpenedRecorderDto>
{
    public override partial OpenedRecorderDto Map(OpenedRecorder source);

    public override partial void Map(OpenedRecorder source, OpenedRecorderDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationFavoritedRecorderToFavoritedRecorderDtoMapper : MapperBase<FavoritedRecorder, FavoritedRecorderDto>
{
    public override partial FavoritedRecorderDto Map(FavoritedRecorder source);

    public override partial void Map(FavoritedRecorder source, FavoritedRecorderDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SessionSectionApplicationDeletedRecorderToDeletedRecorderDtoMapper : MapperBase<DeletedRecorder, DeletedRecorderDto>
{
    public override partial DeletedRecorderDto Map(DeletedRecorder source);

    public override partial void Map(DeletedRecorder source, DeletedRecorderDto destination);
}
