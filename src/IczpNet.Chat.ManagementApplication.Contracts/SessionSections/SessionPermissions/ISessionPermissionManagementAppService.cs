using IczpNet.Chat.Management.SessionSections.SessionPermissions.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.Management.SessionSections.SessionPermissions
{
    public interface ISessionPermissionManagementAppService
    {
        Task<Dictionary<string, PermissionGrantValue>> GetGrantedBySessionRoleAsync(Guid sessionRoleId);

        Task<SessionPermissionGrantDto> GetGrantedBySessionUnitAsync(string definitionId, Guid sessionUnitId);

        Task<SessionPermissionUnitGrantDto> GrantBySessionUnitAsync(string definitionId, Guid sessionUnitId, PermissionGrantValue permissionGrantValue);

        Task<SessionPermissionRoleGrantDto> GrantBySessionRoleAsync(string definitionId, Guid sessionRoleId, PermissionGrantValue permissionGrantValue);
    }
}
