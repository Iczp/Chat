using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionUnits;

/// <inheritdoc />
public class FriendManager(
        IDistributedCache<List<SessionUnitCacheItem>, long> friendsCache,
    IDistributedCache<List<SessionUnitCacheItem>, Guid> userFriendsCache,
    ISessionUnitManager sessionUnitManager
    ) : DomainService, IFriendManager
{

    public IDistributedCache<List<SessionUnitCacheItem>, long> FriendsCache { get; } = friendsCache;
    public IDistributedCache<List<SessionUnitCacheItem>, Guid> UserFriendsCache { get; } = userFriendsCache;
    public ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;


    /// <inheritdoc />
    public virtual Task<List<SessionUnitCacheItem>> GetFriendsAsync(Guid userId)
    {
        return UserFriendsCache.GetOrAddAsync(userId, () => SessionUnitManager.GetListByUserIdAsync(userId));
    }

    /// <inheritdoc />
    public virtual Task<List<SessionUnitCacheItem>> GetFriendsAsync(long chatObjectId)
    {
        return FriendsCache.GetOrAddAsync(chatObjectId, () => SessionUnitManager.GetListByOwnerIdAsync(chatObjectId));
    }
}
