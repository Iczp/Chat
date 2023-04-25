using IczpNet.AbpTrees;
using IczpNet.Chat.Management.SessionSections.SessionOrganiztions.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Management.SessionSections.SessionPermissionGroups
{
    public interface ISessionPermissionGroupManagementAppService :
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
            SessionPermissionGroupUpdateInput>
    {

    }
}
