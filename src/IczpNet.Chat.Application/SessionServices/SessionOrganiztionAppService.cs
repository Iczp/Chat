using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionSections.SessionOrganizations;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionTags;
using Microsoft.AspNetCore.Mvc;
using System;
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
        public IRepository<Session, Guid> SessionRepository { get; }

        protected override ITreeManager<SessionOrganization, long> TreeManager => LazyServiceProvider.LazyGetRequiredService<ISessionOrganizationManager>();

        public SessionOrganizationAppService(
            IRepository<SessionOrganization, long> repository,
            IRepository<Session, Guid> sessionRepository) : base(repository)
        {
            SessionRepository = sessionRepository;
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

        protected override async Task<SessionOrganization> MapToEntityAsync(SessionOrganizationCreateInput createInput)
        {
            await Task.CompletedTask;

            return new SessionOrganization(createInput.Name, createInput.SessionId, createInput.ParentId);
        }

        [HttpPost]
        public override async Task<SessionOrganizationDetailDto> CreateAsync(SessionOrganizationCreateInput input)
        {
            Assert.If(!await SessionRepository.AnyAsync(x => x.Id == input.SessionId), $"No such entity of sessionId:{input.SessionId}");

            Assert.If(input.ParentId.HasValue && !await Repository.AnyAsync(x => x.ParentId == input.ParentId && x.SessionId == input.SessionId), $"No such entity of ParentId:{input.ParentId}");

            return await base.CreateAsync(input);
        }

        [HttpPost]
        public override async Task<SessionOrganizationDetailDto> UpdateAsync(long id, SessionOrganizationUpdateInput input)
        {
            if (input.ParentId.HasValue)
            {
                var perent = await Repository.GetAsync(input.ParentId.Value);

                var entity = await Repository.GetAsync(id);

                Assert.If(perent.SessionId != entity.SessionId, $"Parent session is different,ParentId:{input.ParentId}");
            }

            return await base.UpdateAsync(id, input);
        }
    }
}
