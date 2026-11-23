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
    IChatObjectRepository chatObjectRepository,
    IMenuStatisticManager menuStatisticManager)
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
    protected IMenuStatisticManager MenuStatisticManager { get; } = menuStatisticManager;
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

        var dto = await base.CreateAsync(input);

        if (input.IsPublishNow)
        {
            await TreeManager.PublishAsync(input.OwnerId);
        }

        return dto;
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
        var entity = await Repository.GetAsync(id);

        if (input.ParentId.HasValue)
        {
            var parent = await Repository.GetAsync(input.ParentId.Value);

            Assert.If(parent.OwnerId != entity.OwnerId, $"Parent owner is different,ParentId:{input.ParentId}");
        }

        var dto = await base.UpdateAsync(id, input);

        if (input.IsPublishNow)
        {
            await TreeManager.PublishAsync(entity.OwnerId);
        }

        return dto;
    }

    /// <summary>
    /// 获取指定公众号/订阅号已发布的正式菜单（带版本比较与缓存支持）
    /// </summary>
    /// <param name="chatObjectId">公众号/订阅号 ChatObjectId</param>
    /// <param name="version">客户端本地已持有的菜单版本号（可选）</param>
    /// <returns>发布快照 DTO。若版本一致，IsModified 为 false 且 Items 为空</returns>
    [HttpGet]
    [Route("api/chat/menu/published/{chatObjectId}")]
    public virtual async Task<PublishedMenuOutput> GetPublishedAsync(long chatObjectId, [FromQuery] string version = null)
    {
        var snapshot = await TreeManager.GetPublishedAsync(chatObjectId);
        if (snapshot == null)
        {
            return new PublishedMenuOutput
            {
                ChatObjectId = chatObjectId,
                Version = null,
                IsModified = false,
                PublishTime = null,
                Items = []
            };
        }

        // 版本比对：若客户端持有的版本号与当前发布版本一致，则无需重复下发整树
        var isModified = string.IsNullOrWhiteSpace(version) || !string.Equals(version.Trim(), snapshot.Version, StringComparison.OrdinalIgnoreCase);

        return new PublishedMenuOutput
        {
            ChatObjectId = chatObjectId,
            Version = snapshot.Version,
            IsModified = isModified,
            PublishTime = snapshot.PublishTime,
            Items = isModified ? (snapshot.Items ?? []) : []
        };
    }

    /// <summary>
    /// 立即发布当前公众号/订阅号的菜单草稿为线上正式版本
    /// </summary>
    /// <param name="chatObjectId">公众号/订阅号 ChatObjectId</param>
    /// <returns>最新发布快照 DTO</returns>
    [HttpPost]
    [Route("api/chat/menu/publish/{chatObjectId}")]
    public virtual async Task<PublishedMenuOutput> PublishAsync(long chatObjectId)
    {
        var snapshot = await TreeManager.PublishAsync(chatObjectId);
        return new PublishedMenuOutput
        {
            ChatObjectId = chatObjectId,
            Version = snapshot.Version,
            IsModified = true,
            PublishTime = snapshot.PublishTime,
            Items = snapshot.Items ?? []
        };
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

        var menu = await Repository.FindAsync(id);
        if (menu != null)
        {
            await MenuStatisticManager.RecordClickAsync(id, menu.OwnerId, senderId);
        }

        return await TreeManager.TriggerAsync(id, sessionUnitId, senderId);
    }

    /// <summary>
    /// 客户端菜单点击事件上报（用于 View 网页跳转、LocalPage 等非后台触发事件的 PV/UV 埋点）
    /// </summary>
    /// <param name="id">菜单Id</param>
    /// <param name="input">上报参数</param>
    [HttpPost]
    [Route("api/chat/menu/report-click/{id}")]
    public virtual async Task ReportClickAsync(Guid id, ReportMenuClickInput input = null)
    {
        long? senderId = input?.SenderId;
        if (!senderId.HasValue && input?.SessionUnitId.HasValue == true)
        {
            var sessionUnit = await SessionUnitManager.GetAsync(input.SessionUnitId.Value);
            senderId = sessionUnit?.OwnerId;
        }

        var menu = await Repository.FindAsync(id);
        if (menu != null)
        {
            await MenuStatisticManager.RecordClickAsync(id, menu.OwnerId, senderId);
        }
    }

    /// <summary>
    /// 获取指定公众号/订阅号所有菜单的点击统计数据看板
    /// </summary>
    /// <param name="chatObjectId">公众号/订阅号 ChatObjectId</param>
    [HttpGet]
    [Route("api/chat/menu/statistics/{chatObjectId}")]
    public virtual async Task<OfficialMenuStatisticOutput> GetStatisticsAsync(long chatObjectId)
    {
        var entities = await Repository.GetListAsync(x => x.OwnerId == chatObjectId);
        if (entities.Count == 0)
        {
            return new OfficialMenuStatisticOutput
            {
                ChatObjectId = chatObjectId,
                TotalClicks = 0,
                TodayClicks = 0,
                Items = []
            };
        }

        var menuIds = entities.Select(x => x.Id).ToList();
        var statsMap = await MenuStatisticManager.GetBatchMenuStatsAsync(menuIds);

        var items = new List<MenuItemStatisticDto>();
        long sumTotal = 0;
        long sumToday = 0;

        foreach (var m in entities.OrderByDescending(x => x.Sorting))
        {
            statsMap.TryGetValue(m.Id, out var stat);
            sumTotal += stat.TotalClicks;
            sumToday += stat.TodayClicks;

            items.Add(new MenuItemStatisticDto
            {
                MenuId = m.Id,
                Name = m.Name,
                Event = m.Event.ToString(),
                Code = m.Code,
                TotalClicks = stat.TotalClicks,
                UniqueUsers = stat.UniqueUsers,
                TodayClicks = stat.TodayClicks,
                TodayUsers = stat.TodayUsers
            });
        }

        return new OfficialMenuStatisticOutput
        {
            ChatObjectId = chatObjectId,
            TotalClicks = sumTotal,
            TodayClicks = sumToday,
            Items = items
        };
    }
}
