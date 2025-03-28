﻿using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions.Dtos;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionSections.SessionPermissionGroups;

namespace IczpNet.Chat.SessionServices;

/// <summary>
/// 会话内权限定义
/// </summary>
public class SessionPermissionDefinitionAppService
    : CrudChatAppService<
        SessionPermissionDefinition,
        SessionPermissionDefinitionDetailDto,
        SessionPermissionDefinitionDto,
        string,
        SessionPermissionDefinitionGetListInput,
        SessionPermissionDefinitionCreateInput,
        SessionPermissionDefinitionUpdateInput>,
    ISessionPermissionDefinitionAppService
{

    protected override string UpdatePolicyName { get; set; } = ChatPermissions.SessionPermissionDefinitionPermission.Update;
    protected virtual string SetIsEnabledPolicyName { get; set; } = ChatPermissions.SessionPermissionDefinitionPermission.SetIsEnabled;
    protected virtual string SetAllIsEnabledPolicyName { get; set; } = ChatPermissions.SessionPermissionDefinitionPermission.SetAllIsEnabled;
    protected ISessionPermissionGroupManager SessionPermissionGroupManager { get; }
    protected new ISessionPermissionDefinitionRepository Repository { get; }

    public SessionPermissionDefinitionAppService(
        ISessionPermissionDefinitionRepository repository,
        ISessionPermissionGroupManager sessionPermissionGroupManager) : base(repository)
    {
        Repository = repository;
        SessionPermissionGroupManager = sessionPermissionGroupManager;
    }

    protected override async Task<IQueryable<SessionPermissionDefinition>> CreateFilteredQueryAsync(SessionPermissionDefinitionGetListInput input)
    {

        IQueryable<long> groupIdQuery = null;

        if (input.IsImportChildGroup && input.GroupIdList.IsAny())
        {
            groupIdQuery = (await SessionPermissionGroupManager.QueryCurrentAndAllChildsAsync(input.GroupIdList)).Select(x => x.Id);
        }
        return (await base.CreateFilteredQueryAsync(input))
            //GroupId
            .WhereIf(!input.IsImportChildGroup && input.GroupIdList.IsAny(), x => input.GroupIdList.Contains(x.GroupId.Value))
            .WhereIf(input.IsImportChildGroup && input.GroupIdList.IsAny(), x => groupIdQuery.Contains(x.GroupId.Value))
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword))
            ;
    }

    protected override IQueryable<SessionPermissionDefinition> ApplyDefaultSorting(IQueryable<SessionPermissionDefinition> query)
    {
        return query
            .OrderByDescending(x => x.Group.Sorting)
            .ThenBy(x => x.Group.FullPathName)
            .ThenByDescending(x => x.Sorting);
    }

    [RemoteService(false)]
    public override Task<SessionPermissionDefinitionDetailDto> CreateAsync(SessionPermissionDefinitionCreateInput input) => throw new NotImplementedException();

    [RemoteService(false)]
    public override Task DeleteAsync(string id) => base.DeleteAsync(id);

    [RemoteService(false)]
    public override Task DeleteManyAsync(List<string> idList) => base.DeleteManyAsync(idList);

    /// <summary>
    /// 启用或禁用
    /// </summary>
    /// <param name="id">权限Id</param>
    /// <param name="isEnabled">启用或禁用</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<SessionPermissionDefinitionDto> SetIsEnabledAsync(string id, bool isEnabled)
    {
        await CheckPolicyAsync(SetIsEnabledPolicyName);

        var entity = await Repository.GetAsync(id);

        entity.IsEnabled = isEnabled;

        await Repository.UpdateAsync(entity, autoSave: true);

        return await MapToGetOutputDtoAsync(entity);
    }

    /// <summary>
    /// 全部启用或禁用
    /// </summary>
    /// <param name="isEnabled">启用或禁用</param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<int> SetAllIsEnabledAsync(bool isEnabled)
    {
        await CheckPolicyAsync(SetAllIsEnabledPolicyName);

        return await Repository.BatchUpdateIsEnabledAsync(isEnabled);
    }

   
}
