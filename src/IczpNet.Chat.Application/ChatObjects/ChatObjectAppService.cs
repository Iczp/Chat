using Castle.Components.DictionaryAdapter.Xml;
using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.ChatObjectCategories;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.Follows;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.ServiceStates;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Pusher.DeviceIds;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Users;

namespace IczpNet.Chat.ChatObjects;

/// <summary>
/// 聊天对象
/// </summary>
public class ChatObjectAppService(
    IChatObjectRepository repository,
    IChatObjectManager chatObjectManager,
    IChatObjectCategoryManager chatObjectCategoryManager,
    ISessionPermissionChecker sessionPermissionChecker,
    IServiceStateManager serviceStateManager,
    IFavoritedRecorderManager favoritedRecorderManager,
    IFollowManager followManager,
    IDeviceIdResolver deviceIdResolver)
        : CrudTreeChatAppService<
        ChatObject,
        long,
        ChatObjectDto,
        ChatObjectDto,
        ChatObjectGetListInput,
        ChatObjectCreateInput,
        ChatObjectUpdateInput,
        ChatObjectInfo>(repository, chatObjectManager),
    IChatObjectAppService
{
    protected override string GetPolicyName { get; set; } = ChatPermissions.ChatObjectPermission.GetItem;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.ChatObjectPermission.GetList;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.ChatObjectPermission.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.ChatObjectPermission.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.ChatObjectPermission.Delete;
    //protected IChatObjectManager ChatObjectManager { get; }
    protected IChatObjectCategoryManager ChatObjectCategoryManager { get; } = chatObjectCategoryManager;
    protected ISessionPermissionChecker SessionPermissionChecker { get; } = sessionPermissionChecker;
    protected IServiceStateManager ServiceStateManager { get; } = serviceStateManager;
    protected IFavoritedRecorderManager FavoritedRecorderManager { get; } = favoritedRecorderManager;
    protected IFollowManager FollowManager { get; } = followManager;
    protected IDeviceIdResolver DeviceIdResolver { get; } = deviceIdResolver;

    protected override async Task<IQueryable<ChatObject>> CreateFilteredQueryAsync(ChatObjectGetListInput input)
    {
        //Category
        IQueryable<Guid> categoryIdQuery = null;

        if (input.IsImportChildCategory == true && input.CategoryIdList.IsAny())
        {
            categoryIdQuery = (await ChatObjectCategoryManager.QueryCurrentAndAllChildsAsync(input.CategoryIdList)).Select(x => x.Id);
        }
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.ChatObjectTypeId.IsNullOrWhiteSpace(), x => x.ChatObjectTypeId == input.ChatObjectTypeId)
            .WhereIf(input.ObjectTypes.IsAny(), x => input.ObjectTypes.Contains((ChatObjectTypeEnums)x.ObjectType))
            .WhereIf(input.ObjectType.HasValue, x => x.ObjectType == input.ObjectType)
            .WhereIf(input.IsEnabled.HasValue, x => x.IsEnabled == input.IsEnabled)
            .WhereIf(input.IsDefault.HasValue, x => x.IsDefault == input.IsDefault)
            .WhereIf(input.IsPublic.HasValue, x => x.IsPublic == input.IsPublic)
            .WhereIf(input.IsStatic.HasValue, x => x.IsStatic == input.IsStatic)
            //CategoryId
            .WhereIf(input.IsImportChildCategory == false && input.CategoryIdList.IsAny(), x => x.ChatObjectCategoryUnitList.Any(d => input.CategoryIdList.Contains(d.CategoryId)))
            .WhereIf(input.IsImportChildCategory == true && input.CategoryIdList.IsAny(), x => x.ChatObjectCategoryUnitList.Any(d => categoryIdQuery.Contains(d.CategoryId)))
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword) || x.Code.Contains(input.Keyword) || x.NameSpellingAbbreviation.Contains(input.Keyword))
            ;
    }

    /// <summary>
    /// 获取一条数据[code]
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<ChatObjectDto> GetByCodeAsync(string code)
    {
        Assert.If(code.IsNullOrWhiteSpace(), $"[code] IsNullOrWhiteSpace.");

        await CheckGetPolicyAsync();

        var entity = Assert.NotNull(await ChatObjectManager.FindByCodeAsync(code), $"Entity no such by [code]:{code}");

        return await MapToGetOutputDtoAsync(entity);
    }

    /// <summary>
    /// 获取用户的聊天对象
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<PagedResultDto<ChatObjectDto>> GetListByUserIdAsync(Guid userId, GetListInput input)
    {
        await CheckGetListPolicyAsync();

        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.AppUserId == userId);

        return await GetPagedListAsync<ChatObject, ChatObjectDto>(query, input);
    }

    /// <summary>
    /// 获取当前用户的聊天对象
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    public virtual Task<PagedResultDto<ChatObjectDto>> GetListByCurrentUserAsync(GetListInput input)
    {
        return GetListByUserIdAsync(Assert.NotNull(CurrentUser.GetId(), "未登录"), input);
    }

    [RemoteService(false)]
    public override Task DeleteAsync(long id)
    {
        return base.DeleteAsync(id);
    }

    protected override Task MapToEntityAsync(ChatObjectUpdateInput updateInput, ChatObject entity)
    {
        //owner.SetName(updateInput.Title);
        return base.MapToEntityAsync(updateInput, entity);
    }

    protected override ChatObject MapToEntity(ChatObjectCreateInput createInput)
    {
        var entity = base.MapToEntity(createInput);
        //owner.SetName(createInput.Title);
        return entity;
    }


    /// <summary>
    /// 创建聊天对象[掌柜]
    /// </summary>
    /// <param name="name"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<ChatObjectDto> CreateShopKeeperAsync(string name, string code)
    {
        var shopKeeper = await ChatObjectManager.CreateShopKeeperAsync(name, code);

        return ObjectMapper.Map<ChatObject, ChatObjectDto>(shopKeeper);
    }

    /// <summary>
    /// 创建聊天对象[店小二]
    /// </summary>
    /// <param name="shopKeeperId"></param>
    /// <param name="name"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<ChatObjectDto> CreateShopWaiterAsync(long shopKeeperId, string name, string code)
    {
        var shopWaiter = await ChatObjectManager.CreateShopWaiterAsync(shopKeeperId, name, code);

        return ObjectMapper.Map<ChatObject, ChatObjectDto>(shopWaiter);
    }

    /// <summary>
    /// 创建聊天对象[机器人]
    /// </summary>
    /// <param name="name"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<ChatObjectDto> CreateRobotAsync(string name, string code)
    {
        var entity = await ChatObjectManager.CreateRobotAsync(name, code);

        return ObjectMapper.Map<ChatObject, ChatObjectDto>(entity);
    }

    /// <summary>
    /// 创建聊天对象[聊天广场]
    /// </summary>
    /// <param name="name"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<ChatObjectDto> CreateSquareAsync(string name, string code)
    {
        var entity = await ChatObjectManager.CreateSquareAsync(name, code);

        return ObjectMapper.Map<ChatObject, ChatObjectDto>(entity);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public override async Task<ChatObjectDto> UpdateAsync(long id, ChatObjectUpdateInput input)
    {
        var entity = await ChatObjectManager.GetAsync(id);
        input.ParentId = entity.ParentId;
        return await base.UpdateAsync(id, input);
    }

    /// <summary>
    /// 修改名称
    /// </summary>
    /// <param name="id">主建Id</param>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ChatObjectDto> UpdateNameAsync(long id, string name)
    {
        var entity = await ChatObjectManager.UpdateNameAsync(id, name);
        return await MapToGetOutputDtoAsync(entity);
    }

    /// <summary>
    /// 更新头像
    /// </summary>
    /// <param name="id">主建Id</param>
    /// <param name="thumbnail"></param>
    /// <param name="portrait"></param>
    /// <returns></returns>
    [HttpPost]
    //[RemoteService(false)]
    public Task<ChatObjectDto> UpdatePortraitAsync(long id, string thumbnail, string portrait)
    {
        return UpdateEntityAsync(id, entity => entity.SetPortrait(thumbnail, portrait));
    }

    protected virtual async Task<ChatObjectDto> UpdateEntityAsync(long id, Action<ChatObject> action)
    {
        var entity = await ChatObjectManager.GetAsync(id);

        action?.Invoke(entity);

        await ChatObjectManager.UpdateAsync(entity, entity.ParentId, isUnique: true);

        return await MapToGetOutputDtoAsync(entity);
    }


    /// <summary>
    /// 设置验证方式(好友|群|广场等)
    /// </summary>
    /// <param name="id">主建Id</param>
    /// <param name="verificationMethod">验证方式</param>
    /// <returns></returns>
    [HttpPost]
    public virtual Task<ChatObjectDto> SetVerificationMethodAsync(long id, VerificationMethods verificationMethod)
    {
        return UpdateEntityAsync(id, entity => entity.SetVerificationMethod(verificationMethod));
    }

    protected virtual async Task<ChatObjectDetailDto> MapToEntityDetailAsync(ChatObject entity)
    {
        await Task.Yield();
        return ObjectMapper.Map<ChatObject, ChatObjectDetailDto>(entity);
    }

    /// <summary>
    /// 获取详情
    /// </summary>
    /// <param name="id">主建Id</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ChatObjectDetailDto> GetDetailAsync(long id)
    {
        var entity = await ChatObjectManager.GetAsync(id);

        return await MapToEntityDetailAsync(entity);
    }

    /// <summary>
    /// 获取服务状态(在线状态)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<List<ServiceStatusCacheItem>> GetServiceStatusAsync(long id)
    {
        return await ServiceStateManager.GetAsync(id);
    }

    /// <summary>
    /// <inheritdoc/> 设置服务状态(在线状态)
    /// </summary>
    /// <param name="id"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<List<ServiceStatusCacheItem>> SetServiceStatusAsync(long id, ServiceStatus status)
    {
        var deviceId = await DeviceIdResolver.GetDeviceIdAsync();

        if (deviceId == null)
        {
            //...
        }

        return await ServiceStateManager.SetAsync(id, deviceId, status);
    }

    public async Task<ChatObjectProfileDto> GetProfileAsync(long id)
    {

        return new ChatObjectProfileDto()
        {
            Owner = await GetAsync(id),
            FavoritedCount = await FavoritedRecorderManager.GetCountByOwnerIdAsync(id),
            FllowingCount = await FollowManager.GetFollowingCountAsync(id),
            FllowerCount = await FollowManager.GetFollowerCountAsync(id),
        };
        //return GetProfileInternalAsync(id);
    }
}
