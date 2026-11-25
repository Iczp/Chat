using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionPermissions;
using IczpNet.Chat.SessionRoles;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionServices;

/// <summary>
/// 会话内角色
/// </summary>
public class SessionRoleAppService
    : CrudChatAppService<
        SessionRole,
        SessionRoleDetailDto,
        SessionRoleDto,
        Guid,
        SessionRoleGetListInput,
        SessionRoleCreateInput,
        SessionRoleUpdateInput>,
    ISessionRoleAppService
{
    //protected override string GetPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Default;
    //protected override string GetListPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Default;
    //protected override string CreatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Create;
    //protected override string UpdatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Update;
    //protected override string DeletePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Delete;
    //protected virtual string SetAllPermissionsPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.SetAllPermissions;

    protected ISessionRoleManager SessionRoleManager { get; }
    protected IRepository<Session, Guid> SessionRepository{ get; }

    public SessionRoleAppService(
            IRepository<SessionRole, Guid> repository,
            ISessionRoleManager sessionRoleManager,
            IRepository<Session, Guid> sessionRepository)
            : base(repository)
    {
        SessionRoleManager = sessionRoleManager;
        SessionRepository = sessionRepository;
    }

    protected override async Task<IQueryable<SessionRole>> CreateFilteredQueryAsync(SessionRoleGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(input.SessionId.HasValue, x => x.SessionId.Value == input.SessionId);
        ;
    }

    protected override async Task CheckGetListPolicyAsync(SessionRoleGetListInput input)
    {
        if (input.SessionId.HasValue)
        {
            await ResolveAndCheckSessionUnitPolicyAsync(input.SessionId.Value, null, SessionPermissionDefinitionConsts.SessionRolePermission.Default);
        }
        await base.CheckGetListPolicyAsync(input);
    }

    protected override async Task CheckCreateAsync(SessionRoleCreateInput input)
    {
        Assert.If(IsBuiltInAdminRoleName(input.Name), "内置管理员角色由群主通过管理员任免维护");

        Assert.If(!await SessionRepository.AnyAsync(x => x.Id == input.SessionId), $"No such entity of sessionId:{input.SessionId}");

        Assert.If(await Repository.AnyAsync(x => x.SessionId == input.SessionId && x.Name == input.Name), $"Already exists [{input.Name}].");
    }

    protected override async Task CheckCreatePolicyAsync(SessionRoleCreateInput input)
    {
        var operatorUnit = await ResolveAndCheckSessionUnitPolicyAsync(
            input.SessionId!.Value,
            input.SessionUnitId,
            SessionPermissionDefinitionConsts.SessionRolePermission.Create);
        await EnsureNoPermissionEscalationAsync(operatorUnit, input.PermissionGrant);
    }

    public override async Task<SessionRoleDetailDto> UpdateAsync(Guid id, SessionRoleUpdateInput input)
    {
        var entity = await GetEntityByIdAsync(id);
        Assert.If(IsBuiltInAdminRoleName(entity.Name) || IsBuiltInAdminRoleName(input.Name), "内置管理员角色不可通过自定义角色接口修改");
        var operatorUnit = await ResolveAndCheckSessionUnitPolicyAsync(
            entity.SessionId!.Value,
            input.SessionUnitId,
            SessionPermissionDefinitionConsts.SessionRolePermission.Update);
        await EnsureNoPermissionEscalationAsync(operatorUnit, input.PermissionGrant);
        return await base.UpdateAsync(id, input);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await GetEntityByIdAsync(id);
        Assert.If(IsBuiltInAdminRoleName(entity.Name), "内置管理员角色不可删除");
        await ResolveAndCheckSessionUnitPolicyAsync(
            entity.SessionId!.Value,
            null,
            SessionPermissionDefinitionConsts.SessionRolePermission.Delete);
        await base.DeleteAsync(id);
    }

    public override async Task DeleteManyAsync(List<Guid> idList)
    {
        var entities = await Repository.GetListAsync(x => idList.Contains(x.Id));
        Assert.If(entities.Any(x => IsBuiltInAdminRoleName(x.Name)), "内置管理员角色不可删除");
        foreach (var sessionId in entities.Where(x => x.SessionId.HasValue).Select(x => x.SessionId!.Value).Distinct())
        {
            await ResolveAndCheckSessionUnitPolicyAsync(sessionId, null, SessionPermissionDefinitionConsts.SessionRolePermission.Delete);
        }
        await base.DeleteManyAsync(idList);
    }

    private async Task EnsureNoPermissionEscalationAsync(SessionUnit operatorUnit, Dictionary<string, PermissionGrantValue> permissionGrant)
    {
        if (permissionGrant == null || permissionGrant.Count == 0)
        {
            return;
        }
        if (operatorUnit.Setting?.IsCreator == true || operatorUnit.IsAdmin)
        {
            return;
        }
        var checker = LazyServiceProvider.LazyGetRequiredService<ISessionPermissionChecker>();
        foreach (var kv in permissionGrant.Where(x => x.Value != null && x.Value.IsEnabled))
        {
            if (!await checker.IsGrantedAsync(kv.Key, operatorUnit))
            {
                throw new AbpAuthorizationException($"无权授予自身未持有的权限: {kv.Key}");
            }
        }
    }

    protected override async Task<SessionRole> MapToEntityAsync(SessionRoleCreateInput createInput)
    {
        await Task.Yield();

        var entity = new SessionRole(GuidGenerator.Create(), createInput.SessionId.Value, createInput.Name);

        return entity;
    }

    protected override Task SetCreateEntityAsync(SessionRole entity, SessionRoleCreateInput input)
    {
        entity.SetPermissionGrant(input.PermissionGrant);

        return base.SetCreateEntityAsync(entity, input);
    }

    protected override Task SetUpdateEntityAsync(SessionRole entity, SessionRoleUpdateInput input)
    {
        entity.SetPermissionGrant(input.PermissionGrant);

        return base.SetUpdateEntityAsync(entity, input);
    }

    protected virtual SessionRolePermissionDto MapToPermissionDto(SessionRole entity)
    {
        return ObjectMapper.Map<SessionRole, SessionRolePermissionDto>(entity);
    }

    protected virtual async Task<SessionRolePermissionDto> MapToPermissionDtoAsync(SessionRole entity)
    {
        await Task.Yield();

        return MapToPermissionDto(entity);
    }

    /// <summary>
    /// 获取角色权限
    /// </summary>
    /// <param name="id">主建Id</param>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<SessionRolePermissionDto> GetPermissionsAsync(Guid id)
    {
        var entity = await GetEntityByIdAsync(id);
        await ResolveAndCheckSessionUnitPolicyAsync(entity.SessionId!.Value, null, SessionPermissionDefinitionConsts.SessionRolePermission.Default);

        return await MapToPermissionDtoAsync(entity);
    }

    /// <summary>
    /// 授予角色所有权限
    /// </summary>
    /// <param name="id">主建Id</param>
    /// <param name="permissionGrantValue">授予值</param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<SessionRolePermissionDto> SetAllPermissionsAsync(Guid id, PermissionGrantValue permissionGrantValue)
    {
        var existing = await GetEntityByIdAsync(id);
        Assert.If(IsBuiltInAdminRoleName(existing.Name), "内置管理员角色不可通过全量授权接口修改");
        var operatorUnit = await ResolveAndCheckSessionUnitPolicyAsync(
            existing.SessionId!.Value,
            null,
            SessionPermissionDefinitionConsts.SessionRolePermission.SetAllPermissions);

        if (permissionGrantValue?.IsEnabled == true && operatorUnit.Setting?.IsCreator != true && !operatorUnit.IsAdmin)
        {
            throw new AbpAuthorizationException("仅群主或管理员可授予全量权限");
        }

        var entity = await SessionRoleManager.SetAllPermissionsAsync(id, permissionGrantValue);

        return await MapToPermissionDtoAsync(entity);
    }
}
