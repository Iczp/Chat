using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions.Dtos;
using Microsoft.AspNetCore.Mvc;
using IczpNet.Chat.SessionSections.Sessions;

namespace IczpNet.Chat.SessionServices
{
    public class SessionPermissionDefinitionAppService
        : CrudChatAppService<
            SessionPermissionDefinition,
            SessionPermissionDefinitionDetailDto,
            SessionPermissionDefinitionDto,
            string,
            SessionPermissionDefinitionGetListInput,
            SessionPermissionDefinitionCreateInput,
            SessionPermissionDefinitionUpdateInput>,
        ISessionPermissionDefinitionAppService
    {
        protected IChatObjectRepository ChatObjectRepository { get; }
        protected IRepository<Session, Guid> SessionRepository { get; }
        public SessionPermissionDefinitionAppService(
            IRepository<SessionPermissionDefinition, string> repository,
            IChatObjectRepository chatObjectRepository,
            IRepository<Session, Guid> sessionRepository) : base(repository)
        {
            ChatObjectRepository = chatObjectRepository;
            SessionRepository = sessionRepository;
        }

        protected override async Task<IQueryable<SessionPermissionDefinition>> CreateFilteredQueryAsync(SessionPermissionDefinitionGetListInput input)
        {
            return await base.CreateFilteredQueryAsync(input)

                ;
        }

        [HttpPost]
        [RemoteService(false)]
        public override Task<SessionPermissionDefinitionDetailDto> CreateAsync(SessionPermissionDefinitionCreateInput input)
        {
            throw new NotImplementedException();
        }


        [RemoteService(false)]
        public override Task<SessionPermissionDefinitionDetailDto> UpdateAsync(string id, SessionPermissionDefinitionUpdateInput input)
        {
            return base.UpdateAsync(id, input);
        }
    }
}
