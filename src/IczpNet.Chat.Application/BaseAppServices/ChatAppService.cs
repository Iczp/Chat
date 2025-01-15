using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Localization;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Auditing;
using Volo.Abp.Users;

namespace IczpNet.Chat.BaseAppServices;

[ApiExplorerSettings(GroupName = ChatRemoteServiceConsts.ModuleName)]
[Authorize]
public abstract class ChatAppService : AbpCommonsAppService
{
    protected virtual string GetListPolicyName { get; set; }
    protected virtual string GetPolicyName { get; set; }
    protected virtual string CreatePolicyName { get; set; }
    protected virtual string UpdatePolicyName { get; set; }
    protected virtual string DeletePolicyName { get; set; }

    protected ICurrentChatObject CurrentChatObject => LazyServiceProvider.LazyGetRequiredService<ICurrentChatObject>();
    protected ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();
    protected IChatObjectManager ChatObjectManager => LazyServiceProvider.LazyGetRequiredService<IChatObjectManager>();
    protected ChatAppService()
    {
        LocalizationResource = typeof(ChatResource);
        ObjectMapperContext = typeof(ChatApplicationModule);
    }

    protected virtual void TryToSetLastModificationTime<T>(T entity)
    {
        if (entity is IHasModificationTime)
        {
            var propertyInfo = entity.GetType().GetProperty(nameof(IHasModificationTime.LastModificationTime));

            if (propertyInfo == null || propertyInfo.GetSetMethod(true) == null)
            {
                return;
            }

            propertyInfo.SetValue(entity, Clock.Now);
        }
    }

    //protected virtual async Task<PagedResultDto<TOuputDto>> GetPagedListAsync<T, TOuputDto>(
    //    IQueryable<T> query,
    //    PagedAndSortedResultRequestDto input,
    //    Func<IQueryable<T>, IQueryable<T>> queryableAction = null,
    //    Func<List<T>, Task<List<T>>> entityAction = null)
    //{
    //    return await query.ToPagedListAsync<T, TOuputDto>(AsyncExecuter, ObjectMapper, input, queryableAction, entityAction);
    //}

    //protected virtual async Task<PagedResultDto<T>> GetPagedListAsync<T>(
    //    IQueryable<T> query,
    //    PagedAndSortedResultRequestDto input,
    //    Func<IQueryable<T>, IQueryable<T>> queryableAction = null,
    //    Func<List<T>, Task<List<T>>> entityAction = null)
    //{
    //    return await query.ToPagedListAsync<T, T>(AsyncExecuter, ObjectMapper, input, queryableAction, entityAction);
    //}

    #region CheckPolicyForUserAsync
    protected virtual async Task<bool> IsAnyCurrentUserAsync(IEnumerable<long?> ownerIdList)
    {
        var appUserId = CurrentUser.Id;

        if (appUserId == null || !ownerIdList.Where(x => x.HasValue).Any())
        {
            return false;
        }

        var chatObjectIdList = await ChatObjectManager.GetIdListByUserIdAsync(appUserId.Value);

        return chatObjectIdList.Any(x => ownerIdList.Contains(x));
    }

    protected virtual Task<bool> IsCurrentUserAsync(long? ownerId)
    {
        return IsAnyCurrentUserAsync(new[] { ownerId });
    }

    protected virtual async Task CheckPolicyForUserAsync(IEnumerable<long?> ownerIdList, Func<Task> func = null)
    {
        //if (await IsAnyCurrentUserAsync(ownerIdList))
        //{
        //    return;
        //}

        Assert.If(!CurrentUser.Id.HasValue, $"请先登录");

        var currentUserId = CurrentUser.GetId();

        var chatObjectIdList = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);

        //Assert.If(!await IsAnyCurrentUserAsync(ownerIdList), $"Fail ownerIds [{ownerIdList.JoinAsString(",")}], current user's chatObjectIdList:[{chatObjectIdList.JoinAsString(",")}]");

        await func?.Invoke();
    }

    protected virtual Task CheckPolicyForUserAsync(long? ownerId, Func<Task> func = null)
    {
        return CheckPolicyForUserAsync([ownerId], func);
    }

    protected virtual Task CheckPolicyForUserAsync(IEnumerable<long> ownerIdList, Func<Task> func = null)
    {
        return CheckPolicyForUserAsync(ownerIdList.Select(x => (long?)x), func);
    }
    #endregion

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

        await CheckPolicyForUserAsync(sessionUnit.OwnerId, () => CheckPolicyAsync(policyName));

        return sessionUnit;
    }

    protected virtual async Task<ChatObject> GetAndCheckPolicyAsync(string policyName, long chatObjectId, bool checkIsKilled = true)
    {
        var chatObject = await ChatObjectManager.GetAsync(chatObjectId);

        Assert.If(checkIsKilled && chatObject.IsEnabled, "被禁用的聊天对象");

        await CheckPolicyForUserAsync(chatObjectId, () => CheckPolicyAsync(policyName));

        return chatObject;
    }

    #endregion

    #region 重写备注


    #endregion
}
