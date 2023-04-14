using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.Sessions;
using Microsoft.AspNetCore.Mvc;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;

namespace IczpNet.Chat.SessionServices
{
    public class SessionRoleAppService
        : CrudChatAppService<
            SessionRole,
            SessionRoleDetailDto,
            SessionRoleDto,
            Guid,
            SessionRoleGetListInput,
            SessionRoleCreateInput,
            SessionRoleUpdateInput>,
        ISessionRoleAppService
    {
        protected IChatObjectRepository ChatObjectRepository { get; }
        protected IRepository<Session, Guid> SessionRepository { get; }
        protected ISessionPermissionChecker SessionPermissionChecker { get; }

        public SessionRoleAppService(
            IRepository<SessionRole, Guid> repository,
            IChatObjectRepository chatObjectRepository,
            IRepository<Session, Guid> sessionRepository,
            ISessionPermissionChecker sessionPermissionChecker) : base(repository)
        {
            ChatObjectRepository = chatObjectRepository;
            SessionRepository = sessionRepository;
            SessionPermissionChecker = sessionPermissionChecker;
        }

        protected override async Task<IQueryable<SessionRole>> CreateFilteredQueryAsync(SessionRoleGetListInput input)
        {
            return await base.CreateFilteredQueryAsync(input)

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

            var entity = new SessionRole(GuidGenerator.Create(), createInput.SessionId, createInput.Name);

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

        [HttpPost]
        public override Task<SessionRoleDetailDto> UpdateAsync(Guid id, SessionRoleUpdateInput input)
        {
            return base.UpdateAsync(id, input);
        }

        [HttpGet]
        public async Task<SessionRolePermissionDto> GetPermissions(Guid id)
        {
            var entity = await GetEntityByIdAsync(id);
            return ObjectMapper.Map<SessionRole, SessionRolePermissionDto>(entity);
        }

        [HttpPost]
        public async Task DeleteByAsync(Guid sessionUnitId, Guid id)
        {
            await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.SessionPermissionRole.Delete, sessionUnitId);

            await base.DeleteAsync(id);
        }
    }
}
