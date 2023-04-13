using System;
using System.Threading.Tasks;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.SessionSections.SessionRoles;

public interface ISessionRoleAppService :
    ICrudAppService<
        SessionRoleDetailDto,
        SessionRoleDto,
        Guid,
        SessionRoleGetListInput,
        SessionRoleCreateInput,
        SessionRoleUpdateInput>
{

    Task<SessionRolePermissionDto> GetPermissions(Guid id);

    Task DeleteAsync(Guid sessionUnitId, Guid id);
}
