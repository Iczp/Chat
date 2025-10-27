using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.Xml.Schema;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Friends;

/// <inheritdoc />
public class FriendsManager(
    IDistributedCache<List<SessionUnitCacheItem>, long> friendsCache,
    IDistributedCache<List<SessionUnitCacheItem>, Guid> userFriendsCache,
    IConnectionPoolManager connectionPoolManager,
    ISessionUnitManager sessionUnitManager
    ) : DomainService, IFriendsManager
{

    public IDistributedCache<List<SessionUnitCacheItem>, long> FriendsCache { get; } = friendsCache;
    public IDistributedCache<List<SessionUnitCacheItem>, Guid> UserFriendsCache { get; } = userFriendsCache;
    public IConnectionPoolManager ConnectionPoolManager { get; } = connectionPoolManager;
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

    public async Task<List<FriendStatus>> GetListOnlineAsync(Guid userId)
    {
        var friendList = await GetFriendsAsync(userId);

        // 注意朋友是： DestinationId 不是 OwnerId
        var firendChatObjectIds = friendList.Select(x => x.DestinationId).Distinct().ToList();

        var onlineList = (await ConnectionPoolManager.CreateQueryableAsync())
            .Where(x => x.UserId != userId)
            .Where(x => x.ChatObjectIdList.Any(d => firendChatObjectIds.Contains(d)))
            .ToList();

        var list = onlineList.GroupBy(x => x.ChatObjectIdList.JoinAsString(","))
             .Select(x => new FriendStatus()
             {
                 UserId = x.FirstOrDefault().UserId,
                 ChatObjectIdList = x.Key.Split(",").Select(x => long.Parse(x)).ToList(),
                 LastActiveTime = x.Max(d => d.ActiveTime),
                 DeviceTypes = x.Select(d => d.DeviceType).Distinct().ToList(),
             })
             .Distinct()
             .ToList();

        return list;
    }
}
