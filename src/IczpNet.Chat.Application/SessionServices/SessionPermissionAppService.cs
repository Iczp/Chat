using IczpNet.AbpCommons.Dtos;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissionGroups;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionSections.SessionPermissions.Dtos;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionServices;

/// <summary>
/// 会话权限
/// </summary>
public class SessionPermissionAppService : ChatAppService, ISessionPermissionAppService
{
    protected ISessionUnitRepository SessionUnitRepository { get; }
    protected IRepository<SessionRole, Guid> SessionRoleRepository { get; }
    protected ISessionPermissionDefinitionRepository SessionPermissionDefinitionRepository { get; }
    protected IRepository<SessionPermissionGroup, long> SessionPermissionGroupRepository { get; }
    public SessionPermissionAppService(ISessionUnitRepository sessionUnitRepository,
        ISessionPermissionDefinitionRepository sessionPermissionDefinitionRepository,
        IRepository<SessionRole, Guid> sessionRoleRepository,
        IRepository<SessionPermissionGroup, long> sessionPermissionGroupRepository)
    {
        SessionUnitRepository = sessionUnitRepository;
        SessionPermissionDefinitionRepository = sessionPermissionDefinitionRepository;
        SessionRoleRepository = sessionRoleRepository;
        SessionPermissionGroupRepository = sessionPermissionGroupRepository;
    }

    /// <summary>
    /// 获取会话角色权限
    /// </summary>
    /// <param name="sessionRoleId">会话角色Id</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpGet]
    public Task<Dictionary<string, PermissionGrantValue>> GetGrantedBySessionRoleAsync(Guid sessionRoleId)
    {
        throw new NotImplementedException();
    }

    protected virtual async Task<SessionPermissionGrantDto> GetGrantedItemAsync(SessionPermissionDefinition permissionDefinition, SessionUnit sessionUnit)
    {
        await Task.Yield();

        var roleGrants = sessionUnit.SessionUnitRoleList.SelectMany(x => x.SessionRole.GrantList.Where(d => d.DefinitionId == permissionDefinition.Id)).ToList();

        var unitGrants = sessionUnit.GrantList.Where(x => x.DefinitionId == permissionDefinition.Id).ToList();

        return new SessionPermissionGrantDto()
        {
            Definition = ObjectMapper.Map<SessionPermissionDefinition, SessionPermissionDefinitionDto>(permissionDefinition),
            RoleGrants = ObjectMapper.Map<List<SessionPermissionRoleGrant>, List<SessionPermissionRoleGrantDto>>(roleGrants),
            UnitGrants = ObjectMapper.Map<List<SessionPermissionUnitGrant>, List<SessionPermissionUnitGrantDto>>(unitGrants),
        };
    }

    /// <summary>
    /// 获取授予权限的会话单元
    /// </summary>
    /// <param name="permissionDefinitionId">权限Id</param>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<SessionPermissionGrantDto> GetGrantedBySessionUnitAsync(string permissionDefinitionId, Guid sessionUnitId)
    {
        var permissionDefinition = await SessionPermissionDefinitionRepository.GetAsync(permissionDefinitionId);
        var sessionUnit = await SessionUnitRepository.GetAsync(sessionUnitId);
        return await GetGrantedItemAsync(permissionDefinition, sessionUnit);
    }
    /// <summary>
    /// 获取所有授予权限的会话单元
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PagedResultDto<SessionPermissionGrantDto>> GetAllGrantedBySessionUnitAsync(Guid sessionUnitId)
    {
        var definitionList = await SessionPermissionDefinitionRepository.GetListAsync();
        var sessionUnit = await SessionUnitRepository.GetAsync(sessionUnitId);
        var items = new List<SessionPermissionGrantDto>();

        foreach (var definition in definitionList)
        {
            var item = await GetGrantedItemAsync(definition, sessionUnit);
            items.Add(item);
        }
        return new PagedResultDto<SessionPermissionGrantDto>(definitionList.Count, items);
    }

    /// <summary>
    /// 授予会话单元权限
    /// </summary>
    /// <param name="definitionId">权限Id</param>
    /// <param name="sessionUnitId">会话单元</param>
    /// <param name="permissionGrantValue">授予值</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<SessionPermissionUnitGrantDto> GrantBySessionUnitAsync(string definitionId, Guid sessionUnitId, PermissionGrantValue permissionGrantValue)
    {
        var permissionDefinition = await SessionPermissionDefinitionRepository.GetAsync(definitionId);

        var sessionUnit = await SessionUnitRepository.GetAsync(sessionUnitId);

        var sessionPermissionUnitGrant = sessionUnit.GrantList.FirstOrDefault(x => x.DefinitionId == definitionId);

        if (sessionPermissionUnitGrant == null)
        {
            sessionUnit.GrantList.Add(new SessionPermissionUnitGrant(
                definitionId: definitionId,
                sessionUnitId: sessionUnitId,
                value: permissionGrantValue.Value,
                isEnabled: permissionGrantValue.IsEnabled));
        }
        else
        {
            sessionPermissionUnitGrant.Value = permissionGrantValue.Value;
            sessionPermissionUnitGrant.IsEnabled = permissionGrantValue.IsEnabled;
        }
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<SessionPermissionUnitGrant, SessionPermissionUnitGrantDto>(sessionPermissionUnitGrant);
    }

    /// <summary>
    /// 授予会话角色权限
    /// </summary>
    /// <param name="definitionId">权限Id</param>
    /// <param name="sessionRoleId">角色Id</param>
    /// <param name="permissionGrantValue">授予值</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<SessionPermissionRoleGrantDto> GrantBySessionRoleAsync(string definitionId, Guid sessionRoleId, PermissionGrantValue permissionGrantValue)
    {
        var permissionDefinition = await SessionPermissionDefinitionRepository.GetAsync(definitionId);

        var sessionRole = await SessionRoleRepository.GetAsync(sessionRoleId);

        var sessionPermissionUnitGrant = sessionRole.GrantList.FirstOrDefault(x => x.DefinitionId == definitionId);

        if (sessionPermissionUnitGrant == null)
        {
            sessionRole.GrantList.Add(new SessionPermissionRoleGrant(
                definitionId: definitionId,
                roleId: sessionRoleId,
                value: permissionGrantValue.Value,
                isEnabled: permissionGrantValue.IsEnabled));
        }
        else
        {
            sessionPermissionUnitGrant.Value = permissionGrantValue.Value;
            sessionPermissionUnitGrant.IsEnabled = permissionGrantValue.IsEnabled;
        }
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<SessionPermissionRoleGrant, SessionPermissionRoleGrantDto>(sessionPermissionUnitGrant);
    }

    /// <summary>
    /// 获取权限定义(Tree)
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<List<SessionPermissionDefinitionTreeDto>> GetDefinitionsAsync()
    {
        var groupData = await SessionPermissionGroupRepository.GetListAsync();

        var permissionData = await SessionPermissionDefinitionRepository.GetListAsync();

        var list = await FindChildrenGroupAsync(groupData, permissionData, null);

        var ungroupList = await GetPermissionsAsync(permissionData, null);

        if (ungroupList.Count != 0)
        {
            list.Add(new SessionPermissionDefinitionTreeDto()
            {
                Id = $"groupid:{0}",
                IsGroup =true,
                 Title = "Ungrouped",
                 Description = "",
                 Children = ungroupList
            });
        }

        return list;
    }

    protected virtual async Task<List<SessionPermissionDefinitionTreeDto>> FindChildrenGroupAsync(List<SessionPermissionGroup> groupData, List<SessionPermissionDefinition> permissionData, long? parentId)
    {
        var ret = new List<SessionPermissionDefinitionTreeDto>();

        var items = groupData.Where(x => x.ParentId == parentId).ToList();

        foreach (var group in items)
        {
            var children = await FindChildrenGroupAsync(groupData, permissionData, group.Id);

            if (children.Count == 0)
            {
                children = await GetPermissionsAsync(permissionData, group.Id);
            }
            ret.Add(new SessionPermissionDefinitionTreeDto()
            {
                IsGroup = true,
                Id = $"groupid:{group.Id}",
                Title = group.Name,
                Children = children,
            });
        }
        return ret;
    }
    protected virtual async Task<List<SessionPermissionDefinitionTreeDto>> GetPermissionsAsync(List<SessionPermissionDefinition> permissionData, long? groupId)
    {
        await Task.Yield();

        var ret = new List<SessionPermissionDefinitionTreeDto>();

        var permissions = permissionData.Where(x => x.GroupId == groupId).ToList();

        foreach (var permission in permissions)
        {
            ret.Add(new SessionPermissionDefinitionTreeDto()
            {
                IsGroup = false,
                Id = permission.Id,
                Title = permission.Name,
                Description = permission.Description,
            });
        }
        return ret;
    }
}
