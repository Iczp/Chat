using System;
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
}
