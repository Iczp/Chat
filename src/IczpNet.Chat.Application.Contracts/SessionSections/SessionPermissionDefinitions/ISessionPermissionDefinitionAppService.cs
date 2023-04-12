using System;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.SessionSections.SessionPermissionDefinitions;

public interface ISessionPermissionDefinitionAppService :
    ICrudAppService<
        SessionPermissionDefinitionDetailDto,
        SessionPermissionDefinitionDto,
        string,
        SessionPermissionDefinitionGetListInput,
        SessionPermissionDefinitionCreateInput,
        SessionPermissionDefinitionUpdateInput>
{
}
