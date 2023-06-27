using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionSections.SessionOrganizations;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionServices
{
    public class SessionOrganiztionBySessionUnitAppService
        : CrudBySessionUnitTreeChatAppService<
            SessionOrganization,
            long,
            SessionOrganizationDetailDto,
            SessionOrganizationDto,
            SessionOrganizationGetListBySessionUnitInput,
            SessionOrganizationCreateBySessionUnitInput,
            SessionOrganizationUpdateInput>,
        ISessionOrganiztionBySessionUnitAppService
    {
        //protected override string GetPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionOrganizationPermission.Default;
        //protected override string GetListPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionOrganizationPermission.Default;
        //protected override string CreatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionOrganizationPermission.Create;
        //protected override string UpdatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionOrganizationPermission.Update;
        //protected override string DeletePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionOrganizationPermission.Delete;
        //protected override string DeleteManyPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionOrganizationPermission.Delete;

        public SessionOrganiztionBySessionUnitAppService(
            IRepository<SessionOrganization, long> repository,
            ISessionOrganizationManager sessionOrganizationManager)
            : base(repository, sessionOrganizationManager)
        {
        }

        protected override async Task<IQueryable<SessionOrganization>> CreateFilteredQueryAsync(SessionUnit sessionUnit, SessionOrganizationGetListBySessionUnitInput input)
        {
            Assert.If(!input.IsEnabledParentId && input.ParentId.HasValue, "When [IsEnabledParentId]=false,then [ParentId] != null");

            return (await Repository.GetQueryableAsync())
                .Where(x => x.SessionId == sessionUnit.SessionId)
                .WhereIf(input.DepthList.IsAny(), (x) => input.DepthList.Contains(x.Depth))
                .WhereIf(input.IsEnabledParentId, (x) => x.ParentId == input.ParentId)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.Contains(input.Keyword));
        }

        protected override async Task<SessionOrganization> MapToEntityAsync(SessionUnit sessionUnit, SessionOrganizationCreateBySessionUnitInput createInput)
        {
            await Task.Yield();

            return new SessionOrganization(createInput.Name, sessionUnit.SessionId.Value, createInput.ParentId);
        }

        protected override async Task CheckCreatePolicyAsync(SessionUnit sessionUnit, SessionOrganizationCreateBySessionUnitInput input)
        {
            Assert.If(input.ParentId.HasValue && !await Repository.AnyAsync(x => x.Id == input.ParentId && x.SessionId == sessionUnit.SessionId), $"No such entity of ParentId:{input.ParentId}");

            await base.CheckCreatePolicyAsync(sessionUnit, input);
        }

        protected override async Task CheckUpdatePolicyAsync(SessionUnit sessionUnit, SessionOrganization entity, SessionOrganizationUpdateInput input)
        {
            Assert.If(sessionUnit.SessionId != entity.SessionId, $"Not in same session");

            if (input.ParentId.HasValue)
            {
                var perent = await Repository.GetAsync(input.ParentId.Value);
                Assert.If(perent.SessionId != entity.SessionId, $"Parent session is different, ParentId:{input.ParentId}");
            }

            await base.CheckUpdatePolicyAsync(sessionUnit, entity, input);
        }

        protected override Task CheckDeletePolicyAsync(SessionUnit sessionUnit, SessionOrganization entity)
        {
            Assert.If(sessionUnit.SessionId != entity.SessionId, $"Not in same session");

            return base.CheckDeletePolicyAsync(sessionUnit, entity);
        }

        protected override Expression<Func<SessionOrganization, bool>> GetPredicateDeleteManyAsync(SessionUnit sessionUnit)
        {
            return x => x.SessionId == sessionUnit.SessionId;
        }
    }
}
