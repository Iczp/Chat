using System;
using System.Threading.Tasks;
using IczpNet.Chat.SessionPermissions;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.SessionSections.SessionRoles;

public interface ISessionRoleBySessionUnitAppService :
    ICrudAppService<
        SessionRoleDetailDto,
        SessionRoleDto,
        Guid,
        SessionRoleGetListBySessionUnitInput,
        SessionRoleCreateBySessionUnitInput,
        SessionRoleUpdateInput>
{
    Task<SessionRolePermissionDto> GetPermissionsAsync(Guid sessionUnitId, Guid id);

    Task<SessionRolePermissionDto> SetAllPermissionsAsync(Guid sessionUnitId, Guid id, PermissionGrantValue permissionGrantValue);
}
