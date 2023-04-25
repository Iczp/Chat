using System;
using System.Threading.Tasks;
using IczpNet.Chat.Management.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissions;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Management.SessionSections.SessionRoles;

public interface ISessionRoleManagementAppService :
    ICrudAppService<
        SessionRoleDetailDto,
        SessionRoleDto,
        Guid,
        SessionRoleGetListInput,
        SessionRoleCreateInput,
        SessionRoleUpdateInput>
{

    Task<SessionRolePermissionDto> GetPermissionsAsync(Guid id);

    Task<SessionRolePermissionDto> SetAllPermissionsAsync(Guid id, PermissionGrantValue permissionGrantValue);
}
