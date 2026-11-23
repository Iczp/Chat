using IczpNet.AbpTrees;
using IczpNet.Chat.Menus.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Menus;

public interface IMenuAppService :
    ICrudAppService<
        MenuDto,
        MenuDto,
        Guid,
        MenuGetListInput,
        MenuCreateInput,
        MenuUpdateInput>
    ,
    ITreeAppService<
        MenuDto,
        MenuDto,
        Guid,
        MenuGetListInput,
        MenuCreateInput,
        MenuUpdateInput, MenuInfo>
{
    /// <summary>
    /// 获取指定聊天对象(公众号/订阅号)的菜单列表
    /// </summary>
    Task<List<MenuDto>> GetListByOwnerIdAsync(long ownerId);

    /// <summary>
    /// 获取指定公众号/订阅号已发布的正式菜单（带版本比较与缓存支持）
    /// </summary>
    /// <param name="chatObjectId">公众号/订阅号 ChatObjectId</param>
    /// <param name="version">客户端本地已持有的菜单版本号（可选）</param>
    /// <returns>发布快照 DTO。若版本一致，IsModified 为 false 且 Items 为空</returns>
    Task<PublishedMenuOutput> GetPublishedAsync(long chatObjectId, string version = null);

    /// <summary>
    /// 立即发布当前公众号/订阅号的菜单草稿为线上正式版本
    /// </summary>
    /// <param name="chatObjectId">公众号/订阅号 ChatObjectId</param>
    /// <returns>最新发布快照 DTO</returns>
    Task<PublishedMenuOutput> PublishAsync(long chatObjectId);

    /// <summary>
    /// 菜单触发器(调用后台作业)
    /// </summary>
    /// <param name="id">菜单Id</param>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <returns></returns>
    Task<string> TriggerAsync(Guid id, Guid? sessionUnitId = null);

    /// <summary>
    /// 客户端菜单点击事件上报（用于 View 网页跳转、LocalPage、SwitchTab 等非后台事件动作的 PV/UV 统计）
    /// </summary>
    /// <param name="id">菜单Id</param>
    /// <param name="input">上报参数</param>
    Task ReportClickAsync(Guid id, ReportMenuClickInput input = null);

    /// <summary>
    /// 获取指定公众号/订阅号所有菜单的点击统计数据看板
    /// </summary>
    /// <param name="chatObjectId">公众号/订阅号 ChatObjectId</param>
    Task<OfficialMenuStatisticOutput> GetStatisticsAsync(long chatObjectId);
}
