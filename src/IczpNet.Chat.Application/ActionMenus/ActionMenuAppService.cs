using IczpNet.AbpTrees;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ActionMenus;
using IczpNet.Chat.ActionMenus.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Services
{
    public class ActionMenuAppService
        : CrudTreeChatAppService<
            ActionMenu,
            long,
            ActionMenuDto,
            ActionMenuDto,
            ActionMenuGetListInput,
            ActionMenuCreateInput,
            ActionMenuUpdateInput,
            ActionMenuInfo>,
        IActionMenuAppService
    {
        protected IActionMenuManager ActionMenuManager { get; }
        protected override ITreeManager<ActionMenu, long> TreeManager => LazyServiceProvider.LazyGetRequiredService<IActionMenuManager>();
        protected ISessionPermissionChecker SessionPermissionChecker { get; }
        public ActionMenuAppService(
            IRepository<ActionMenu,long> repository,
            IActionMenuManager chatObjectManager,
            ISessionPermissionChecker sessionPermissionChecker) : base(repository)
        {

            ActionMenuManager = chatObjectManager;
            SessionPermissionChecker = sessionPermissionChecker;
        }

        protected override async Task<IQueryable<ActionMenu>> CreateFilteredQueryAsync(ActionMenuGetListInput input)
        {
         
            return (await base.CreateFilteredQueryAsync(input))
                .WhereIf(!input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
              
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword))
                ;
        }

    }
}
