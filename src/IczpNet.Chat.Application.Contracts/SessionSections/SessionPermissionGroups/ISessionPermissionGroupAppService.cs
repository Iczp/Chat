using IczpNet.AbpTrees;
using IczpNet.Chat.SessionPermissionGroups;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissionGroups.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.SessionSections.SessionPermissionGroups;

public interface ISessionPermissionGroupAppService :
    ICrudAppService<
        SessionPermissionGroupDetailDto,
        SessionPermissionGroupDto,
        long,
        SessionPermissionGroupGetListInput,
        SessionPermissionGroupCreateInput,
        SessionPermissionGroupUpdateInput>
    ,
    ITreeAppService<SessionPermissionGroupDetailDto,
        SessionPermissionGroupDto,
        long,
        SessionPermissionGroupGetListInput,
        SessionPermissionGroupCreateInput,
        SessionPermissionGroupUpdateInput, SessionPermissionGroupInfo>
{

}
