using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionUnits;
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
        var query1 = (await Repository.GetQueryableAsync())
           .Where(x => x.ObjectType == ChatObjectTypeEnums.ShopKeeper)
           .Where(x => x.Id == input.ShopKeeperId);

        var query2 = (await Repository.GetQueryableAsync())
            .Where(x => x.ObjectType == ChatObjectTypeEnums.ShopWaiter)
            .Where(x => x.ParentId == input.ShopKeeperId)
            ;
        var query = query1.Concat(query2)
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordChatObjectSpecification(input.Keyword))
            ;

        return await GetPagedListAsync<ChatObject, ShopWaiterDto>(query, input, x => x.OrderBy(d => d.Name));

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
    /// <param name="id">主建Id</param>
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
    /// <param name="id">主建Id</param>
    /// <param name="userId">用户Id</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpPost]
    public virtual Task<ShopWaiterDto> BingUserAsync(long id, Guid userId)
    {
        throw new NotImplementedException();
    }
}
