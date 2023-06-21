using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.ShopKeepers.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ShopKeepers;

/// <summary>
/// 掌柜
/// </summary>
public class ShopKeeperAppService : ChatAppService, IShopKeeperAppService
{
    protected IChatObjectRepository Repository { get; }
    protected IShopKeeperManager ShopKeeperManager { get; }

    public ShopKeeperAppService(IChatObjectRepository repository, 
        IShopKeeperManager shopKeeperManager)
    {
        Repository = repository;
        ShopKeeperManager = shopKeeperManager;
    }

    /// <summary>
    /// 获取掌柜列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<PagedResultDto<ShopKeeperDto>> GetListAsync(ShopKeeperGetListInput input)
    {
        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.ObjectType == ChatObjectTypeEnums.ShopKeeper);

        return await query.ToPagedListAsync<ChatObject, ShopKeeperDto>(AsyncExecuter, ObjectMapper, input, x => x.OrderBy(d => d.Name));
    }

    /// <summary>
    /// 创建掌柜
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    [HttpPost]
    public Task<PagedResultDto<ShopKeeperDto>> CreateAsync(ShopKeeperCreateInput input)
    {
        throw new System.NotImplementedException();
    }
}
