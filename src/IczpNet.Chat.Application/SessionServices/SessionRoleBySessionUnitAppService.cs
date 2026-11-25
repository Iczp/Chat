using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionPermissions;
using IczpNet.Chat.SessionRoles;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionServices;

/// <summary>
/// 会话角色
/// </summary>
/// <param name="repository"></param>
public class SessionRoleBySessionUnitAppService(
    IRepository<SessionRole, Guid> repository)
        : CrudBySessionUnitChatAppService<
        SessionRole,
        SessionRoleDetailDto,
        SessionRoleDto,
        Guid,
        SessionRoleGetListBySessionUnitInput,
        SessionRoleCreateBySessionUnitInput,
        SessionRoleUpdateInput>(repository),
    ISessionRoleBySessionUnitAppService
{
    protected override string GetPolicyName { get; set; } = Permissions.SessionPermissionDefinitionConsts.SessionRolePermission.Default;
    protected override string GetListPolicyName { get; set; } = Permissions.SessionPermissionDefinitionConsts.SessionRolePermission.Default;
    protected override string CreatePolicyName { get; set; } = Permissions.SessionPermissionDefinitionConsts.SessionRolePermission.Create;
    protected override string UpdatePolicyName { get; set; } = Permissions.SessionPermissionDefinitionConsts.SessionRolePermission.Update;
    protected override string DeletePolicyName { get; set; } = Permissions.SessionPermissionDefinitionConsts.SessionRolePermission.Delete;
    protected override string DeleteManyPolicyName { get; set; } = Permissions.SessionPermissionDefinitionConsts.SessionRolePermission.Delete;
    protected ISessionRoleManager SessionRoleManager => LazyServiceProvider.LazyGetRequiredService<ISessionRoleManager>();

    protected override async Task<IQueryable<SessionRole>> CreateFilteredQueryAsync(SessionUnit sessionUnit, SessionRoleGetListBySessionUnitInput input)
    {
        return (await base.CreateFilteredQueryAsync(sessionUnit, input))
            .Where(x => x.SessionId.Value == sessionUnit.SessionId)
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword))
            ;
    }

    protected override Task SetCreateEntityAsync(SessionUnit sessionUnit, SessionRole entity, SessionRoleCreateBySessionUnitInput input)
    {
        entity.SetSessionId(sessionUnit.SessionId.Value);

        entity.SetPermissionGrant(input.PermissionGrant);

        return base.SetCreateEntityAsync(sessionUnit, entity, input);
    }


    protected override Task CheckGetPolicyAsync(SessionUnit sessionUnit, SessionRole entity)
    {
        Assert.If(sessionUnit.SessionId != entity.SessionId, "Not in same session");

        return base.CheckGetPolicyAsync(sessionUnit, entity);
    }

    protected override async Task CheckCreatePolicyAsync(SessionUnit sessionUnit, SessionRoleCreateBySessionUnitInput input)
    {
        Assert.If(IsBuiltInAdminRoleName(input.Name), "内置管理员角色由群主通过管理员任免维护");

        Assert.If(!await SessionRepository.AnyAsync(x => x.Id == sessionUnit.SessionId), $"No such entity of sessionId:{sessionUnit.SessionId}");

        Assert.If(await Repository.AnyAsync(x => x.SessionId == sessionUnit.SessionId && x.Name == input.Name), $"Already exists [{input.Name}].");

        await base.CheckCreatePolicyAsync(sessionUnit, input);
        await EnsureNoPermissionEscalationAsync(sessionUnit, input.PermissionGrant);
    }

    protected override async Task CheckUpdatePolicyAsync(SessionUnit sessionUnit, SessionRole entity, SessionRoleUpdateInput input)
    {
        Assert.If(sessionUnit.SessionId != entity.SessionId, $"Not in same session");
        Assert.If(IsBuiltInAdminRoleName(entity.Name) || IsBuiltInAdminRoleName(input.Name), "内置管理员角色不可通过自定义角色接口修改");

        await base.CheckUpdatePolicyAsync(sessionUnit, entity, input);
        await EnsureNoPermissionEscalationAsync(sessionUnit, input.PermissionGrant);
    }

    private async Task EnsureNoPermissionEscalationAsync(SessionUnit operatorUnit, Dictionary<string, PermissionGrantValue> permissionGrant)
    {
        if (permissionGrant == null || permissionGrant.Count == 0 || operatorUnit.Setting?.IsCreator == true)
        {
            return;
        }
        foreach (var kv in permissionGrant.Where(x => x.Value != null && x.Value.IsEnabled))
        {
            if (!await SessionPermissionChecker.IsGrantedAsync(kv.Key, operatorUnit))
            {
                throw new AbpAuthorizationException($"无权授予自身未持有的权限: {kv.Key}");
            }
        }
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

    protected override Task CheckDeletePolicyAsync(SessionUnit sessionUnit, SessionRole entity)
    {
        Assert.If(sessionUnit.SessionId != entity.SessionId, $"Not in same session");
        Assert.If(IsBuiltInAdminRoleName(entity.Name), "内置管理员角色不可删除");

        return base.CheckDeletePolicyAsync(sessionUnit, entity);
    }

    protected override Expression<Func<SessionRole, bool>> GetPredicateDeleteManyAsync(SessionUnit sessionUnit)
    {
        return x => x.SessionId == sessionUnit.SessionId && x.Name != "管理员" && x.Name != "Admin";
    }

    [HttpGet]
    public virtual async Task<SessionRolePermissionDto> GetPermissionsAsync(Guid sessionUnitId, Guid id)
    {
        var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);
        var entity = await GetEntityByIdAsync(id);
        Assert.If(sessionUnit.SessionId != entity.SessionId, "Not in same session");
        await CheckPolicyAsync(SessionPermissionDefinitionConsts.SessionRolePermission.Default, sessionUnit);
        return await MapToPermissionDtoAsync(entity);
    }

    [HttpPost]
    public virtual async Task<SessionRolePermissionDto> SetAllPermissionsAsync(Guid sessionUnitId, Guid id, PermissionGrantValue permissionGrantValue)
    {
        var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);
        var existing = await GetEntityByIdAsync(id);
        Assert.If(sessionUnit.SessionId != existing.SessionId, "Not in same session");
        Assert.If(IsBuiltInAdminRoleName(existing.Name), "内置管理员角色不可通过全量授权接口修改");
        await CheckPolicyAsync(SessionPermissionDefinitionConsts.SessionRolePermission.SetAllPermissions, sessionUnit);
        if (permissionGrantValue?.IsEnabled == true && sessionUnit.Setting?.IsCreator != true && !sessionUnit.IsAdmin)
        {
            throw new AbpAuthorizationException("仅群主或管理员可授予全量权限");
        }
        var entity = await SessionRoleManager.SetAllPermissionsAsync(id, permissionGrantValue);
        return await MapToPermissionDtoAsync(entity);
    }
}
