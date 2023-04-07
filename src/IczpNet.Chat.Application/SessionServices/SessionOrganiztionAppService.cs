using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionSections.SessionOrganizations;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionServices
{
    public class SessionOrganizationAppService
        : CrudTreeChatAppService<
            SessionOrganization,
            long,
            SessionOrganizationDetailDto,
            SessionOrganizationDto,
            SessionOrganizationGetListInput,
            SessionOrganizationCreateInput,
            SessionOrganizationUpdateInput,
            SessionOrganizationInfo>,
        ISessionOrganizationAppService
    {
        public SessionOrganizationAppService(IRepository<SessionOrganization, long> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<SessionOrganization>> CreateFilteredQueryAsync(SessionOrganizationGetListInput input)
        {

            Assert.If(!input.IsEnabledParentId && input.ParentId.HasValue, "When [IsEnabledParentId]=false,then [ParentId] != null");

            return (await Repository.GetQueryableAsync())
                //.WhereIf(input.Depth.HasValue, (x) => x.Depth == input.Depth)
                .WhereIf(input.SessionId.HasValue, (x) => x.SessionId == input.SessionId)
                .WhereIf(input.IsEnabledParentId, (x) => x.ParentId == input.ParentId)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.Contains(input.Keyword));


        }
    }
}
