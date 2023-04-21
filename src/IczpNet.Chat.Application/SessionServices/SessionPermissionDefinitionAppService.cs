using IczpNet.AbpCommons.Extensions;
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
using System.Collections.Generic;
using IczpNet.Chat.SessionSections.SessionOrganizations;

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
        protected ISessionPermissionGroupManager SessionPermissionGroupManager { get; }

        public SessionPermissionDefinitionAppService(
            IRepository<SessionPermissionDefinition, string> repository,
            IChatObjectRepository chatObjectRepository,
            IRepository<Session, Guid> sessionRepository,
            ISessionPermissionGroupManager sessionPermissionGroupManager) : base(repository)
        {
            ChatObjectRepository = chatObjectRepository;
            SessionRepository = sessionRepository;
            SessionPermissionGroupManager = sessionPermissionGroupManager;
        }

        protected override async Task<IQueryable<SessionPermissionDefinition>> CreateFilteredQueryAsync(SessionPermissionDefinitionGetListInput input)
        {

            IQueryable<long> groupIdQuery = null;

            if (input.IsImportChildGroup && input.GroupIdList.IsAny())
            {
                groupIdQuery = (await SessionPermissionGroupManager.QueryCurrentAndAllChildsAsync(input.GroupIdList)).Select(x => x.Id);
            }
            return (await base.CreateFilteredQueryAsync(input))
                //GroupId
                .WhereIf(!input.IsImportChildGroup && input.GroupIdList.IsAny(), x => input.GroupIdList.Contains(x.GroupId.Value))
                .WhereIf(input.IsImportChildGroup && input.GroupIdList.IsAny(), x => groupIdQuery.Contains(x.GroupId.Value))
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword))
                ;
        }

        protected override IQueryable<SessionPermissionDefinition> ApplyDefaultSorting(IQueryable<SessionPermissionDefinition> query)
        {
            return query.OrderByDescending(x => x.Group.Sorting)
                .ThenBy(x => x.Group.FullPathName)
                .ThenByDescending(x => x.Sorting);
        }

        [HttpPost]
        [RemoteService(false)]
        public override Task<SessionPermissionDefinitionDetailDto> CreateAsync(SessionPermissionDefinitionCreateInput input)
        {
            throw new NotImplementedException();
        }

        [RemoteService(false)]
        public override Task DeleteAsync(string id)
        {
            return base.DeleteAsync(id);
        }

        [RemoteService(false)]
        public override Task DeleteManyAsync(List<string> idList)
        {
            return base.DeleteManyAsync(idList);
        }
    }
}
