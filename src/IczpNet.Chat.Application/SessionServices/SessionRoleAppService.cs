using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using Microsoft.AspNetCore.Mvc;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;

namespace IczpNet.Chat.SessionServices
{
    public class SessionRoleAppService
        : CrudWithSessionUnitChatAppService<
            SessionRole,
            SessionRoleDetailDto,
            SessionRoleDto,
            Guid,
            SessionRoleGetListInput,
            SessionRoleCreateInput,
            SessionRoleUpdateInput>,
        ISessionRoleAppService
    {
        //protected override string GetPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Default;
        //protected override string GetListPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Default;
        //protected override string CreatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Create;
        //protected override string UpdatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Update;
        //protected override string DeletePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Delete;
        protected override string GetBySessionUnitPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Default;
        protected override string GetListBySessionUnitPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Default;
        protected override string CreateBySessionUnitPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Create;
        protected override string UpdateBySessionUnitPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Update;
        protected override string DeleteBySessionUnitPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Delete;
        protected override string DeleteManyBySessionUnitPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Delete;

        protected IChatObjectRepository ChatObjectRepository { get; }

        public SessionRoleAppService(
            IRepository<SessionRole, Guid> repository,
            IChatObjectRepository chatObjectRepository)
            : base(repository)
        {
            ChatObjectRepository = chatObjectRepository;
        }

        protected override async Task<IQueryable<SessionRole>> CreateFilteredQueryAsync(SessionRoleGetListInput input)
        {
            return (await base.CreateFilteredQueryAsync(input))
                .WhereIf(input.SessionId.HasValue, x => x.SessionId.Value == input.SessionId);
            ;
        }

        protected override async Task CheckCreateAsync(SessionRoleCreateInput input)
        {
            Assert.If(!await SessionRepository.AnyAsync(x => x.Id == input.SessionId), $"No such entity of sessionId:{input.SessionId}");
            Assert.If(await Repository.AnyAsync(x => x.SessionId == input.SessionId && x.Name == input.Name), $"Already exists [{input.Name}].");
        }

        protected override async Task<SessionRole> MapToEntityAsync(SessionRoleCreateInput createInput)
        {
            await Task.CompletedTask;

            var entity = new SessionRole(GuidGenerator.Create(), createInput.SessionId.Value, createInput.Name);

            return entity;
        }

        protected override Task SetCreateEntityAsync(SessionRole entity, SessionRoleCreateInput input)
        {
            entity.SetPermissionGrant(input.PermissionGrant);
            return base.SetCreateEntityAsync(entity, input);
        }

        protected override Task SetUpdateEntityAsync(SessionRole entity, SessionRoleUpdateInput input)
        {
            entity.SetPermissionGrant(input.PermissionGrant);
            return base.SetUpdateEntityAsync(entity, input);
        }

        [HttpGet]
        public async Task<SessionRolePermissionDto> GetPermissions(Guid id)
        {
            var entity = await GetEntityByIdAsync(id);
            return ObjectMapper.Map<SessionRole, SessionRolePermissionDto>(entity);
        }
    }
}
