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
    /// 菜单触发器(调用后台作业)
    /// </summary>
    /// <param name="id">菜单Id</param>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <returns></returns>
    Task<string> TriggerAsync(Guid id, Guid? sessionUnitId = null);
}
