using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionSections.SessionPermissionGroups;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using IczpNet.Chat.SessionSections.SessionOrganizations;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;

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
        //protected override string GetPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionPermissionGroupPermission.Default;
        //protected override string GetListPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionPermissionGroupPermission.Default;
        //protected override string CreatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionPermissionGroupPermission.Create;
        //protected override string UpdatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionPermissionGroupPermission.Update;
        //protected override string DeletePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionPermissionGroupPermission.Delete;


        protected override ITreeManager<SessionPermissionGroup, long> TreeManager => LazyServiceProvider.LazyGetRequiredService<ISessionPermissionGroupManager>();

        public SessionPermissionGroupAppService(
            IRepository<SessionPermissionGroup, long> repository)
            : base(repository)
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
