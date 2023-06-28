using IczpNet.AbpCommons;
using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Localization;
using IczpNet.Chat.SessionSections.SessionRequests.Dtos;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.BaseAppServices;


public abstract class CrudChatAppService<
    TEntity,
    TGetOutputDto,
    TGetListOutputDto,
    TKey,
    TGetListInput>
    :
    CrudAbpCommonsAppService<
        TEntity,
        TGetOutputDto,
        TGetListOutputDto,
        TKey,
        TGetListInput,
        BaseInput,
        BaseInput>
    ,
    ICrudChatAppService<
        TGetOutputDto,
        TGetListOutputDto,
        TKey,
        TGetListInput,
        BaseInput,
        BaseInput>

    where TEntity : class, IEntity<TKey>
    where TGetOutputDto : IEntityDto<TKey>
    where TGetListOutputDto : IEntityDto<TKey>
{
    protected CrudChatAppService(IRepository<TEntity, TKey> repository) : base(repository)
    {
    }

    [RemoteService(false)]
    public override Task<TGetOutputDto> CreateAsync(BaseInput input) => throw new NotImplementedException();

    [RemoteService(false)]
    public override Task<TGetOutputDto> UpdateAsync(TKey id, BaseInput input) => throw new NotImplementedException();

    [RemoteService(false)]
    public override Task DeleteAsync(TKey id) => throw new NotImplementedException();

    [RemoteService(false)]
    public override Task DeleteManyAsync(List<TKey> idList) => throw new NotImplementedException();

}

[ApiExplorerSettings(GroupName = ChatRemoteServiceConsts.ModuleName)]
[Authorize]

public abstract class CrudChatAppService<
    TEntity,
    TGetOutputDto,
    TGetListOutputDto,
    TKey,
    TGetListInput,
    TCreateInput,
    TUpdateInput>
    :
    CrudAbpCommonsAppService<
        TEntity,
        TGetOutputDto,
        TGetListOutputDto,
        TKey,
        TGetListInput,
        TCreateInput,
        TUpdateInput>
    ,
    ICrudChatAppService<
        TGetOutputDto,
        TGetListOutputDto,
        TKey,
        TGetListInput,
        TCreateInput,
        TUpdateInput>

    where TEntity : class, IEntity<TKey>
    where TGetOutputDto : IEntityDto<TKey>
    where TGetListOutputDto : IEntityDto<TKey>
    //where TCreateInput : IName
    //where TUpdateInput : IName
{

    protected ICurrentChatObject CurrentChatObject => LazyServiceProvider.LazyGetRequiredService<ICurrentChatObject>();
    protected IChatObjectManager ChatObjectManager => LazyServiceProvider.LazyGetRequiredService<IChatObjectManager>();
    protected ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();
    protected CrudChatAppService(IRepository<TEntity, TKey> repository) : base(repository)
    {
        LocalizationResource = typeof(ChatResource);
        ObjectMapperContext = typeof(ChatApplicationModule);
    }

    protected virtual async Task<bool> IsAnyCurrentUserAsync(IEnumerable<long?> ownerIdList)
    {
        var appUserId = CurrentUser.Id;

        if (appUserId == null || ownerIdList.Where(x => x.HasValue).Any())
        {
            return false;
        }

        var chatObjectIdList = await ChatObjectManager.GetIdListByUserId(appUserId.Value);

        return chatObjectIdList.Any(x => ownerIdList.Contains(x));
    }

    protected virtual Task<bool> IsCurrentUserAsync(long? ownerId)
    {
        return IsAnyCurrentUserAsync(new[] { ownerId });
    }

    protected virtual async Task CheckPolicyForUserAsync(IEnumerable<long?> ownerIdList, Func<Task> func)
    {
        if (await IsAnyCurrentUserAsync(ownerIdList))
        {
            return;
        }

        await func();
    }

    protected virtual Task CheckPolicyForUserAsync(long? ownerId, Func<Task> func)
    {
        return CheckPolicyForUserAsync(new[] { ownerId }, func);
    }

    

    protected virtual bool HasNameProperty(TEntity entity)
    {
        return entity.GetType().GetProperty(nameof(IName.Name)) != null;
    }

    protected virtual void TryToSetName(TEntity entity, string name)
    {
        if (entity is IName && HasNameProperty(entity))
        {
            if (!name.IsNullOrWhiteSpace())
            {
                return;
            }

            var methodInfo = entity.GetType().GetMethod(nameof(IName.Name));

            if (methodInfo == null || !methodInfo.IsPublic)
            {
                return;
            }

            methodInfo.Invoke(entity, new object[] { name });
        }
    }

    #region  CheckPolicyAsync
    protected virtual async Task CheckPolicyAsync(string policyName, long ownerId)
    {

        var owner = await ChatObjectManager.GetAsync(ownerId);

        await CheckPolicyAsync(policyName, owner);
    }

    protected virtual async Task CheckPolicyAsync(string policyName, ChatObject owner)
    {

        if (string.IsNullOrEmpty(policyName))
        {
            return;
        }

        await AuthorizationService.CheckAsync(owner, policyName);
    }

    protected virtual async Task CheckPolicyAsync(string policyName, Guid sessionUnitId)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

        await CheckPolicyAsync(policyName, sessionUnit);
    }

    protected virtual async Task CheckPolicyAsync(string policyName, SessionUnit sessionUnit)
    {
        if (string.IsNullOrEmpty(policyName))
        {
            return;
        }

        await AuthorizationService.CheckAsync(sessionUnit, policyName);
    }

    protected virtual async Task<SessionUnit> GetAndCheckPolicyAsync(string policyName, Guid sessionUnitId, bool checkIsKilled = true)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

        Assert.If(checkIsKilled && sessionUnit.Setting.IsKilled, "已经删除的会话单元!");

        await CheckPolicyAsync(policyName, sessionUnit);

        return sessionUnit;
    }

    protected virtual async Task<ChatObject> GetAndCheckPolicyAsync(string policyName, long chatObjectId, bool checkIsKilled = true)
    {
        var chatObject = await ChatObjectManager.GetAsync(chatObjectId);

        Assert.If(checkIsKilled && chatObject.IsEnabled, "被禁用的聊天对象");

        await CheckPolicyAsync(policyName, chatObject);

        return chatObject;
    }

    #endregion

    #region 重写备注

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public override Task<PagedResultDto<TGetListOutputDto>> GetListAsync(TGetListInput input)
    {
        return base.GetListAsync(input);
    }

    /// <summary>
    /// 获取多条数据
    /// </summary>
    /// <param name="idList">主键Id[多个]</param>
    /// <returns></returns>
    [HttpGet]
    public override Task<List<TGetOutputDto>> GetManyAsync(List<TKey> idList)
    {
        return base.GetManyAsync(idList);
    }

    /// <summary>
    /// 获取一条记录 Get
    /// </summary>
    /// <param name="id">主键Id</param>
    /// <returns></returns>
    [HttpGet]
    public override Task<TGetOutputDto> GetAsync(TKey id)
    {
        //CurrentUser.PhoneNumber = id;
        return base.GetAsync(id);
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public override Task<TGetOutputDto> CreateAsync(TCreateInput input)
    {
        return base.CreateAsync(input);
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="id">主键Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public override Task<TGetOutputDto> UpdateAsync(TKey id, TUpdateInput input)
    {
        return base.UpdateAsync(id, input);
    }

    /// <summary>
    /// 删除一条数据
    /// </summary>
    /// <param name="id">主键Id</param>
    /// <returns></returns>
    [HttpPost]
    public override Task DeleteAsync(TKey id)
    {
        return base.DeleteAsync(id);
    }

    /// <summary>
    /// 删除多条数据
    /// </summary>
    /// <param name="idList">主键Id[多个]</param>
    /// <returns></returns>
    [HttpPost]
    public override Task DeleteManyAsync(List<TKey> idList)
    {
        return base.DeleteManyAsync(idList);
    }

    #endregion
}
