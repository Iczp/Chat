using IczpNet.AbpTrees;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Menus;
using IczpNet.Chat.Menus.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Menus
{
    public class MenuAppService
        : CrudTreeChatAppService<
            Menu,
            Guid,
            MenuDto,
            MenuDto,
            MenuGetListInput,
            MenuCreateInput,
            MenuUpdateInput,
            MenuInfo>,
        IMenuAppService
    {
        protected IMenuManager ActionMenuManager { get; }
        protected override ITreeManager<Menu, Guid> TreeManager => LazyServiceProvider.LazyGetRequiredService<IMenuManager>();
        protected ISessionPermissionChecker SessionPermissionChecker { get; }
        public MenuAppService(
            IRepository<Menu, Guid> repository,
            IMenuManager chatObjectManager,
            ISessionPermissionChecker sessionPermissionChecker) : base(repository)
        {

            ActionMenuManager = chatObjectManager;
            SessionPermissionChecker = sessionPermissionChecker;
        }

        protected override async Task<IQueryable<Menu>> CreateFilteredQueryAsync(MenuGetListInput input)
        {
         
            return (await base.CreateFilteredQueryAsync(input))
                .WhereIf(!input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
              
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword))
                ;
        }

    }
}
