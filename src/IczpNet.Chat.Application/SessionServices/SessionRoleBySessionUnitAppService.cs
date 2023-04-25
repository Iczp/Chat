using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits;
using System.Linq.Expressions;
using IczpNet.Chat.Permissions;

namespace IczpNet.Chat.SessionServices
{
    public class SessionRoleBySessionUnitAppService
        : CrudBySessionUnitChatAppService<
            SessionRole,
            SessionRoleDetailDto,
            SessionRoleDto,
            Guid,
            SessionRoleGetListBySessionUnitInput,
            SessionRoleCreateBySessionUnitInput,
            SessionRoleUpdateInput>,
        ISessionRoleBySessionUnitAppService
    {
        protected override string GetPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Default;
        protected override string GetListPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Default;
        protected override string CreatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Create;
        protected override string UpdatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Update;
        protected override string DeletePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Delete;
        protected override string DeleteManyPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Delete;
        protected ISessionRoleManager SessionRoleManager { get; }

        public SessionRoleBySessionUnitAppService(
            IRepository<SessionRole, Guid> repository)
            : base(repository)
        {
        }

        protected override async Task<IQueryable<SessionRole>> CreateFilteredQueryAsync(SessionUnit sessionUnit, SessionRoleGetListBySessionUnitInput input)
        {
            return (await base.CreateFilteredQueryAsync(sessionUnit, input))
                .Where(x => x.SessionId.Value == sessionUnit.SessionId);
            ;
        }

        protected override Task SetCreateEntityAsync(SessionRole entity, SessionRoleCreateBySessionUnitInput input)
        {
            entity.SetPermissionGrant(input.PermissionGrant);
            //entity.SessionId = 
            return base.SetCreateEntityAsync(entity, input);
        }

        protected override async Task<SessionRole> MapToEntityAsync(SessionUnit sessionUnit, SessionRoleCreateBySessionUnitInput input)
        {
            await Task.CompletedTask;

            var entity = new SessionRole(GuidGenerator.Create(), sessionUnit.SessionId.Value, input.Name);

            return entity;
        }

        protected override async Task CheckCreatePolicyAsync(SessionUnit sessionUnit, SessionRoleCreateBySessionUnitInput input)
        {
            Assert.If(!await SessionRepository.AnyAsync(x => x.Id == sessionUnit.SessionId), $"No such entity of sessionId:{sessionUnit.SessionId}");

            Assert.If(await Repository.AnyAsync(x => x.SessionId == sessionUnit.SessionId && x.Name == input.Name), $"Already exists [{input.Name}].");

            await base.CheckCreatePolicyAsync(sessionUnit, input);
        }

        protected override Task CheckUpdatePolicyAsync(SessionUnit sessionUnit, SessionRole entity, SessionRoleUpdateInput input)
        {
            Assert.If(sessionUnit.SessionId != entity.SessionId, $"Not in same session");

            return base.CheckUpdatePolicyAsync(sessionUnit, entity, input);
        }

        protected override Task SetUpdateEntityAsync(SessionRole entity, SessionRoleUpdateInput input)
        {
            entity.SetPermissionGrant(input.PermissionGrant);

            return base.SetUpdateEntityAsync(entity, input);
        }

        protected virtual SessionRolePermissionDto MapToPermissionDto(SessionRole entity)
        {
            return ObjectMapper.Map<SessionRole, SessionRolePermissionDto>(entity);
        }

        protected virtual async Task<SessionRolePermissionDto> MapToPermissionDtoAsync(SessionRole entity)
        {
            await Task.CompletedTask;

            return MapToPermissionDto(entity);
        }

        protected override Task CheckDeletePolicyAsync(SessionUnit sessionUnit, SessionRole entity)
        {
            Assert.If(sessionUnit.SessionId != entity.SessionId, $"Not in same session");

            return base.CheckDeletePolicyAsync(sessionUnit, entity);
        }

        protected override Expression<Func<SessionRole, bool>> GetPredicateDeleteManyAsync(SessionUnit sessionUnit)
        {
            return x => x.SessionId == sessionUnit.SessionId;
        }
    }
}
