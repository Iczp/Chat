using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionOrganizationRoles;
using IczpNet.Chat.SessionPermissionDefinitions;
using IczpNet.Chat.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionUnitOrganizations;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Clients;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace IczpNet.Chat.SessionPermissions;

public class SessionPermissionChecker : DomainService, ISessionPermissionChecker
{
    protected ISessionUnitManager SessionUnitManager { get; }
    protected ISessionPermissionDefinitionRepository Repository { get; }
    protected IRepository<SessionPermissionRoleGrant> SessionPermissionRoleGrantRepository { get; }
    protected ICurrentClient CurrentClient { get; }
    protected ICurrentUser CurrentUser { get; }
    public SessionPermissionChecker(ISessionUnitManager sessionUnitManager,
        ISessionPermissionDefinitionRepository sessionPermissionDefinitionRepository,
        ICurrentClient currentClient,
        ICurrentUser currentUser)
    {
        SessionUnitManager = sessionUnitManager;
        Repository = sessionPermissionDefinitionRepository;
        CurrentClient = currentClient;
        CurrentUser = currentUser;
    }


    public async Task CheckAsync(string sessionPermissionDefinitionId, SessionUnit sessionUnit)
    {
        var definition = await Repository.GetAsync(sessionPermissionDefinitionId);

        if (!definition.IsEnabled)
        {
            return;
        }

        Assert.If(!await IsGrantedAsync(sessionPermissionDefinitionId, sessionUnit),
            message: $"No permission:{definition.Name}",
            code: $"Permission:{sessionPermissionDefinitionId}");
    }

    /// <summary>
    /// 管理员受限默认权限模板（排除角色授权、组织绑定角色、转让群主、解散群及任命管理员）
    /// </summary>
    public static readonly IReadOnlySet<string> AdminDefaultPermissions = new HashSet<string>(StringComparer.Ordinal)
    {
        SessionPermissionDefinitionConsts.AnnouncementPermission.Default,
        SessionPermissionDefinitionConsts.AnnouncementPermission.Create,
        SessionPermissionDefinitionConsts.AnnouncementPermission.Publish,
        SessionPermissionDefinitionConsts.AnnouncementPermission.Approve,
        SessionPermissionDefinitionConsts.AnnouncementPermission.Update,
        SessionPermissionDefinitionConsts.AnnouncementPermission.UpdateOwn,
        SessionPermissionDefinitionConsts.AnnouncementPermission.UpdateAny,
        SessionPermissionDefinitionConsts.AnnouncementPermission.Delete,
        SessionPermissionDefinitionConsts.AnnouncementPermission.DeleteOwn,
        SessionPermissionDefinitionConsts.AnnouncementPermission.DeleteAny,
        SessionPermissionDefinitionConsts.AnnouncementPermission.Pin,
        SessionPermissionDefinitionConsts.AnnouncementPermission.Remind,
        SessionPermissionDefinitionConsts.AnnouncementPermission.ViewStat,
        SessionPermissionDefinitionConsts.SessionRequestPermission.Default,
        SessionPermissionDefinitionConsts.SessionRequestPermission.Handle,
        SessionPermissionDefinitionConsts.SessionOrganizationPermission.Default,
        SessionPermissionDefinitionConsts.SessionOrganizationPermission.Create,
        SessionPermissionDefinitionConsts.SessionOrganizationPermission.Update,
        SessionPermissionDefinitionConsts.SessionOrganizationPermission.Delete,
        SessionPermissionDefinitionConsts.SessionUnitPermissions.Default,
        SessionPermissionDefinitionConsts.SessionUnitPermissions.RemindEveryone,
        SessionPermissionDefinitionConsts.SessionUnitPermissions.RemoveMember,
        SessionPermissionDefinitionConsts.SessionUnitPermissions.RollbackOthersMessage,
        SessionPermissionDefinitionConsts.SessionUnitPermissions.ToggleInputEnabled,
        SessionPermissionDefinitionConsts.SessionUnitPermissions.SetMuteExpireTime,
        SessionPermissionDefinitionConsts.ChatObjectPermission.Default,
        SessionPermissionDefinitionConsts.ChatObjectPermission.UpdateName,
        SessionPermissionDefinitionConsts.ChatObjectPermission.UpdatePortrait,
        SessionPermissionDefinitionConsts.ChatObjectPermission.ToggleAllInputEnabled,
        SessionPermissionDefinitionConsts.ChatObjectPermission.VerificationMethod,
        SessionPermissionDefinitionConsts.SessionTagPermission.Default,
        SessionPermissionDefinitionConsts.SessionTagPermission.Create,
        SessionPermissionDefinitionConsts.SessionTagPermission.Update,
        SessionPermissionDefinitionConsts.SessionTagPermission.Delete,
    };

    public async Task<bool> IsGrantedAsync(string sessionPermissionDefinitionId, SessionUnit sessionUnit)
    {
        await Task.Yield();

        Assert.If(!SessionPermissionDefinitionConsts.GetAll().Contains(sessionPermissionDefinitionId), $"Key does not exist:{sessionPermissionDefinitionId}");

        if (!sessionUnit.Setting.IsEnabled || sessionUnit.Setting.IsKilled)
        {
            Logger.LogDebug($"SessionUnit is disabled or killed, sessionUnitId:{sessionUnit.Id}, sessionPermissionDefinitionId:{sessionPermissionDefinitionId},");
            return false;
        }

        if (sessionUnit.Setting.IsStatic && sessionUnit.OwnerId == sessionUnit.DestinationId)
        {
            Logger.LogDebug($"SessionUnit is system Static, sessionUnitId:{sessionUnit.Id}, sessionPermissionDefinitionId:{sessionPermissionDefinitionId},");
            return true;
        }

        if (sessionUnit.Setting.IsCreator)
        {
            Logger.LogDebug($"SessionUnit is Creator, sessionUnitId:{sessionUnit.Id}, sessionPermissionDefinitionId:{sessionPermissionDefinitionId},");
            return true;
        }

        if (sessionUnit.IsAdmin && AdminDefaultPermissions.Contains(sessionPermissionDefinitionId))
        {
            Logger.LogDebug($"SessionUnit is Admin (default template), sessionUnitId:{sessionUnit.Id}, sessionPermissionDefinitionId:{sessionPermissionDefinitionId},");
            return true;
        }

        //UnitGrant
        if (sessionUnit.GrantList != null && sessionUnit.GrantList.Any(d => d.IsEnabled && d.DefinitionId == sessionPermissionDefinitionId))
        {
            Logger.LogDebug($"Unit Permission IsGranted:{sessionPermissionDefinitionId}");
            return true;
        }

        //RoleGrant
        if (sessionUnit.SessionUnitRoleList != null && sessionUnit.SessionUnitRoleList.Any(x => x.SessionRole?.GrantList != null && x.SessionRole.GrantList.Any(d => d.IsEnabled && d.DefinitionId == sessionPermissionDefinitionId)))
        {
            Logger.LogDebug($"Role Permission IsGranted:{sessionPermissionDefinitionId}");
            return true;
        }

        //OrganizationRoleGrant (仅直接所属组织，首版不向子组织继承)
        List<long> directOrgIds = null;
        if (sessionUnit.SessionUnitOrganizationList != null)
        {
            directOrgIds = sessionUnit.SessionUnitOrganizationList
                .Select(x => x.SessionOrganizationId)
                .Distinct()
                .ToList();
        }
        else
        {
            var unitOrgRepo = LazyServiceProvider.LazyGetService<IRepository<SessionUnitOrganization>>();
            if (unitOrgRepo != null)
            {
                var unitOrgQuery = await unitOrgRepo.GetQueryableAsync();
                directOrgIds = await AsyncExecuter.ToListAsync(
                    unitOrgQuery.Where(x => x.SessionUnitId == sessionUnit.Id)
                        .Select(x => x.SessionOrganizationId)
                        .Distinct());
            }
        }

        if (directOrgIds != null && directOrgIds.Count > 0)
        {
            var orgRoleRepo = LazyServiceProvider.LazyGetService<IRepository<SessionOrganizationRole>>();
            if (orgRoleRepo != null)
            {
                var orgRoleQuery = await orgRoleRepo.GetQueryableAsync();
                var grantedByOrg = await AsyncExecuter.AnyAsync(
                    orgRoleQuery.Where(x =>
                        directOrgIds.Contains(x.SessionOrganizationId) &&
                        x.SessionRole != null &&
                        x.SessionRole.GrantList.Any(g => g.IsEnabled && g.DefinitionId == sessionPermissionDefinitionId)));
                if (grantedByOrg)
                {
                    Logger.LogDebug($"Organization Role Permission IsGranted:{sessionPermissionDefinitionId}");
                    return true;
                }
            }
        }

        Logger.LogDebug($"Not granted permission:{sessionPermissionDefinitionId}");

        return false;
    }

    public virtual async Task CheckAsync(string sessionPermissionDefinitionId, ChatObject chatObject)
    {
        await Task.Yield();

        var definition = await Repository.GetAsync(sessionPermissionDefinitionId);

        Assert.If(!await IsGrantedAsync(sessionPermissionDefinitionId, chatObject),
           message: $"No permission:{definition.Name}",
           code: $"Permission:{sessionPermissionDefinitionId}");
    }

    public async Task<bool> IsGrantedAsync(string sessionPermissionDefinitionId, ChatObject chatObject)
    {
        await Task.Yield();

        if (!chatObject.IsEnabled)
        {
            Logger.LogDebug($"ChatObject is disabled, Id:{chatObject.Id}, sessionPermissionDefinitionId:{sessionPermissionDefinitionId},");
            return false;
        }

        return CurrentUser.GetChatObjectIdList().Contains(chatObject.Id);
    }

    public virtual async Task<bool> IsLoginAsync(ChatObject chatObject)
    {
        await Task.Yield();

        return CurrentUser.GetChatObjectIdList().Contains(chatObject.Id);
    }

    public virtual async Task CheckLoginAsync(ChatObject chatObject)
    {
        Assert.If(!CurrentUser.Id.HasValue, message: $"Not Login:{chatObject.Id}", code: $"NotLogin");

        Assert.If(!await IsLoginAsync(chatObject),
        message: $"ChatObjectId:{chatObject.Id} is unbound CurrentUserId:{CurrentUser.Id}",
        code: $"Unbound");
    }
}
