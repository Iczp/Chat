using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionUnits;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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
    //protected override string GetPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Default;
    //protected override string GetListPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Default;
    //protected override string CreatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Create;
    //protected override string UpdatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Update;
    //protected override string DeletePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Delete;
    //protected override string DeleteManyPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionRolePermission.Delete;
    protected ISessionRoleManager SessionRoleManager { get; }

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


    protected override async Task CheckCreatePolicyAsync(SessionUnit sessionUnit, SessionRoleCreateBySessionUnitInput input)
    {
        Assert.If(!await SessionRepository.AnyAsync(x => x.Id == sessionUnit.SessionId), $"No such entity of sessionId:{sessionUnit.SessionId}");

        Assert.If(await Repository.AnyAsync(x => x.SessionId == sessionUnit.SessionId && x.Name == input.Name), $"Already exists [{input.Name}].");

        await base.CheckCreatePolicyAsync(sessionUnit, input);
    }

    protected override Task CheckUpdatePolicyAsync(SessionUnit sessionUnit, SessionRole entity, SessionRoleUpdateInput input)
    {
        Assert.If(sessionUnit.SessionId != entity.SessionId, $"Not in same session");

        return base.CheckUpdatePolicyAsync(sessionUnit, entity, input);
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

        return base.CheckDeletePolicyAsync(sessionUnit, entity);
    }

    protected override Expression<Func<SessionRole, bool>> GetPredicateDeleteManyAsync(SessionUnit sessionUnit)
    {
        return x => x.SessionId == sessionUnit.SessionId;
    }
}
