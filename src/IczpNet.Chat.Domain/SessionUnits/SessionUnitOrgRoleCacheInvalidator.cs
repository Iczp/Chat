using System;
using System.Threading.Tasks;
using IczpNet.Chat.SessionOrganizations;
using IczpNet.Chat.SessionRoles;
using IczpNet.Chat.SessionUnitOrganizations;
using IczpNet.Chat.SessionUnitRoles;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// 当会话组织架构、群角色或成员组织/角色绑定发生增删改时，推进该会话的组织角色缓存版本号，
/// 使群成员列表的 OrganizationList / RoleList 缓存实现 O(1) 实时失效，同时保留数据库为唯一真相源。
/// </summary>
public class SessionUnitOrgRoleCacheInvalidator(
    IDistributedCache<string> versionCache,
    ISessionUnitManager sessionUnitManager,
    IUnitOfWorkManager unitOfWorkManager) :
    ILocalEventHandler<EntityChangedEventData<SessionOrganization>>,
    ILocalEventHandler<EntityChangedEventData<SessionRole>>,
    ILocalEventHandler<EntityChangedEventData<SessionUnitOrganization>>,
    ILocalEventHandler<EntityChangedEventData<SessionUnitRole>>,
    ITransientDependency
{
    private static readonly DistributedCacheEntryOptions VersionOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
    };

    public Task HandleEventAsync(EntityChangedEventData<SessionOrganization> eventData)
        => BumpSessionVersionAsync(eventData.Entity?.SessionId);

    public Task HandleEventAsync(EntityChangedEventData<SessionRole> eventData)
        => BumpSessionVersionAsync(eventData.Entity?.SessionId);

    public async Task HandleEventAsync(EntityChangedEventData<SessionUnitOrganization> eventData)
    {
        if (eventData.Entity == null) return;
        var sessionId = eventData.Entity.SessionOrganization?.SessionId
            ?? eventData.Entity.SessionUnit?.SessionId
            ?? (await TryResolveSessionIdAsync(eventData.Entity.SessionUnitId));
        await BumpSessionVersionAsync(sessionId);
    }

    public async Task HandleEventAsync(EntityChangedEventData<SessionUnitRole> eventData)
    {
        if (eventData.Entity == null) return;
        var sessionId = eventData.Entity.SessionRole?.SessionId
            ?? eventData.Entity.SessionUnit?.SessionId
            ?? (await TryResolveSessionIdAsync(eventData.Entity.SessionUnitId));
        await BumpSessionVersionAsync(sessionId);
    }

    private async Task<Guid?> TryResolveSessionIdAsync(Guid sessionUnitId)
    {
        if (sessionUnitId == Guid.Empty) return null;
        try
        {
            var cached = await sessionUnitManager.GetCacheAsync(sessionUnitId);
            return cached?.SessionId;
        }
        catch
        {
            return null;
        }
    }

    private async Task BumpSessionVersionAsync(Guid? sessionId)
    {
        if (!sessionId.HasValue || sessionId.Value == Guid.Empty) return;
        var targetSessionId = sessionId.Value;
        var newVer = Guid.NewGuid().ToString("N");
        var key = SessionUnitOrgRoleCacheKeys.VersionKey(targetSessionId);

        if (unitOfWorkManager.Current != null)
        {
            unitOfWorkManager.Current.OnCompleted(async () =>
            {
                await versionCache.SetAsync(key, newVer, VersionOptions, hideErrors: true);
            });
            return;
        }

        await versionCache.SetAsync(key, newVer, VersionOptions, hideErrors: true);
    }
}
