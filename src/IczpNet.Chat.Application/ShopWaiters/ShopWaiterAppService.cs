using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.ShopWaiters.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ShopWaiters;

/// <summary>
/// 店小二
/// </summary>
public class ShopWaiterAppService : ChatAppService, IShopWaiterAppService
{
    protected IChatObjectRepository Repository { get; }
    protected IShopWaiterManager ShopWaiterManager { get; }

    public ShopWaiterAppService(IChatObjectRepository repository,
        IShopWaiterManager shopKeeperManager)
    {
        Repository = repository;
        ShopWaiterManager = shopKeeperManager;
    }

    protected virtual Task<ShopWaiterDto> MapToDtoAsync(ChatObject entity)
    {
        return Task.FromResult(MapToDto(entity));
    }

    protected virtual ShopWaiterDto MapToDto(ChatObject entity)
    {
        return ObjectMapper.Map<ChatObject, ShopWaiterDto>(entity);
    }

    /// <summary>
    /// 获取店小二（子账号）列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<PagedResultDto<ShopWaiterDto>> GetListAsync(ShopWaiterGetListInput input)
    {
        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.ParentId == input.ShopKeeperId)
            .Where(x => x.ObjectType == ChatObjectTypeEnums.ShopWaiter);

        return await query.ToPagedListAsync<ChatObject, ShopWaiterDto>(AsyncExecuter, ObjectMapper, input, x => x.OrderBy(d => d.Name));

    }

    /// <summary>
    /// 添加店小二
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<ShopWaiterDto> CreateAsync(ShopWaiterCreateInput input)
    {
        var entity = await ShopWaiterManager.CreateAsync(input.ShopKeeperId, input.Name);

        return await MapToDtoAsync(entity);
    }

    /// <summary>
    /// 修改店小二
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<ShopWaiterDto> UpdateAsync(long id, ShopWaiterUpdateInput input)
    {
        var entity = await ShopWaiterManager.UpdateAsync(id, input.Name);

        return await MapToDtoAsync(entity);
    }

    /// <summary>
    /// 店小二绑定用户
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpPost]
    public virtual Task<ShopWaiterDto> BingUserAsync(long id, Guid userId)
    {
        throw new NotImplementedException();
    }
}
