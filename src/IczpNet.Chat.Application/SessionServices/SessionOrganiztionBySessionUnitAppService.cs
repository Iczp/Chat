using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionOrganizationRoles;
using IczpNet.Chat.SessionOrganizations;
using IczpNet.Chat.SessionPermissions;
using IczpNet.Chat.SessionRoles;
using IczpNet.Chat.SessionSections.SessionOrganiztions;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionUnitOrganizations;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionServices;

public class SessionOrganiztionBySessionUnitAppService
    : CrudBySessionUnitTreeChatAppService<
        SessionOrganization,
        long,
        SessionOrganizationDetailDto,
        SessionOrganizationDto,
        SessionOrganizationGetListBySessionUnitInput,
        SessionOrganizationCreateBySessionUnitInput,
        SessionOrganizationUpdateInput>,
    ISessionOrganiztionBySessionUnitAppService
{
    protected override string GetPolicyName { get; set; } = Permissions.SessionPermissionDefinitionConsts.SessionOrganizationPermission.Default;
    protected override string GetListPolicyName { get; set; } = Permissions.SessionPermissionDefinitionConsts.SessionOrganizationPermission.Default;
    protected override string CreatePolicyName { get; set; } = Permissions.SessionPermissionDefinitionConsts.SessionOrganizationPermission.Create;
    protected override string UpdatePolicyName { get; set; } = Permissions.SessionPermissionDefinitionConsts.SessionOrganizationPermission.Update;
    protected override string DeletePolicyName { get; set; } = Permissions.SessionPermissionDefinitionConsts.SessionOrganizationPermission.Delete;
    protected override string DeleteManyPolicyName { get; set; } = Permissions.SessionPermissionDefinitionConsts.SessionOrganizationPermission.Delete;

    public SessionOrganiztionBySessionUnitAppService(
        IRepository<SessionOrganization, long> repository,
        ISessionOrganizationManager sessionOrganizationManager)
        : base(repository, sessionOrganizationManager)
    {
    }

    protected override async Task<IQueryable<SessionOrganization>> CreateFilteredQueryAsync(SessionUnit sessionUnit, SessionOrganizationGetListBySessionUnitInput input)
    {
        Assert.If(!input.IsEnabledParentId && input.ParentId.HasValue, "When [IsEnabledParentId]=false,then [ParentId] != null");

        return (await Repository.GetQueryableAsync())
            .Where(x => x.SessionId == sessionUnit.SessionId)
            .WhereIf(input.DepthList.IsAny(), (x) => input.DepthList.Contains(x.Depth))
            .WhereIf(input.IsEnabledParentId, (x) => x.ParentId == input.ParentId)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.Contains(input.Keyword));
    }

    protected override async Task<SessionOrganization> MapToEntityAsync(SessionUnit sessionUnit, SessionOrganizationCreateBySessionUnitInput createInput)
    {
        await Task.Yield();

        return new SessionOrganization(createInput.Name, sessionUnit.SessionId.Value, createInput.ParentId);
    }

    protected override Task CheckGetPolicyAsync(SessionUnit sessionUnit, SessionOrganization entity)
    {
        Assert.If(sessionUnit.SessionId != entity.SessionId, "Not in same session");

        return base.CheckGetPolicyAsync(sessionUnit, entity);
    }

    protected override async Task CheckCreatePolicyAsync(SessionUnit sessionUnit, SessionOrganizationCreateBySessionUnitInput input)
    {
        Assert.If(input.ParentId.HasValue && !await Repository.AnyAsync(x => x.Id == input.ParentId && x.SessionId == sessionUnit.SessionId), $"No such entity of ParentId:{input.ParentId}");

        await base.CheckCreatePolicyAsync(sessionUnit, input);
    }

    protected override async Task CheckUpdatePolicyAsync(SessionUnit sessionUnit, SessionOrganization entity, SessionOrganizationUpdateInput input)
    {
        Assert.If(sessionUnit.SessionId != entity.SessionId, $"Not in same session");

        if (input.ParentId.HasValue)
        {
            var perent = await Repository.GetAsync(input.ParentId.Value);
            Assert.If(perent.SessionId != entity.SessionId, $"Parent session is different, ParentId:{input.ParentId}");
        }

        await base.CheckUpdatePolicyAsync(sessionUnit, entity, input);
    }

    protected override Task CheckDeletePolicyAsync(SessionUnit sessionUnit, SessionOrganization entity)
    {
        Assert.If(sessionUnit.SessionId != entity.SessionId, $"Not in same session");

        return base.CheckDeletePolicyAsync(sessionUnit, entity);
    }

    protected override Expression<Func<SessionOrganization, bool>> GetPredicateDeleteManyAsync(SessionUnit sessionUnit)
    {
        return x => x.SessionId == sessionUnit.SessionId;
    }

    public override async Task<PagedResultDto<SessionOrganizationDto>> GetListAsync(SessionOrganizationGetListBySessionUnitInput input)
    {
        var result = await base.GetListAsync(input);
        await EnrichOrganizationsAsync(result.Items);
        return result;
    }

    private async Task EnrichOrganizationsAsync(IReadOnlyList<SessionOrganizationDto> items)
    {
        if (items == null || items.Count == 0)
        {
            return;
        }
        var orgIds = items.Select(x => x.Id).Distinct().ToList();
        var orgRoleRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<SessionOrganizationRole>>();
        var unitOrgRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<SessionUnitOrganization>>();

        var roleRows = await AsyncExecuter.ToListAsync(
            (await orgRoleRepo.GetQueryableAsync())
                .AsNoTracking()
                .Where(x => orgIds.Contains(x.SessionOrganizationId) && x.SessionRole != null)
                .Select(x => new
                {
                    x.SessionOrganizationId,
                    RoleId = x.SessionRole.Id,
                    RoleName = x.SessionRole.Name,
                }));

        var roleMap = roleRows
            .GroupBy(x => x.SessionOrganizationId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(r => new SessionRoleSimpleDto { Id = r.RoleId, Name = r.RoleName }).ToList());

        var memberCounts = await AsyncExecuter.ToListAsync(
            (await unitOrgRepo.GetQueryableAsync())
                .AsNoTracking()
                .Where(x => orgIds.Contains(x.SessionOrganizationId) && x.SessionUnit != null && x.SessionUnit.Setting.IsEnabled && !x.SessionUnit.Setting.IsKilled)
                .GroupBy(x => x.SessionOrganizationId)
                .Select(g => new { OrgId = g.Key, Count = g.Count() }));
        var countMap = memberCounts.ToDictionary(x => x.OrgId, x => x.Count);

        foreach (var item in items)
        {
            item.BoundRoles = roleMap.GetValueOrDefault(item.Id) ?? new();
            item.MemberCount = countMap.GetValueOrDefault(item.Id);
        }
    }

    [HttpPost("/api/chat/session-organiztion-by-session-unit/batch-set-members")]
    [UnitOfWork(isTransactional: true)]
    public virtual async Task<BatchSetMemberOrganizationsResultDto> BatchSetMembersAsync(BatchSetMemberOrganizationsInput input)
    {
        var operatorUnit = await SessionUnitManager.GetAsync(input.SessionUnitId);
        await CheckPolicyAsync(UpdatePolicyName, operatorUnit);

        var memberIds = (input.MemberSessionUnitIdList ?? new List<Guid>()).Distinct().ToList();
        if (memberIds.Count == 0 || memberIds.Count > 200)
        {
            throw new UserFriendlyException("单次批量操作成员数量必须在 1 到 200 人之间。");
        }

        var orgIds = (input.OrganizationIdList ?? new List<long>()).Distinct().ToList();
        if (orgIds.Count > 10)
        {
            throw new UserFriendlyException("单成员最多归属 10 个组织。");
        }

        switch (input.Mode)
        {
            case MemberOrganizationBatchMode.Add:
            case MemberOrganizationBatchMode.Remove:
                if (orgIds.Count == 0)
                {
                    throw new UserFriendlyException("请至少选择一个目标组织。");
                }
                break;
            case MemberOrganizationBatchMode.Replace:
                if (orgIds.Count == 0)
                {
                    throw new UserFriendlyException("重新设置组织时，目标组织不能为空；如需清空请使用「设为未分组」。");
                }
                break;
            case MemberOrganizationBatchMode.Clear:
                if (orgIds.Count > 0)
                {
                    throw new UserFriendlyException("设为未分组时不可传入目标组织。");
                }
                break;
        }

        var sessionUnitRepo = LazyServiceProvider.LazyGetRequiredService<ISessionUnitRepository>();
        var targetUnits = await AsyncExecuter.ToListAsync(
            (await sessionUnitRepo.GetQueryableAsync())
                .Include(x => x.Setting)
                .Where(x => memberIds.Contains(x.Id) && x.SessionId == operatorUnit.SessionId && x.Setting.IsEnabled && !x.Setting.IsKilled));

        if (targetUnits.Count != memberIds.Count)
        {
            throw new UserFriendlyException("部分目标成员不存在、已退群或不属于当前群聊。");
        }

        if (orgIds.Count > 0)
        {
            var validOrgCount = await Repository.CountAsync(x => orgIds.Contains(x.Id) && x.SessionId == operatorUnit.SessionId);
            if (validOrgCount != orgIds.Count)
            {
                throw new UserFriendlyException("部分目标组织不存在或不属于当前群聊。");
            }
        }

        var unitOrgRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<SessionUnitOrganization>>();
        var existingLinks = await AsyncExecuter.ToListAsync(
            (await unitOrgRepo.GetQueryableAsync())
                .Where(x => memberIds.Contains(x.SessionUnitId)));
        var existingByUnit = existingLinks.GroupBy(x => x.SessionUnitId).ToDictionary(g => g.Key, g => g.ToList());

        // 防提权校验：如果被调整的组织（新增或移出）绑定了角色，仅群主可执行
        var affectedOrgIds = new HashSet<long>(orgIds);
        if (input.Mode is MemberOrganizationBatchMode.Replace or MemberOrganizationBatchMode.Clear)
        {
            foreach (var link in existingLinks)
            {
                affectedOrgIds.Add(link.SessionOrganizationId);
            }
        }

        if (affectedOrgIds.Count > 0 && !operatorUnit.Setting.IsCreator)
        {
            var orgRoleRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<SessionOrganizationRole>>();
            var hasBoundRole = await orgRoleRepo.AnyAsync(x => affectedOrgIds.Contains(x.SessionOrganizationId));
            if (hasBoundRole)
            {
                throw new UserFriendlyException("所选组织或成员当前所属组织已绑定角色，为防止越权提权/降权，仅群主可调整该成员的组织归属。");
            }
        }

        var toInsert = new List<SessionUnitOrganization>();
        var toDelete = new List<SessionUnitOrganization>();

        foreach (var unit in targetUnits)
        {
            var currentLinks = existingByUnit.GetValueOrDefault(unit.Id) ?? new List<SessionUnitOrganization>();
            var currentOrgSet = currentLinks.Select(x => x.SessionOrganizationId).ToHashSet();

            switch (input.Mode)
            {
                case MemberOrganizationBatchMode.Add:
                    {
                        var addedIds = orgIds.Where(id => !currentOrgSet.Contains(id)).ToList();
                        if (currentOrgSet.Count + addedIds.Count > 10)
                        {
                            throw new UserFriendlyException($"成员「{unit.Setting?.MemberName ?? unit.Id.ToString()}」的所属组织数量将超过上限（10 个）。");
                        }
                        foreach (var oid in addedIds)
                        {
                            toInsert.Add(new SessionUnitOrganization(oid, unit.Id));
                        }
                        break;
                    }
                case MemberOrganizationBatchMode.Remove:
                    {
                        var removeSet = orgIds.ToHashSet();
                        toDelete.AddRange(currentLinks.Where(x => removeSet.Contains(x.SessionOrganizationId)));
                        break;
                    }
                case MemberOrganizationBatchMode.Replace:
                    {
                        var targetSet = orgIds.ToHashSet();
                        toDelete.AddRange(currentLinks.Where(x => !targetSet.Contains(x.SessionOrganizationId)));
                        foreach (var oid in orgIds.Where(id => !currentOrgSet.Contains(id)))
                        {
                            toInsert.Add(new SessionUnitOrganization(oid, unit.Id));
                        }
                        break;
                    }
                case MemberOrganizationBatchMode.Clear:
                    {
                        toDelete.AddRange(currentLinks);
                        break;
                    }
            }
        }

        if (toDelete.Count > 0)
        {
            await unitOrgRepo.DeleteManyAsync(toDelete, autoSave: true);
        }
        if (toInsert.Count > 0)
        {
            await unitOrgRepo.InsertManyAsync(toInsert, autoSave: true);
        }

        return new BatchSetMemberOrganizationsResultDto
        {
            AffectedMemberCount = targetUnits.Count,
            UpdatedMemberSessionUnitIdList = targetUnits.Select(x => x.Id).ToList()
        };
    }

    [HttpGet("/api/chat/session-organiztion-by-session-unit/organization-roles")]
    public virtual async Task<List<SessionRoleSimpleDto>> GetOrganizationRolesAsync(Guid sessionUnitId, long organizationId)
    {
        var operatorUnit = await SessionUnitManager.GetAsync(sessionUnitId);
        await CheckPolicyAsync(GetPolicyName, operatorUnit);

        var org = await Repository.GetAsync(organizationId);
        Assert.If(org.SessionId != operatorUnit.SessionId, "Not in same session");

        var orgRoleRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<SessionOrganizationRole>>();
        return await AsyncExecuter.ToListAsync(
            (await orgRoleRepo.GetQueryableAsync())
                .AsNoTracking()
                .Where(x => x.SessionOrganizationId == organizationId && x.SessionRole != null)
                .Select(x => new SessionRoleSimpleDto { Id = x.SessionRole.Id, Name = x.SessionRole.Name }));
    }

    [HttpPost("/api/chat/session-organiztion-by-session-unit/set-organization-roles")]
    [UnitOfWork(isTransactional: true)]
    public virtual async Task<List<SessionRoleSimpleDto>> SetOrganizationRolesAsync(SetOrganizationRolesInput input)
    {
        var operatorUnit = await SessionUnitManager.GetAsync(input.SessionUnitId);
        if (!operatorUnit.Setting.IsCreator)
        {
            throw new UserFriendlyException("仅群主可配置组织绑定的角色。");
        }

        var org = await Repository.GetAsync(input.OrganizationId);
        Assert.If(org.SessionId != operatorUnit.SessionId, "Not in same session");

        var roleIds = (input.RoleIdList ?? new List<Guid>()).Distinct().ToList();
        var roleRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<SessionRole, Guid>>();
        var roles = roleIds.Count > 0
            ? await AsyncExecuter.ToListAsync(
                (await roleRepo.GetQueryableAsync())
                    .Where(x => roleIds.Contains(x.Id) && x.SessionId == operatorUnit.SessionId))
            : new List<SessionRole>();

        if (roles.Count != roleIds.Count)
        {
            throw new UserFriendlyException("部分角色不存在或不属于当前群聊。");
        }

        if (roles.Any(r => string.Equals(r.Name, "管理员", StringComparison.OrdinalIgnoreCase)
            || string.Equals(r.Name, "Admin", StringComparison.OrdinalIgnoreCase)))
        {
            throw new UserFriendlyException("内置管理员角色不可直接绑定到组织。");
        }

        var orgRoleRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<SessionOrganizationRole>>();
        var existing = await AsyncExecuter.ToListAsync(
            (await orgRoleRepo.GetQueryableAsync())
                .Where(x => x.SessionOrganizationId == input.OrganizationId));

        if (existing.Count > 0)
        {
            await orgRoleRepo.DeleteManyAsync(existing, autoSave: true);
        }

        if (roleIds.Count > 0)
        {
            var links = roleIds.Select(rid => new SessionOrganizationRole(input.OrganizationId, rid)).ToList();
            await orgRoleRepo.InsertManyAsync(links, autoSave: true);
        }

        return roles.Select(r => new SessionRoleSimpleDto { Id = r.Id, Name = r.Name }).ToList();
    }

    [HttpGet("/api/chat/session-organiztion-by-session-unit/member-permission-sources")]
    public virtual async Task<MemberPermissionSourcesDto> GetMemberPermissionSourcesAsync(Guid sessionUnitId, Guid targetSessionUnitId)
    {
        var operatorUnit = await SessionUnitManager.GetAsync(sessionUnitId);
        if (!operatorUnit.Setting.IsCreator && !operatorUnit.IsAdmin)
        {
            await CheckPolicyAsync(SessionPermissionDefinitionConsts.SessionRolePermission.Default, operatorUnit);
        }

        var sessionUnitRepo = LazyServiceProvider.LazyGetRequiredService<ISessionUnitRepository>();
        var target = await AsyncExecuter.FirstOrDefaultAsync(
            (await sessionUnitRepo.GetQueryableAsync())
                .Include(x => x.Setting)
                .Include(x => x.GrantList)
                .Include(x => x.SessionUnitRoleList).ThenInclude(r => r.SessionRole).ThenInclude(sr => sr.GrantList)
                .Include(x => x.SessionUnitOrganizationList).ThenInclude(o => o.SessionOrganization)
                .Where(x => x.Id == targetSessionUnitId && x.SessionId == operatorUnit.SessionId));

        if (target == null)
        {
            throw new UserFriendlyException("目标成员不存在或不属于当前群聊。");
        }

        var orgIds = target.SessionUnitOrganizationList?.Select(x => x.SessionOrganizationId).Distinct().ToList() ?? new List<long>();
        var orgRoleRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<SessionOrganizationRole>>();
        var orgRoles = orgIds.Count > 0
            ? await AsyncExecuter.ToListAsync(
                (await orgRoleRepo.GetQueryableAsync())
                    .Include(x => x.SessionOrganization)
                    .Include(x => x.SessionRole).ThenInclude(r => r.GrantList)
                    .Where(x => orgIds.Contains(x.SessionOrganizationId) && x.SessionRole != null))
            : new List<SessionOrganizationRole>();

        var allDefs = SessionPermissionDefinitionConsts.GetAll();
        var permItems = new List<MemberPermissionSourceItemDto>();

        foreach (var defId in allDefs)
        {
            var fromCreator = target.Setting?.IsCreator ?? false;
            var fromAdminTemplate = target.IsAdmin && global::IczpNet.Chat.SessionPermissions.SessionPermissionChecker.AdminDefaultPermissions.Contains(defId);
            var fromDirectGrant = target.GrantList?.Any(g => g.IsEnabled && g.DefinitionId == defId) ?? false;
            var directRoleNames = target.SessionUnitRoleList?
                .Where(r => r.SessionRole != null && r.SessionRole.GrantList != null && r.SessionRole.GrantList.Any(g => g.IsEnabled && g.DefinitionId == defId))
                .Select(r => r.SessionRole.Name)
                .Distinct()
                .ToList() ?? new List<string>();
            var orgRoleSources = orgRoles
                .Where(or => or.SessionRole?.GrantList != null && or.SessionRole.GrantList.Any(g => g.IsEnabled && g.DefinitionId == defId))
                .Select(or => $"{or.SessionOrganization?.Name ?? or.SessionOrganizationId.ToString()} -> {or.SessionRole.Name}")
                .Distinct()
                .ToList();

            var isGranted = fromCreator || fromAdminTemplate || fromDirectGrant || directRoleNames.Count > 0 || orgRoleSources.Count > 0;
            if (isGranted)
            {
                permItems.Add(new MemberPermissionSourceItemDto
                {
                    DefinitionId = defId,
                    IsGranted = true,
                    FromCreator = fromCreator,
                    FromAdminTemplate = fromAdminTemplate,
                    FromDirectGrant = fromDirectGrant,
                    DirectRoleNames = directRoleNames,
                    OrganizationRoleSources = orgRoleSources
                });
            }
        }

        return new MemberPermissionSourcesDto
        {
            TargetSessionUnitId = target.Id,
            IsCreator = target.Setting?.IsCreator ?? false,
            IsAdmin = target.IsAdmin,
            DirectRoles = target.SessionUnitRoleList?
                .Where(r => r.SessionRole != null)
                .Select(r => new SessionRoleSimpleDto { Id = r.SessionRole.Id, Name = r.SessionRole.Name })
                .ToList() ?? new List<SessionRoleSimpleDto>(),
            Organizations = target.SessionUnitOrganizationList?
                .Where(o => o.SessionOrganization != null)
                .Select(o => new SessionOrganizationDto
                {
                    Id = o.SessionOrganization.Id,
                    Name = o.SessionOrganization.Name,
                    ParentId = o.SessionOrganization.ParentId,
                    FullPath = o.SessionOrganization.FullPath,
                    FullPathName = o.SessionOrganization.FullPathName,
                    Depth = o.SessionOrganization.Depth,
                })
                .ToList() ?? new List<SessionOrganizationDto>(),
            Permissions = permItems
        };
    }
}
