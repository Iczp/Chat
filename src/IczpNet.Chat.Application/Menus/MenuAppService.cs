using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Menus.Dtos;
using IczpNet.Chat.SessionPermissions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Menus;

/// <summary>
/// 会话菜单（公众号菜单等）
/// </summary>
public class MenuAppService(
    IRepository<Menu, Guid> repository,
    IMenuManager menuManager,
    ISessionPermissionChecker sessionPermissionChecker,
    IChatObjectRepository chatObjectRepository)
        : CrudTreeChatAppService<
        Menu,
        Guid,
        MenuDto,
        MenuDto,
        MenuGetListInput,
        MenuCreateInput,
        MenuUpdateInput,
        MenuInfo>(repository, menuManager),
    IMenuAppService
{
    protected ISessionPermissionChecker SessionPermissionChecker { get; } = sessionPermissionChecker;
    protected IChatObjectRepository ChatObjectRepository { get; } = chatObjectRepository;
    protected new IMenuManager TreeManager { get; } = menuManager;

    protected override async Task<IQueryable<Menu>> CreateFilteredQueryAsync(MenuGetListInput input)
    {
     
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword))
            ;
    }

    /// <summary>
    /// 添加菜单
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public override async Task<MenuDto> CreateAsync(MenuCreateInput input)
    {
        Assert.If(!await ChatObjectRepository.AnyAsync(x => x.Id == input.OwnerId), $"No such entity of OwnerId:{input.OwnerId}");

        Assert.If(input.ParentId.HasValue && !await Repository.AnyAsync(x => x.Id == input.ParentId && x.OwnerId == input.OwnerId), $"No such entity of ParentId:{input.ParentId}");

        return await base.CreateAsync(input);
    }

    /// <summary>
    /// 修改菜单
    /// </summary>
    /// <param name="id">菜单Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public override async Task<MenuDto> UpdateAsync(Guid id, MenuUpdateInput input)
    {
        if (input.ParentId.HasValue)
        {
            var perent = await Repository.GetAsync(input.ParentId.Value);

            var entity = await Repository.GetAsync(id);

            Assert.If(perent.OwnerId != entity.OwnerId, $"Parent owner is different,ParentId:{input.ParentId}");
        }
        return await base.UpdateAsync(id, input);
    }

    /// <summary>
    /// 获取指定聊天对象(公众号/订阅号)的菜单列表
    /// </summary>
    /// <param name="ownerId">聊天对象Id</param>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<List<MenuDto>> GetListByOwnerIdAsync(long ownerId)
    {
        var entities = await Repository.GetListAsync(x => x.OwnerId == ownerId);
        var ordered = entities.OrderByDescending(x => x.Sorting).ToList();
        return ObjectMapper.Map<List<Menu>, List<MenuDto>>(ordered);
    }

    /// <summary>
    /// 菜单触发器(调用后台作业)
    /// </summary>
    /// <param name="id">菜单Id</param>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <returns></returns>
    [HttpPost]
    [HttpGet]
    public virtual async Task<string> TriggerAsync(Guid id, Guid? sessionUnitId = null)
    {
        long? senderId = null;
        if (sessionUnitId.HasValue)
        {
            var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId.Value);
            senderId = sessionUnit?.OwnerId;
        }
        return await TreeManager.TriggerAsync(id, sessionUnitId, senderId);
    }
}
