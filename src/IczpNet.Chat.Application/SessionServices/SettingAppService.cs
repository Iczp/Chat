using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionServices;

public class SettingAppService : ChatAppService, ISettingAppService
{
    protected override string GetListPolicyName { get; set; }
    protected override string GetPolicyName { get; set; }
    protected virtual string SetRenamePolicyName { get; set; }
    protected virtual string SetMemberNamePolicyName { get; set; }
    protected virtual string GetDetailPolicyName { get; set; }
    protected virtual string SetReadedPolicyName { get; set; }
    protected virtual string SetToppingPolicyName { get; set; }
    protected virtual string SetImmersedPolicyName { get; set; }
    protected virtual string RemoveSessionPolicyName { get; set; }
    protected virtual string ClearMessagePolicyName { get; set; }
    protected virtual string DeleteMessagePolicyName { get; set; }

    protected ISessionUnitRepository Repository { get; }
    protected ISessionUnitManager SessionUnitManager { get; }

    public SettingAppService(
        ISessionUnitRepository repository,
        ISessionUnitManager sessionUnitManager)
    {
        Repository = repository;
        SessionUnitManager = sessionUnitManager;
    }

    /// <inheritdoc/>
    protected override Task CheckPolicyAsync(string policyName)
    {
        return base.CheckPolicyAsync(policyName);
    }


    /// <inheritdoc/>
    protected virtual async Task<SessionUnit> GetEntityAsync(Guid id, bool checkIsKilled = true)
    {
        var entity = await Repository.GetAsync(id);

        Assert.If(checkIsKilled && entity.Setting.IsKilled, "已经删除的会话单元!");

        return entity;
    }

    /// <inheritdoc/>
    protected virtual Task<SessionUnitOwnerDto> MapToDtoAsync(SessionUnit entity)
    {
        return Task.FromResult(ObjectMapper.Map<SessionUnit, SessionUnitOwnerDto>(entity));
    }

    /// <inheritdoc/>
    protected virtual Task<SessionUnitDestinationDto> MapToDestinationDtoAsync(SessionUnit entity)
    {
        return Task.FromResult(ObjectMapper.Map<SessionUnit, SessionUnitDestinationDto>(entity));
    }

    /// <inheritdoc/>
    [HttpPost]
    public async Task<SessionUnitOwnerDto> SetMemberNameAsync(Guid sessionUnitId, string memberName)
    {
        await CheckPolicyAsync(SetMemberNamePolicyName);

        var entity = await GetEntityAsync(sessionUnitId);

        await SessionUnitManager.SetMemberNameAsync(entity, memberName);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public async Task<SessionUnitOwnerDto> SetRenameAsync(Guid sessionUnitId, string rename)
    {
        await CheckPolicyAsync(SetRenamePolicyName);

        var entity = await GetEntityAsync(sessionUnitId);

        await SessionUnitManager.SetRenameAsync(entity, rename);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> SetToppingAsync(Guid sessionUnitId, bool isTopping)
    {
        await CheckPolicyAsync(SetToppingPolicyName);

        var entity = await GetEntityAsync(sessionUnitId);

        await SessionUnitManager.SetToppingAsync(entity, isTopping);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> SetReadedMessageIdAsync(Guid sessionUnitId, bool isForce = false, long? messageId = null)
    {
        await CheckPolicyAsync(SetReadedPolicyName);

        var entity = await GetEntityAsync(sessionUnitId);

        await SessionUnitManager.SetReadedMessageIdAsync(entity, isForce, messageId);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public async Task<SessionUnitOwnerDto> SetImmersedAsync(Guid sessionUnitId, bool isImmersed)
    {
        await CheckPolicyAsync(SetImmersedPolicyName);

        var entity = await GetEntityAsync(sessionUnitId);

        await SessionUnitManager.SetImmersedAsync(entity, isImmersed);

        return await MapToDtoAsync(entity);
    }


    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> RemoveAsync(Guid sessionUnitId)
    {
        await CheckPolicyAsync(RemoveSessionPolicyName);

        var entity = await GetEntityAsync(sessionUnitId);

        await SessionUnitManager.RemoveAsync(entity);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> KillAsync(Guid sessionUnitId)
    {
        var entity = await GetEntityAsync(sessionUnitId);

        await SessionUnitManager.KillAsync(entity);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> ClearMessageAsync(Guid sessionUnitId)
    {
        await CheckPolicyAsync(ClearMessagePolicyName);

        var entity = await GetEntityAsync(sessionUnitId);

        await SessionUnitManager.ClearMessageAsync(entity);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> DeleteMessageAsync(Guid sessionUnitId, long messageId)
    {
        await CheckPolicyAsync(DeleteMessagePolicyName);

        throw new NotImplementedException();
    }
}
