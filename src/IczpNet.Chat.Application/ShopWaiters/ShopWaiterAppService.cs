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
using Volo.Abp;
using Volo.Abp.Identity;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Uow;
using Volo.Abp.BackgroundJobs;

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
            .Where(x => x.ObjectType == ChatObjectTypeEnums.ShopWaiter)
            .Where(x => x.ParentId == input.ShopKeeperId)
            ;
        if (input.IsContainsShopKeeper)
        {
            var query1 = (await Repository.GetQueryableAsync())
           .Where(x => x.ObjectType == ChatObjectTypeEnums.ShopKeeper)
           .Where(x => x.Id == input.ShopKeeperId);
            query = query.Concat(query1);
        }
        query = query.WhereIf(!input.Keyword.IsEmpty(), new KeywordChatObjectSpecification(input.Keyword));

        return await GetPagedListAsync<ChatObject, ShopWaiterDto>(query, input, x => x.OrderBy(x => x.Depth).ThenBy(d => d.Name));

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

        await LazyServiceProvider.LazyGetRequiredService<IBackgroundJobManager>()
            .EnqueueAsync(new ShopWaiterSessionSyncJobArgs { ShopKeeperId = input.ShopKeeperId, WaiterId = entity.Id });

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
    [HttpPost]
    public virtual async Task<ShopWaiterDto> BingUserAsync(long id, Guid userId)
    {
        var distributedLock = LazyServiceProvider.LazyGetRequiredService<IAbpDistributedLock>();
        if (distributedLock is NullAbpDistributedLock)
        {
            throw new InvalidOperationException("Binding a shop waiter requires a distributed lock provider.");
        }
        await using var handle = await distributedLock.TryAcquireAsync($"Chat:ShopWaiter:Bind:{id}", TimeSpan.FromSeconds(10));
        if (handle == null)
        {
            throw new UserFriendlyException("店小二绑定正在处理，请重试。");
        }
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var waiter = await Repository.GetAsync(id);
        if (waiter.ObjectType != ChatObjectTypeEnums.ShopWaiter || !waiter.ParentId.HasValue)
        {
            throw new UserFriendlyException("聊天对象不是店小二。");
        }

        var shopKeeper = await Repository.GetAsync(waiter.ParentId.Value);
        if (shopKeeper.ObjectType != ChatObjectTypeEnums.ShopKeeper)
        {
            throw new UserFriendlyException("店小二所属的聊天对象不是店长。");
        }
        await CheckPolicyForUserAsync(shopKeeper.Id);

        var users = LazyServiceProvider.LazyGetRequiredService<IdentityUserManager>();
        if (await users.FindByIdAsync(userId.ToString()) == null)
        {
            throw new UserFriendlyException("用户不存在。");
        }
        if (waiter.AppUserId.HasValue && waiter.AppUserId != userId)
        {
            throw new UserFriendlyException("店小二已绑定其他用户。");
        }
        if (waiter.AppUserId == userId)
        {
            var existing = await MapToDtoAsync(waiter);
            await uow.CompleteAsync();
            return existing;
        }

        var result = await MapToDtoAsync(await ChatObjectManager.BingAppUserIdAsync(id, userId));
        await uow.CompleteAsync();
        return result;
    }
}
