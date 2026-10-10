using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.Management.SessionSections.SessionOrganiztions.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionPermissionDefinitions.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionRequests.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionTags.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionUnits.Dtos;
using IczpNet.Chat.SessionOrganizations;
using IczpNet.Chat.SessionPermissionDefinitions;
using IczpNet.Chat.SessionPermissionGroups;
using IczpNet.Chat.SessionRequests;
using IczpNet.Chat.SessionRoles;
using IczpNet.Chat.SessionTags;
using IczpNet.Chat.SessionUnits;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.Management.AutoMappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionOrganizationCreateManagementInputToSessionOrganizationMapper
    : MapperBase<SessionOrganizationCreateManagementInput, SessionOrganization>
{
    public override SessionOrganization Map(SessionOrganizationCreateManagementInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<SessionOrganization>();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionOrganizationCreateManagementInput source, SessionOrganization destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(SessionOrganizationCreateManagementInput source, SessionOrganization destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionOrganizationCreateBySessionUnitManagementInputToSessionOrganizationMapper
    : MapperBase<SessionOrganizationCreateBySessionUnitManagementInput, SessionOrganization>
{
    public override SessionOrganization Map(SessionOrganizationCreateBySessionUnitManagementInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<SessionOrganization>();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionOrganizationCreateBySessionUnitManagementInput source, SessionOrganization destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(SessionOrganizationCreateBySessionUnitManagementInput source, SessionOrganization destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionOrganizationUpdateManagementInputToSessionOrganizationMapper
    : MapperBase<SessionOrganizationUpdateManagementInput, SessionOrganization>
{
    public override SessionOrganization Map(SessionOrganizationUpdateManagementInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<SessionOrganization>();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionOrganizationUpdateManagementInput source, SessionOrganization destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(SessionOrganizationUpdateManagementInput source, SessionOrganization destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionTagCreateInputToSessionTagMapper
    : MapperBase<SessionTagCreateInput, SessionTag>
{
    public override SessionTag Map(SessionTagCreateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<SessionTag>();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionTagCreateInput source, SessionTag destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(SessionTagCreateInput source, SessionTag destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionTagUpdateInputToSessionTagMapper
    : MapperBase<SessionTagUpdateInput, SessionTag>
{
    public override SessionTag Map(SessionTagUpdateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<SessionTag>();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionTagUpdateInput source, SessionTag destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(SessionTagUpdateInput source, SessionTag destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionRoleCreateInputToSessionRoleMapper
    : MapperBase<SessionRoleCreateInput, SessionRole>
{
    public override SessionRole Map(SessionRoleCreateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<SessionRole>();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionRoleCreateInput source, SessionRole destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
        if (source.SessionId.HasValue)
        {
            destination.SetSessionId(source.SessionId.Value);
        }
        if (source.PermissionGrant != null)
        {
            destination.SetPermissionGrant(source.PermissionGrant);
        }
    }

    [MapperIgnoreTarget(nameof(SessionRole.PermissionGrant))]
    [MapperIgnoreTarget(nameof(SessionRole.GrantList))]
    private partial void MapPublicProperties(SessionRoleCreateInput source, SessionRole destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionRoleCreateBySessionUnitInputToSessionRoleMapper
    : MapperBase<SessionRoleCreateBySessionUnitInput, SessionRole>
{
    public override SessionRole Map(SessionRoleCreateBySessionUnitInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<SessionRole>();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionRoleCreateBySessionUnitInput source, SessionRole destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
        if (source.PermissionGrant != null)
        {
            destination.SetPermissionGrant(source.PermissionGrant);
        }
    }

    [MapperIgnoreTarget(nameof(SessionRole.PermissionGrant))]
    [MapperIgnoreTarget(nameof(SessionRole.GrantList))]
    private partial void MapPublicProperties(SessionRoleCreateBySessionUnitInput source, SessionRole destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionRoleUpdateInputToSessionRoleMapper
    : MapperBase<SessionRoleUpdateInput, SessionRole>
{
    public override SessionRole Map(SessionRoleUpdateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<SessionRole>();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionRoleUpdateInput source, SessionRole destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
        if (source.PermissionGrant != null)
        {
            destination.SetPermissionGrant(source.PermissionGrant);
        }
    }

    [MapperIgnoreTarget(nameof(SessionRole.PermissionGrant))]
    [MapperIgnoreTarget(nameof(SessionRole.GrantList))]
    private partial void MapPublicProperties(SessionRoleUpdateInput source, SessionRole destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionPermissionDefinitionCreateInputToSessionPermissionDefinitionMapper
    : MapperBase<SessionPermissionDefinitionCreateInput, SessionPermissionDefinition>
{
    public override SessionPermissionDefinition Map(SessionPermissionDefinitionCreateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<SessionPermissionDefinition>();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionPermissionDefinitionCreateInput source, SessionPermissionDefinition destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(SessionPermissionDefinitionCreateInput source, SessionPermissionDefinition destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionPermissionDefinitionUpdateInputToSessionPermissionDefinitionMapper
    : MapperBase<SessionPermissionDefinitionUpdateInput, SessionPermissionDefinition>
{
    public override SessionPermissionDefinition Map(SessionPermissionDefinitionUpdateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<SessionPermissionDefinition>();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionPermissionDefinitionUpdateInput source, SessionPermissionDefinition destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(SessionPermissionDefinitionUpdateInput source, SessionPermissionDefinition destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionPermissionGroupCreateInputToSessionPermissionGroupMapper
    : MapperBase<SessionPermissionGroupCreateInput, SessionPermissionGroup>
{
    public override SessionPermissionGroup Map(SessionPermissionGroupCreateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<SessionPermissionGroup>();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionPermissionGroupCreateInput source, SessionPermissionGroup destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(SessionPermissionGroupCreateInput source, SessionPermissionGroup destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionPermissionGroupUpdateInputToSessionPermissionGroupMapper
    : MapperBase<SessionPermissionGroupUpdateInput, SessionPermissionGroup>
{
    public override SessionPermissionGroup Map(SessionPermissionGroupUpdateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<SessionPermissionGroup>();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionPermissionGroupUpdateInput source, SessionPermissionGroup destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(SessionPermissionGroupUpdateInput source, SessionPermissionGroup destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionRequestCreateInputToSessionRequestMapper
    : MapperBase<SessionRequestCreateInput, SessionRequest>
{
    public override SessionRequest Map(SessionRequestCreateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<SessionRequest>();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionRequestCreateInput source, SessionRequest destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(SessionRequestCreateInput source, SessionRequest destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionRequestUpdateInputToSessionRequestMapper
    : MapperBase<SessionRequestUpdateInput, SessionRequest>
{
    public override SessionRequest Map(SessionRequestUpdateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<SessionRequest>();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionRequestUpdateInput source, SessionRequest destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(SessionRequestUpdateInput source, SessionRequest destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionUnitToSessionUnitDestinationDetailDtoMapper
    : MapperBase<SessionUnit, SessionUnitDestinationDetailDto>
{
    public override partial SessionUnitDestinationDetailDto Map(SessionUnit source);
    public override partial void Map(SessionUnit source, SessionUnitDestinationDetailDto destination);
}
