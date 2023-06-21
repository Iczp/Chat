using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.Sessions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    protected override string GetPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Default;
    protected override string GetListPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Default;
    protected override string CreatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Create;
    protected override string UpdatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Update;
    protected override string DeletePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Delete;
    protected virtual string SetAllPermissionsPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.SetAllPermissions;

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

    protected override async Task CheckCreateAsync(SessionRoleCreateInput input)
    {
        Assert.If(!await SessionRepository.AnyAsync(x => x.Id == input.SessionId), $"No such entity of sessionId:{input.SessionId}");

        Assert.If(await Repository.AnyAsync(x => x.SessionId == input.SessionId && x.Name == input.Name), $"Already exists [{input.Name}].");
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

        return await MapToPermissionDtoAsync(entity);
    }

    /// <summary>
    /// 授予角色所有权限
    /// </summary>
    /// <param name="id">主建Id</param>
    /// <param name="permissionGrantValue">授予值</param>
    /// <returns></returns>
    //[Authorize(policy: SessionPermissionDefinitionConsts.SessionRolePermission.SetAllPermissions)]
    [HttpPost]
    public virtual async Task<SessionRolePermissionDto> SetAllPermissionsAsync(Guid id, PermissionGrantValue permissionGrantValue)
    {
        //await CheckPolicyAsync(SetAllPermissionsPolicyName);

        var entity = await SessionRoleManager.SetAllPermissionsAsync(id, permissionGrantValue);

        return await MapToPermissionDtoAsync(entity);
    }
}
