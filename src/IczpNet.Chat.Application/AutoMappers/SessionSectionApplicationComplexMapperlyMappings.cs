using IczpNet.Chat.Entries.Dtos;
using IczpNet.Chat.EntryNames;
using IczpNet.Chat.EntryNames.Dtos;
using IczpNet.Chat.EntryValues;
using IczpNet.Chat.EntryValues.Dtos;
using IczpNet.Chat.SessionOrganizations;
using IczpNet.Chat.SessionPermissionDefinitions;
using IczpNet.Chat.SessionPermissionGroups;
using IczpNet.Chat.SessionRequests;
using IczpNet.Chat.SessionRoles;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissionGroups.Dtos;
using IczpNet.Chat.SessionSections.SessionRequests.Dtos;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.SessionTags.Dtos;
using IczpNet.Chat.SessionTags;
using IczpNet.Chat.SessionUnitEntryValues;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using Riok.Mapperly.Abstractions;
using System;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionOrganizationCreateBySessionUnitInputToSessionOrganizationMapper
    : MapperBase<SessionOrganizationCreateBySessionUnitInput, SessionOrganization>
{
    public override SessionOrganization Map(SessionOrganizationCreateBySessionUnitInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<SessionOrganization>();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionOrganizationCreateBySessionUnitInput source, SessionOrganization destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(SessionOrganizationCreateBySessionUnitInput source, SessionOrganization destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionOrganizationCreateInputToSessionOrganizationMapper
    : MapperBase<SessionOrganizationCreateInput, SessionOrganization>
{
    public override SessionOrganization Map(SessionOrganizationCreateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<SessionOrganization>();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionOrganizationCreateInput source, SessionOrganization destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(SessionOrganizationCreateInput source, SessionOrganization destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionOrganizationUpdateInputToSessionOrganizationMapper
    : MapperBase<SessionOrganizationUpdateInput, SessionOrganization>
{
    public override SessionOrganization Map(SessionOrganizationUpdateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<SessionOrganization>();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionOrganizationUpdateInput source, SessionOrganization destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(SessionOrganizationUpdateInput source, SessionOrganization destination);
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
public partial class SessionSectionApplicationSessionTagToSessionTagCacheItemMapper
    : MapperBase<SessionTag, SessionTagCacheItem>
{
    public override partial SessionTagCacheItem Map(SessionTag source);
    public override partial void Map(SessionTag source, SessionTagCacheItem destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionUnitToSessionUnitDestinationDetailDtoMapper
    : MapperBase<SessionUnit, SessionUnitDestinationDetailDto>
{
    public override partial SessionUnitDestinationDetailDto Map(SessionUnit source);
    public override partial void Map(SessionUnit source, SessionUnitDestinationDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionUnitToSessionUnitDetailDtoMapper
    : MapperBase<SessionUnit, SessionUnitDetailDto>
{
    public override partial SessionUnitDetailDto Map(SessionUnit source);
    public override partial void Map(SessionUnit source, SessionUnitDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionUnitToSessionUnitOwnerDetailDtoMapper
    : MapperBase<SessionUnit, SessionUnitOwnerDetailDto>
{
    public override partial SessionUnitOwnerDetailDto Map(SessionUnit source);
    public override partial void Map(SessionUnit source, SessionUnitOwnerDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionUnitToSessionUnitSenderDtoMapper
    : MapperBase<SessionUnit, SessionUnitSenderDto>
{
    public override SessionUnitSenderDto Map(SessionUnit source)
    {
        if (source is null) return default!;
        var destination = new SessionUnitSenderDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionUnit source, SessionUnitSenderDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.Score = MemberScore.Create(source.IsCreator, source.CreationTime);
    }

    [MapperIgnoreTarget(nameof(SessionUnitSenderDto.Score))]
    private partial void MapFields(SessionUnit source, SessionUnitSenderDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionUnitCacheItemToSessionUnitFriendDetailDtoMapper
    : MapperBase<SessionUnitCacheItem, SessionUnitFriendDetailDto>
{
    public override SessionUnitFriendDetailDto Map(SessionUnitCacheItem source)
    {
        if (source is null) return default!;
        var destination = new SessionUnitFriendDetailDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionUnitCacheItem source, SessionUnitFriendDetailDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.Score = FriendScore.Create(source.Sorting, source.Ticks).Value;
    }

    [MapperIgnoreTarget(nameof(SessionUnitFriendDetailDto.Score))]
    private partial void MapFields(SessionUnitCacheItem source, SessionUnitFriendDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionUnitCacheItemToSessionUnitFriendDtoMapper
    : MapperBase<SessionUnitCacheItem, SessionUnitFriendDto>
{
    public override SessionUnitFriendDto Map(SessionUnitCacheItem source)
    {
        if (source is null) return default!;
        var destination = new SessionUnitFriendDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionUnitCacheItem source, SessionUnitFriendDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.Score = FriendScore.Create(source.Sorting, source.Ticks).Value;
    }

    [MapperIgnoreTarget(nameof(SessionUnitFriendDto.Score))]
    private partial void MapFields(SessionUnitCacheItem source, SessionUnitFriendDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionUnitCacheItemToSessionUnitMemberDtoMapper
    : MapperBase<SessionUnitCacheItem, SessionUnitMemberDto>
{
    public override SessionUnitMemberDto Map(SessionUnitCacheItem source)
    {
        if (source is null) return default!;
        var destination = new SessionUnitMemberDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionUnitCacheItem source, SessionUnitMemberDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.Score = MemberScore.Create(source.IsCreator, source.CreationTime);
    }

    [MapperIgnoreTarget(nameof(SessionUnitMemberDto.Score))]
    private partial void MapFields(SessionUnitCacheItem source, SessionUnitMemberDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionUnitEntryValueToEntryObjectDtoMapper
    : MapperBase<SessionUnitEntryValue, EntryObjectDto>
{
    public override EntryObjectDto Map(SessionUnitEntryValue source)
    {
        if (source is null) return default!;
        return new EntryObjectDto
        {
            EntryName = MapEntryName(source.EntryValue?.EntryName),
            EntryValue = MapEntryValue(source.EntryValue)
        };
    }

    public override void Map(SessionUnitEntryValue source, EntryObjectDto destination)
    {
        if (source is null || destination is null) return;
        destination.EntryName = MapEntryName(source.EntryValue?.EntryName);
        destination.EntryValue = MapEntryValue(source.EntryValue);
    }

    private partial EntryNameSimpleDto MapEntryName(EntryName source);
    private partial EntryValueSimpleDto MapEntryValue(EntryValue source);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionUnitFriendDtoToSessionUnitFriendDetailDtoMapper
    : MapperBase<SessionUnitFriendDto, SessionUnitFriendDetailDto>
{
    public override partial SessionUnitFriendDetailDto Map(SessionUnitFriendDto source);
    public override partial void Map(SessionUnitFriendDto source, SessionUnitFriendDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionUnitFriendDtoToSessionUnitFriendDtoMapper
    : MapperBase<SessionUnitFriendDto, SessionUnitFriendDto>
{
    public override partial SessionUnitFriendDto Map(SessionUnitFriendDto source);
    public override partial void Map(SessionUnitFriendDto source, SessionUnitFriendDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SessionSectionApplicationSessionUnitSenderInfoToSessionUnitMemberSenderDtoMapper
    : MapperBase<SessionUnitSenderInfo, SessionUnitMemberSenderDto>
{
    public override partial SessionUnitMemberSenderDto Map(SessionUnitSenderInfo source);
    public override partial void Map(SessionUnitSenderInfo source, SessionUnitMemberSenderDto destination);
}
