using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.Sessions.Dtos;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionSections.SessionTags.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionServices;

/// <summary>
/// 聊天会话
/// </summary>
public class SessionAppService : ChatAppService, ISessionAppService
{
    protected IRepository<Session, Guid> Repository { get; }
    protected ISessionManager SessionManager { get; }
    protected ISessionGenerator SessionGenerator { get; }

    public SessionAppService(
        ISessionManager sessionManager,
        ISessionGenerator sessionGenerator,
        IRepository<Session, Guid> repository)
    {
        SessionManager = sessionManager;
        SessionGenerator = sessionGenerator;
        Repository = repository;
    }

    /// <summary>
    /// 聊天会话列表 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PagedResultDto<SessionDto>> GetListAsync(SessionGetListInput input)
    {
        var query = (await Repository.GetQueryableAsync())
            .WhereIf(input.OwnerId.HasValue, x => x.UnitList.Any(m => m.OwnerId == input.OwnerId))
            ;
        return await query.ToPagedListAsync<Session, SessionDto>(AsyncExecuter, ObjectMapper, input, q => q.OrderByDescending(x => x.LastMessageId));
    }

    /// <summary>
    /// 获取一个聊天会话
    /// </summary>
    /// <param name="id">主建Id</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<SessionDto> GetAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        return await MapToDtoAsync(entity);
    }

    protected virtual Task<SessionDto> MapToDtoAsync(Session entity)
    {
        return Task.FromResult(ObjectMapper.Map<Session, SessionDto>(entity));
    }

    /// <summary>
    /// 聊天会话详情
    /// </summary>
    /// <param name="id">主建Id</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<SessionDetailDto> GetDetailAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        return await MapToDetailDtoAsync(entity);
    }

    protected virtual Task<SessionDetailDto> MapToDetailDtoAsync(Session entity)
    {
        return Task.FromResult(ObjectMapper.Map<Session, SessionDetailDto>(entity));
    }

    /// <summary>
    /// 根据消息Id生成会话
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<List<SessionDto>> GenerateSessionByMessageAsync()
    {
        var entitys = await SessionGenerator.GenerateSessionByMessageAsync();
        return ObjectMapper.Map<List<Session>, List<SessionDto>>(entitys);
    }

    /// <summary>
    /// 添加会话标签
    /// </summary>
    /// <param name="sessionId">会话Id</param>
    /// <param name="name">名称</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<SessionTagDto> AddTagAsync(Guid sessionId, string name)
    {
        var entity = await Repository.GetAsync(sessionId);

        var tag = await SessionManager.AddTagAsync(entity, new SessionTag(GuidGenerator.Create(), sessionId, name));

        return ObjectMapper.Map<SessionTag, SessionTagDto>(tag);
    }

    /// <summary>
    /// 删除会话标签
    /// </summary>
    /// <param name="tagId">会话标签Id</param>
    /// <returns></returns>
    [HttpPost]
    public Task RemoveTagAsync(Guid tagId)
    {
        return SessionManager.RemoveTagAsync(tagId);
    }

    /// <summary>
    /// 添加标签成员
    /// </summary>
    /// <param name="tagId">会话标签Id</param>
    /// <param name="sessionUnitIdList">会话单元Id列表</param>
    /// <returns></returns>
    [HttpPost]
    public async Task AddTagMembersAsync(Guid tagId, List<Guid> sessionUnitIdList)
    {
        await SessionManager.AddTagMembersAsync(tagId, sessionUnitIdList);
    }

    /// <summary>
    /// 删除标签成员
    /// </summary>
    /// <param name="tagId">会话标签Id</param>
    /// <param name="sessionUnitIdList">会话单元Id列表</param>
    /// <returns></returns>
    [HttpPost]
    public async Task RemoveTagMembersAsync(Guid tagId, List<Guid> sessionUnitIdList)
    {
        await SessionManager.RemoveTagMembersAsync(tagId, sessionUnitIdList);
    }

    /// <summary>
    /// 添加会话角色
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<SessionRoleDto> AddRoleAsync(Guid sessionId, string name)
    {
        var entity = await Repository.GetAsync(sessionId);

        var role = await SessionManager.AddRoleAsync(entity, new SessionRole(GuidGenerator.Create(), sessionId, name));

        return ObjectMapper.Map<SessionRole, SessionRoleDto>(role);
    }

    /// <summary>
    /// 删除会话角色
    /// </summary>
    /// <param name="roleId">会话角色Id</param>
    /// <returns></returns>
    [HttpPost]
    public Task RemoveRoleAsync(Guid roleId)
    {
        return SessionManager.RemoveRoleAsync(roleId);
    }

    /// <summary>
    /// 添加角色成员
    /// </summary>
    /// <param name="roleId">会话角色Id</param>
    /// <param name="sessionUnitIdList">会话单元Id列表</param>
    /// <returns></returns>
    [HttpPost]
    public async Task AddRoleMembersAsync(Guid roleId, List<Guid> sessionUnitIdList)
    {
        await SessionManager.AddRoleMembersAsync(roleId, sessionUnitIdList);
    }

    /// <summary>
    /// 删除角色成员
    /// </summary>
    /// <param name="roleId">会话角色Id</param>
    /// <param name="sessionUnitIdList">会话单元Id列表</param>
    /// <returns></returns>
    [HttpPost]
    public Task RemoveRoleMembersAsync(Guid roleId, List<Guid> sessionUnitIdList)
    {
        return SessionManager.RemoveRoleMembersAsync(roleId, sessionUnitIdList);
    }

    /// <summary>
    /// 设置角色
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="roleIdList">会话角色Id</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpPost]
    public Task<List<SessionRoleDto>> SetRolesAsync(Guid sessionUnitId, List<Guid> roleIdList)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 设置标签
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="tagIdList">会话标签Id列表</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpPost]
    public Task<List<SessionTagDto>> SetTagsAsync(Guid sessionUnitId, List<Guid> tagIdList)
    {
        throw new NotImplementedException();
    }
}
