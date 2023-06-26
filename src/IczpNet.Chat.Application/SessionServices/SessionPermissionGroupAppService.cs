using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionSections.SessionOrganizations;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissionGroups;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionServices
{
    public class SessionPermissionGroupAppService
        : CrudTreeChatAppService<
            SessionPermissionGroup,
            long,
            SessionPermissionGroupDetailDto,
            SessionPermissionGroupDto,
            SessionPermissionGroupGetListInput,
            SessionPermissionGroupCreateInput,
            SessionPermissionGroupUpdateInput,
            SessionPermissionGroupInfo>,
        ISessionPermissionGroupAppService
    {
        protected override string GetPolicyName { get; set; } = ChatPermissions.SessionPermissionGroupPermission.Default;
        protected override string GetListPolicyName { get; set; } = ChatPermissions.SessionPermissionGroupPermission.Default;
        protected override string CreatePolicyName { get; set; } = ChatPermissions.SessionPermissionGroupPermission.Create;
        protected override string UpdatePolicyName { get; set; } = ChatPermissions.SessionPermissionGroupPermission.Update;
        protected override string DeletePolicyName { get; set; } = ChatPermissions.SessionPermissionGroupPermission.Delete;

        public SessionPermissionGroupAppService(
            IRepository<SessionPermissionGroup, long> repository,
            ISessionPermissionGroupManager sessionPermissionGroupManager)
            : base(repository, sessionPermissionGroupManager)
        {
        }

        protected override async Task<IQueryable<SessionPermissionGroup>> CreateFilteredQueryAsync(SessionPermissionGroupGetListInput input)
        {
            Assert.If(!input.IsEnabledParentId && input.ParentId.HasValue, "When [IsEnabledParentId]=false,then [ParentId] != null");

            return (await Repository.GetQueryableAsync())
                .WhereIf(input.Depth.HasValue, (x) => x.Depth == input.Depth)
                .WhereIf(input.IsEnabledParentId, (x) => x.ParentId == input.ParentId)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.Contains(input.Keyword));
        }
    }
}
