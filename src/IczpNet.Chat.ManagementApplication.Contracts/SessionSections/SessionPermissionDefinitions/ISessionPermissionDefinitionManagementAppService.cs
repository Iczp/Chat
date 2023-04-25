using System.Threading.Tasks;
using IczpNet.Chat.Management.SessionSections.SessionPermissionDefinitions.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Management.Management.SessionSections.SessionPermissionDefinitions;

public interface ISessionPermissionDefinitionManagementAppService :
    ICrudAppService<
        SessionPermissionDefinitionDetailDto,
        SessionPermissionDefinitionDto,
        string,
        SessionPermissionDefinitionGetListInput,
        SessionPermissionDefinitionCreateInput,
        SessionPermissionDefinitionUpdateInput>
{

    Task<SessionPermissionDefinitionDto> SetIsEnabledAsync(string id, bool isEnabled);

    Task<int> SetAllIsEnabledAsync(bool isEnabled);
}
