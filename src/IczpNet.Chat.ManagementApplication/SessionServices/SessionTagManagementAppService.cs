using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using IczpNet.Chat.SessionSections.SessionTags;
using Microsoft.AspNetCore.Mvc;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.Management.BaseAppServices;
using IczpNet.Chat.Management.SessionSections.SessionTags.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionTags;

namespace IczpNet.Chat.Management.SessionServices
{
    public class SessionTagManagementAppService
        : CrudChatManagementAppService<
            SessionTag,
            SessionTagDetailDto,
            SessionTagDto,
            Guid,
            SessionTagGetListInput,
            SessionTagCreateInput,
            SessionTagUpdateInput>,
        ISessionTagManagementAppService
    {
        protected IChatObjectRepository ChatObjectRepository { get; }
        protected IRepository<Session, Guid> SessionRepository { get; }
        public SessionTagManagementAppService(
            IRepository<SessionTag, Guid> repository,
            IChatObjectRepository chatObjectRepository,
            IRepository<Session, Guid> sessionRepository) : base(repository)
        {
            ChatObjectRepository = chatObjectRepository;
            SessionRepository = sessionRepository;
        }

        protected override async Task<IQueryable<SessionTag>> CreateFilteredQueryAsync(SessionTagGetListInput input)
        {
            return await base.CreateFilteredQueryAsync(input)

                ;
        }

        protected override async Task<SessionTag> MapToEntityAsync(SessionTagCreateInput createInput)
        {
            //var owner = Assert.NotNull(await ChatObjectRepository.FindAsync(createInput.SessionId), $"No such Entity by OwnerId:{createInput.SessionId}.");

            Assert.If(await Repository.AnyAsync(x => x.SessionId == createInput.SessionId && x.Name == createInput.Name), $"Already exists [{createInput.Name}].");

            return new SessionTag(GuidGenerator.Create(), createInput.SessionId, createInput.Name);
        }

        [HttpPost]
        public override async Task<SessionTagDetailDto> CreateAsync(SessionTagCreateInput input)
        {
            Assert.If(!await SessionRepository.AnyAsync(x => x.Id == input.SessionId), $"No such entity of sessionId:{input.SessionId}");

            return await base.CreateAsync(input);
        }


        [RemoteService(false)]
        public override Task<SessionTagDetailDto> UpdateAsync(Guid id, SessionTagUpdateInput input)
        {
            return base.UpdateAsync(id, input);
        }
    }
}
