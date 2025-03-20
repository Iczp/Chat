using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Clients;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace IczpNet.Chat.SessionSections.SessionPermissions;

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

    public async Task<bool> IsGrantedAsync(string sessionPermissionDefinitionId, SessionUnit sessionUnit)
    {
        await Task.Yield();

        //if (!CurrentUser.GetChatObjectIdList().Contains(sessionUnit.SessionUnitId))
        //{
        //    return false;
        //}

        Assert.If(!SessionPermissionDefinitionConsts.GetAll().Contains(sessionPermissionDefinitionId), $"Key does not exist:{sessionPermissionDefinitionId}");

        if (!sessionUnit.Setting.IsEnabled)
        {
            Logger.LogDebug($"SessionUnit is disabled, sessionUnitId:{sessionUnit.Id}, sessionPermissionDefinitionId:{sessionPermissionDefinitionId},");
            return false;
        }

        if (sessionUnit.Setting.IsStatic)
        {
            Logger.LogDebug($"SessionUnit is Static, sessionUnitId:{sessionUnit.Id}, sessionPermissionDefinitionId:{sessionPermissionDefinitionId},");
            return true;
        }

        if (sessionUnit.Setting.IsCreator)
        {
            Logger.LogDebug($"SessionUnit is Creator, sessionUnitId:{sessionUnit.Id}, sessionPermissionDefinitionId:{sessionPermissionDefinitionId},");
            return true;
        }

        //UnitGrant
        if (sessionUnit.GrantList.Any(d => d.IsEnabled && d.DefinitionId == sessionPermissionDefinitionId))
        {
            Logger.LogDebug($"Role Permission IsGranted:{sessionPermissionDefinitionId}");
            return true;
        }

        //RoleGrant
        if (sessionUnit.SessionUnitRoleList.Any(x => x.SessionRole.GrantList.Any(d => d.IsEnabled && d.DefinitionId == sessionPermissionDefinitionId)))
        {
            Logger.LogDebug($"SessionUnit Permission IsGranted:{sessionPermissionDefinitionId}");
            return true;
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
