using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.SessionTags.Dtos;
using Microsoft.AspNetCore.Mvc;
using IczpNet.Chat.SessionSections.Sessions;

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
        public IRepository<Session, Guid> SessionRepository { get; }

        public SessionRoleAppService(
            IRepository<SessionRole, Guid> repository,
            IChatObjectRepository chatObjectRepository,
            IRepository<Session, Guid> sessionRepository) : base(repository)
        {
            ChatObjectRepository = chatObjectRepository;
            SessionRepository = sessionRepository;
        }

        protected override async Task<IQueryable<SessionRole>> CreateFilteredQueryAsync(SessionRoleGetListInput input)
        {
            return await base.CreateFilteredQueryAsync(input)

                ;
        }

        protected override async Task<SessionRole> MapToEntityAsync(SessionRoleCreateInput createInput)
        {
            //var owner = Assert.NotNull(await ChatObjectRepository.FindAsync(createInput.SessionId), $"No such Entity by OwnerId:{createInput.SessionId}.");

            Assert.If(await Repository.AnyAsync(x => x.SessionId == createInput.SessionId && x.Name == createInput.Name), $"Already exists [{createInput.Name}].");

            return new SessionRole(GuidGenerator.Create(), createInput.SessionId, createInput.Name);
        }

        [HttpPost]
        public override async Task<SessionRoleDetailDto> CreateAsync(SessionRoleCreateInput input)
        {
            Assert.If(!await SessionRepository.AnyAsync(x => x.Id == input.SessionId), $"No such entity of sessionId:{input.SessionId}");

            return await base.CreateAsync(input);
        }

        [RemoteService(false)]
        public override Task<SessionRoleDetailDto> UpdateAsync(Guid id, SessionRoleUpdateInput input)
        {
            return base.UpdateAsync(id, input);
        }
    }
}
