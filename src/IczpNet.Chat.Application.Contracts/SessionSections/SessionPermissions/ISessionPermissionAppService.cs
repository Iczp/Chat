using IczpNet.Chat.SessionSections.SessionPermissions.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.SessionPermissions
{
    public interface ISessionPermissionAppService
    {
        Task<Dictionary<string, PermissionGrantValue>> GetGrantedByRoleAsync(Guid roleId);

        Task<SessionPermissionGrantDto> GetGrantedBySessionUnitAsync(string permissionDefinitionId, Guid sessionUnitId);
    }
}
