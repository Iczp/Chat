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
using IczpNet.Chat.SessionSections.Sessions;
using System.Collections.Generic;
using IczpNet.Chat.SessionSections.SessionOrganizations;
using Microsoft.AspNetCore.Mvc;

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

        protected override string UpdatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionPermissionDefinitionPermission.Update;
        protected virtual string SetIsEnabledPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionPermissionDefinitionPermission.SetIsEnabled;
        protected virtual string SetAllIsEnabledPolicyName { get; set; } //= SessionPermissionDefinitionConsts.SessionPermissionDefinitionPermission.SetAllIsEnabled;
        protected IChatObjectRepository ChatObjectRepository { get; }
        protected IRepository<Session, Guid> SessionRepository { get; }
        protected ISessionPermissionGroupManager SessionPermissionGroupManager { get; }
        protected new ISessionPermissionDefinitionRepository Repository { get; }

        public SessionPermissionDefinitionAppService(
            ISessionPermissionDefinitionRepository repository,
            IChatObjectRepository chatObjectRepository,
            IRepository<Session, Guid> sessionRepository,
            ISessionPermissionGroupManager sessionPermissionGroupManager) : base(repository)
        {
            Repository = repository;
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

        [RemoteService(false)]
        public override Task<SessionPermissionDefinitionDetailDto> CreateAsync(SessionPermissionDefinitionCreateInput input) => throw new NotImplementedException();

        [RemoteService(false)]
        public override Task DeleteAsync(string id) => base.DeleteAsync(id);

        [RemoteService(false)]
        public override Task DeleteManyAsync(List<string> idList)
        {
            return base.DeleteManyAsync(idList);
        }

        public async Task<SessionPermissionDefinitionDto> SetIsEnabledAsync(string id, bool isEnabled)
        {
            await CheckPolicyAsync(SetIsEnabledPolicyName);

            var entity = await Repository.GetAsync(id);

            entity.IsEnabled = isEnabled;

            await Repository.UpdateAsync(entity, autoSave: true);

            return await MapToGetOutputDtoAsync(entity);
        }

        [HttpPost]
        public virtual async Task<int> SetAllIsEnabledAsync(bool isEnabled)
        {
            await CheckPolicyAsync(SetAllIsEnabledPolicyName);

            return await Repository.BatchUpdateIsEnabledAsync(isEnabled);
        }
    }
}
