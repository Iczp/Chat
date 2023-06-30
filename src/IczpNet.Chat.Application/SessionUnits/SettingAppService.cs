using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ContactTags;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionSections.SessionUnitContactTags;
using IczpNet.Chat.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// 聊天设置
/// </summary>
public class SettingAppService : ChatAppService, ISettingAppService
{
    protected virtual string SetRenamePolicyName { get; set; } = ChatPermissions.SessionUnitSettingPermissions.SetRename;
    protected virtual string SetMemberNamePolicyName { get; set; } = ChatPermissions.SessionUnitSettingPermissions.SetMemberName;
    protected virtual string SetReadedPolicyName { get; set; } = ChatPermissions.SessionUnitSettingPermissions.SetReaded;
    protected virtual string SetToppingPolicyName { get; set; } = ChatPermissions.SessionUnitSettingPermissions.SetTopping;
    protected virtual string SetImmersedPolicyName { get; set; } = ChatPermissions.SessionUnitSettingPermissions.SetImmersed;
    protected virtual string RemoveSessionPolicyName { get; set; } = ChatPermissions.SessionUnitSettingPermissions.RemoveSession;
    protected virtual string ClearMessagePolicyName { get; set; } = ChatPermissions.SessionUnitSettingPermissions.ClearMessage;
    protected virtual string DeleteMessagePolicyName { get; set; } = ChatPermissions.SessionUnitSettingPermissions.DeleteMessage;
    protected virtual string SetContactTagsPolicyName { get; set; } = ChatPermissions.SessionUnitSettingPermissions.SetContactTags;
    protected virtual string KillPolicyName { get; set; } = ChatPermissions.SessionUnitSettingPermissions.Kill;
    protected virtual string SetMuteExpireTimePolicyName { get; set; } = ChatPermissions.SessionUnitSettingPermissions.SetMuteExpireTime;
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
    protected virtual async Task<SessionUnit> GetEntityAsync([Required] Guid id, bool checkIsKilled = true)
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

    /// <summary>
    /// 设置成员（群内名称，会话内名称）
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="memberName">会话内名称</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<SessionUnitOwnerDto> SetMemberNameAsync([Required] Guid sessionUnitId, string memberName)
    {
        var entity = await GetAndCheckPolicyAsync(SetMemberNamePolicyName, sessionUnitId);

        await SessionUnitManager.SetMemberNameAsync(entity, memberName);

        return await MapToDtoAsync(entity);
    }

    /// <summary>
    /// 备注名称
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="rename">名称</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<SessionUnitOwnerDto> SetRenameAsync([Required] Guid sessionUnitId, string rename)
    {
        var entity = await GetAndCheckPolicyAsync(SetRenamePolicyName, sessionUnitId);

        await SessionUnitManager.SetRenameAsync(entity, rename);

        return await MapToDtoAsync(entity);
    }

    /// <summary>
    /// 设置置顶
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="isTopping">是否置顶</param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> SetToppingAsync([Required] Guid sessionUnitId, bool isTopping)
    {
        var entity = await GetAndCheckPolicyAsync(SetToppingPolicyName, sessionUnitId);

        await SessionUnitManager.SetToppingAsync(entity, isTopping);

        return await MapToDtoAsync(entity);
    }

    /// <summary>
    /// 设置已读消息Id
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="isForce">是否强制</param>
    /// <param name="messageId">消息Id</param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> SetReadedMessageIdAsync([Required] Guid sessionUnitId, bool isForce = false, long? messageId = null)
    {
        var entity = await GetAndCheckPolicyAsync(SetReadedPolicyName, sessionUnitId);

        await SessionUnitManager.SetReadedMessageIdAsync(entity, isForce, messageId);

        return await MapToDtoAsync(entity);
    }

    /// <summary>
    /// 设置为静默模式（免打扰）
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<SessionUnitOwnerDto> SetImmersedAsync([Required] Guid sessionUnitId, bool isImmersed)
    {
        var entity = await GetAndCheckPolicyAsync(SetImmersedPolicyName, sessionUnitId);

        await SessionUnitManager.SetImmersedAsync(entity, isImmersed);

        return await MapToDtoAsync(entity);
    }


    /// <summary>
    /// 移除会话
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> RemoveAsync([Required] Guid sessionUnitId)
    {
        var entity = await GetAndCheckPolicyAsync(RemoveSessionPolicyName, sessionUnitId);

        await SessionUnitManager.RemoveAsync(entity);

        return await MapToDtoAsync(entity);
    }

    /// <summary>
    /// 退出会话
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> ExitAsync([Required] Guid sessionUnitId)
    {
        var entity = await GetAndCheckPolicyAsync(RemoveSessionPolicyName, sessionUnitId);

        await SessionUnitManager.RemoveAsync(entity);

        return await MapToDtoAsync(entity);
    }

    /// <summary>
    /// 删除会话
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> KillAsync([Required] Guid sessionUnitId)
    {
        var entity = await GetAndCheckPolicyAsync(KillPolicyName, sessionUnitId);

        await SessionUnitManager.KillAsync(entity);

        return await MapToDtoAsync(entity);
    }

    /// <summary>
    /// 清空消息
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> ClearMessageAsync([Required] Guid sessionUnitId)
    {
        var entity = await GetAndCheckPolicyAsync(ClearMessagePolicyName, sessionUnitId);

        await SessionUnitManager.ClearMessageAsync(entity);

        return await MapToDtoAsync(entity);
    }

    /// <summary>
    /// 删除消息
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="messageId">消息Id</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpPost]
    public virtual async Task DeleteMessageAsync([Required] Guid sessionUnitId, long messageId)
    {
        var entity = await GetAndCheckPolicyAsync(DeleteMessagePolicyName, sessionUnitId);

        throw new NotImplementedException();
    }

    /// <summary>
    /// 设置联系人标签
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="contactTagIdList">联系人标签Id</param>
    /// <returns></returns>
    [HttpPost]
    public async Task SetContactTagsAsync([Required] Guid sessionUnitId, List<Guid> contactTagIdList)
    {
        var entity = await GetAndCheckPolicyAsync(SetContactTagsPolicyName, sessionUnitId);

        var contactTags = (await ContactTagRepository.GetQueryableAsync())
            .Where(x => x.OwnerId == entity.OwnerId)
            .Where(x => contactTagIdList.Contains(x.Id))
            .ToList();

        entity.SessionUnitContactTagList?.Clear();

        entity.SessionUnitContactTagList = contactTags.Select(x => new SessionUnitContactTag(entity, x)).ToList();
    }

    /// <summary>
    /// 禁言过期时间，为空则不禁言
    /// </summary>
    /// <param name="muterSessionUnitId">被设置的会话单元Id</param>
    /// <param name="setterSessionUnitId">设置者会话单元Id</param>
    /// <param name="muteExpireTime">为空则不禁言</param>
    /// <returns>禁言过期时间</returns>
    public async Task<DateTime?> SetMuteExpireTimeAsync([Required] Guid muterSessionUnitId, [Required] Guid setterSessionUnitId, DateTime? muteExpireTime)
    {
        var setterSessionUnit = await GetEntityAsync(setterSessionUnitId);

        await CheckPolicyAsync(SessionPermissionDefinitionConsts.SessionUnitPermissions.SetMuteExpireTime, setterSessionUnit);

        var muterSessionUnit = await GetEntityAsync(muterSessionUnitId);

        return await SessionUnitManager.SetMuteExpireTimeAsync(muterSessionUnit, muteExpireTime, setterSessionUnit, true);
    }
}
