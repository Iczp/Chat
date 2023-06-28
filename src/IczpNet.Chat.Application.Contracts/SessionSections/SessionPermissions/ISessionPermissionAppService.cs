using IczpNet.Chat.SessionSections.SessionPermissions.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.SessionPermissions
{
    public interface ISessionPermissionAppService
    {
        Task<Dictionary<string, PermissionGrantValue>> GetGrantedBySessionRoleAsync(Guid sessionRoleId);

        Task<SessionPermissionGrantDto> GetGrantedBySessionUnitAsync(string definitionId, Guid sessionUnitId);

        Task<SessionPermissionUnitGrantDto> GrantBySessionUnitAsync(string definitionId, Guid sessionUnitId, PermissionGrantValue permissionGrantValue);

        Task<SessionPermissionRoleGrantDto> GrantBySessionRoleAsync(string definitionId, Guid sessionRoleId, PermissionGrantValue permissionGrantValue);
    }
}
