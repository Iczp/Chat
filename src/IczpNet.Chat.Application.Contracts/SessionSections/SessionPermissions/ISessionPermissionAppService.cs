using IczpNet.Chat.SessionSections.SessionPermissionDefinitions.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissions.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionPermissions;

public interface ISessionPermissionAppService
{
    Task<Dictionary<string, PermissionGrantValue>> GetGrantedBySessionRoleAsync(Guid sessionRoleId);

    Task<SessionPermissionGrantDto> GetGrantedBySessionUnitAsync(string definitionId, Guid sessionUnitId);

    Task<PagedResultDto<SessionPermissionGrantDto>> GetAllGrantedBySessionUnitAsync(Guid sessionUnitId);

    Task<List<SessionPermissionDefinitionTreeDto>> GetDefinitionsAsync();

    Task<SessionPermissionUnitGrantDto> GrantBySessionUnitAsync(string definitionId, Guid sessionUnitId, PermissionGrantValue permissionGrantValue);

    Task<SessionPermissionRoleGrantDto> GrantBySessionRoleAsync(string definitionId, Guid sessionRoleId, PermissionGrantValue permissionGrantValue);
}
