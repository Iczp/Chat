using System.Threading.Tasks;
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

    Task<SessionPermissionDefinitionDto> SetIsEnabledAsync(string id, bool isEnabled);

    Task<int> SetAllIsEnabledAsync(bool isEnabled);
}
