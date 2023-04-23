using System;
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
}
