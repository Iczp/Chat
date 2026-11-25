using IczpNet.AbpCommons;
using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Localization;
using IczpNet.Chat.SessionPermissions;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Users;

namespace IczpNet.Chat.BaseAppServices;

internal static class SessionUnitPolicyHelper
{
    public static bool IsBuiltInAdminRoleName(string roleName)
    {
        var normalized = roleName?.Trim();
        return string.Equals(normalized, "管理员", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "Admin", StringComparison.OrdinalIgnoreCase);
    }

    public static async Task<SessionUnit> LoadDetailedSessionUnitAsync(IAbpLazyServiceProvider lazyServiceProvider, Guid sessionUnitId)
    {
        var sessionUnitRepo = lazyServiceProvider.LazyGetRequiredService<ISessionUnitRepository>();
        var asyncExecuter = lazyServiceProvider.LazyGetRequiredService<IAsyncQueryableExecuter>();
        var query = (await sessionUnitRepo.GetQueryableAsync())
            .Include(x => x.Setting)
            .Include(x => x.GrantList)
            .Include(x => x.SessionUnitRoleList)
                .ThenInclude(x => x.SessionRole)
                    .ThenInclude(x => x.GrantList);
        var entity = await asyncExecuter.FirstOrDefaultAsync(query, x => x.Id == sessionUnitId);
        return Assert.NotNull(entity, $"SessionUnit not found: {sessionUnitId}");
    }

    public static async Task<SessionUnit> ResolveAndCheckSessionUnitPolicyAsync(
        IAbpLazyServiceProvider lazyServiceProvider,
        ICurrentUser currentUser,
        IChatObjectManager chatObjectManager,
        Guid sessionId,
        Guid? sessionUnitId,
        string policyName)
    {
        var sessionRepo = lazyServiceProvider.LazyGetRequiredService<IRepository<Session, Guid>>();
        var asyncExecuter = lazyServiceProvider.LazyGetRequiredService<IAsyncQueryableExecuter>();
        var sessionEnabled = await asyncExecuter.AnyAsync(
            (await sessionRepo.GetQueryableAsync()).Where(x => x.Id == sessionId && x.IsEnabled));
        Assert.If(!sessionEnabled, "群会话不存在或已解散");

        SessionUnit unit;
        if (sessionUnitId.HasValue)
        {
            unit = await LoadDetailedSessionUnitAsync(lazyServiceProvider, sessionUnitId.Value);
            Assert.If(unit.SessionId != sessionId, "操作成员身份不属于目标群会话");
            Assert.If(unit.Setting == null || !unit.Setting.IsEnabled || unit.Setting.IsKilled, "当前成员身份已失效");
            if (currentUser.Id.HasValue)
            {
                var ownedIds = await chatObjectManager.GetIdListByUserIdAsync(currentUser.Id.Value);
                if (!ownedIds.Contains(unit.OwnerId))
                {
                    throw new AbpAuthorizationException("无权使用该群成员身份操作");
                }
            }
        }
        else
        {
            if (!currentUser.Id.HasValue)
            {
                throw new AbpAuthorizationException("未登录或缺少操作成员身份");
            }
            var ownedIds = await chatObjectManager.GetIdListByUserIdAsync(currentUser.Id.Value);
            var sessionUnitRepo = lazyServiceProvider.LazyGetRequiredService<ISessionUnitRepository>();
            var query = (await sessionUnitRepo.GetQueryableAsync())
                .Include(x => x.Setting)
                .Include(x => x.GrantList)
                .Include(x => x.SessionUnitRoleList)
                    .ThenInclude(x => x.SessionRole)
                        .ThenInclude(x => x.GrantList)
                .Where(x => x.SessionId == sessionId && ownedIds.Contains(x.OwnerId) && x.Setting != null && x.Setting.IsEnabled && !x.Setting.IsKilled);
            var candidates = await asyncExecuter.ToListAsync(query);
            if (candidates.Count == 0)
            {
                throw new AbpAuthorizationException("当前账号不是该群有效成员");
            }
            if (candidates.Count > 1)
            {
                throw new UserFriendlyException("当前账号在该群存在多个身份，请显式指定操作成员身份");
            }
            unit = candidates[0];
        }

        if (!string.IsNullOrEmpty(policyName))
        {
            var checker = lazyServiceProvider.LazyGetRequiredService<ISessionPermissionChecker>();
            await checker.CheckAsync(policyName, unit);
        }

        return unit;
    }
}

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
        if (!CurrentUser.Id.HasValue)
        {
            throw new AbpAuthorizationException("Authentication required.");
        }

        var requestedIds = ownerIdList.ToList();
        var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
        if (requestedIds.Any(x => !x.HasValue || !ownedIds.Contains(x.Value)))
        {
            throw new AbpAuthorizationException("The current user does not own the requested chat objects.");
        }

        if (func != null)
        {
            await func();
        }
    }

    protected virtual Task CheckPolicyForUserAsync(long? ownerId, Func<Task> func = null)
    {
        return CheckPolicyForUserAsync([ownerId], func);
    }

    // Account delegation is limited to article management and public account types.
    protected async Task CheckArticleManagementOrOwnerAsync(long? ownerId, Func<Task> ownerPolicy = null)
    {
        if (CurrentUser.Id.HasValue && ownerId.HasValue &&
            await AuthorizationService.IsGrantedAsync(IczpNet.Chat.MessageSections.ArticleMessagePermissions.Manage))
        {
            var repository = LazyServiceProvider.LazyGetRequiredService<Volo.Abp.Domain.Repositories.IRepository<ChatObject, long>>();
            var account = await repository.GetAsync(ownerId.Value);
            if (account.ObjectType == IczpNet.Chat.Enums.ChatObjectTypeEnums.Official ||
                account.ObjectType == IczpNet.Chat.Enums.ChatObjectTypeEnums.Subscription)
                return;
        }
        await CheckPolicyForUserAsync(ownerId, ownerPolicy);
    }

    protected virtual Task CheckPolicyForUserAsync(IEnumerable<long> ownerIdList, Func<Task> func = null)
    {
        return CheckPolicyForUserAsync(ownerIdList.Select(x => (long?)x), func);
    }
    #endregion

    #region  CheckPolicyAsync


    protected virtual async Task CheckGetListPolicyAsync()
    {
        await CheckPolicyAsync(GetListPolicyName);
    }

    protected virtual async Task CheckGetItemPolicyAsync()
    {
        await CheckPolicyAsync(GetPolicyName);
    }

    protected virtual async Task CheckCreatePolicyAsync()
    {
        await CheckPolicyAsync(CreatePolicyName);
    }

    protected virtual async Task CheckUpdatePolicyAsync()
    {
        await CheckPolicyAsync(UpdatePolicyName);
    }

    protected virtual async Task CheckDeletePolicyAsync()
    {
        await CheckPolicyAsync(DeletePolicyName);
    }

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

    protected virtual Task<SessionUnit> LoadDetailedSessionUnitAsync(Guid sessionUnitId)
        => SessionUnitPolicyHelper.LoadDetailedSessionUnitAsync(LazyServiceProvider, sessionUnitId);

    protected virtual Task<SessionUnit> ResolveAndCheckSessionUnitPolicyAsync(Guid sessionId, Guid? sessionUnitId, string policyName)
        => SessionUnitPolicyHelper.ResolveAndCheckSessionUnitPolicyAsync(LazyServiceProvider, CurrentUser, ChatObjectManager, sessionId, sessionUnitId, policyName);

    protected static bool IsBuiltInAdminRoleName(string roleName)
        => SessionUnitPolicyHelper.IsBuiltInAdminRoleName(roleName);

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
