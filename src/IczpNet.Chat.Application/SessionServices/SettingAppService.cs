using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ContactTags;
using IczpNet.Chat.SessionSections.SessionUnitContactTags;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

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
    protected virtual string SetContactTagsPolicyName { get; set; }
    protected virtual string KillPolicyName { get; set; }

    protected ISessionUnitRepository Repository { get; }
    protected IRepository<ContactTag, Guid> ContactTagRepository { get; }


    public SettingAppService(
        ISessionUnitRepository repository,
        IRepository<ContactTag, Guid> contactTagRepository)
    {
        Repository = repository;
        ContactTagRepository = contactTagRepository;
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
        var entity = await GetAndCheckPolicyAsync(SetMemberNamePolicyName, sessionUnitId);

        await SessionUnitManager.SetMemberNameAsync(entity, memberName);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public async Task<SessionUnitOwnerDto> SetRenameAsync(Guid sessionUnitId, string rename)
    {
        var entity = await GetAndCheckPolicyAsync(SetRenamePolicyName, sessionUnitId);

        await SessionUnitManager.SetRenameAsync(entity, rename);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> SetToppingAsync(Guid sessionUnitId, bool isTopping)
    {
        var entity = await GetAndCheckPolicyAsync(SetToppingPolicyName, sessionUnitId);

        await SessionUnitManager.SetToppingAsync(entity, isTopping);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> SetReadedMessageIdAsync(Guid sessionUnitId, bool isForce = false, long? messageId = null)
    {
        var entity = await GetAndCheckPolicyAsync(SetReadedPolicyName, sessionUnitId);

        await SessionUnitManager.SetReadedMessageIdAsync(entity, isForce, messageId);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public async Task<SessionUnitOwnerDto> SetImmersedAsync(Guid sessionUnitId, bool isImmersed)
    {
        var entity = await GetAndCheckPolicyAsync(SetImmersedPolicyName, sessionUnitId);

        await SessionUnitManager.SetImmersedAsync(entity, isImmersed);

        return await MapToDtoAsync(entity);
    }


    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> RemoveAsync(Guid sessionUnitId)
    {
        var entity = await GetAndCheckPolicyAsync(RemoveSessionPolicyName, sessionUnitId);

        await SessionUnitManager.RemoveAsync(entity);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> ExitAsync(Guid sessionUnitId)
    {
        var entity = await GetAndCheckPolicyAsync(RemoveSessionPolicyName, sessionUnitId);

        await SessionUnitManager.RemoveAsync(entity);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> KillAsync(Guid sessionUnitId)
    {
        var entity = await GetAndCheckPolicyAsync(KillPolicyName, sessionUnitId);

        await SessionUnitManager.KillAsync(entity);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> ClearMessageAsync(Guid sessionUnitId)
    {
        var entity = await GetAndCheckPolicyAsync(ClearMessagePolicyName, sessionUnitId);

        await SessionUnitManager.ClearMessageAsync(entity);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task DeleteMessageAsync(Guid sessionUnitId, long messageId)
    {
        var entity = await GetAndCheckPolicyAsync(DeleteMessagePolicyName, sessionUnitId);

        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    [HttpPost]
    public async Task SetContactTagsAsync(Guid sessionUnitId, List<Guid> contactTagIdList)
    {
        var entity = await GetAndCheckPolicyAsync(SetContactTagsPolicyName, sessionUnitId);

        var contactTags = (await ContactTagRepository.GetQueryableAsync())
            .Where(x => x.OwnerId == entity.OwnerId)
            .Where(x => contactTagIdList.Contains(x.Id))
            .ToList();

        entity.SessionUnitContactTagList?.Clear();

        entity.SessionUnitContactTagList = contactTags.Select(x => new SessionUnitContactTag(entity, x)).ToList();
    }
}
