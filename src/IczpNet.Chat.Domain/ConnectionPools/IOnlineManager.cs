using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.ConnectionPools;

public interface IOnlineManager //: IConnectionPoolManager
{
    /// <summary>
    /// 添加连接
    /// </summary>
    /// <param name="connectionPool"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> ConnectedAsync(ConnectionPoolCacheItem connectionPool, CancellationToken token = default);

    /// <summary>
    /// 设置活跃时间
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<ConnectionPoolCacheItem> UpdateActiveTimeAsync(string connectionId, CancellationToken token = default);

    /// <summary>
    /// 移除连接
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> DisconnectedAsync(string connectionId, CancellationToken token = default);

    /// <summary>
    /// 删除主机所有连接
    /// </summary>
    /// <param name="hostHame"></param>
    /// <returns></returns>
    Task<long> DeleteByHostNameAsync(string hostHame);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StopAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 获取连接信息
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<ConnectionPoolCacheItem> GetAsync(string connectionId, CancellationToken token = default);

    /// <summary>
    /// 获取连接信息(多个)
    /// </summary>
    /// <param name="connectionIds"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<KeyValuePair<string, ConnectionPoolCacheItem>[]> GetManyAsync(IEnumerable<string> connectionIds, CancellationToken token = default);

    /// <summary>
    /// 是否在线（用户）
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> IsOnlineAsync(Guid userId, CancellationToken token = default);

    /// <summary>
    /// 是否在线（聊天对象）
    /// </summary>
    /// <param name="ownertId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> IsOnlineAsync(long ownertId, CancellationToken token = default);

    /// <summary>
    /// 是否在线（聊天对象）
    /// </summary>
    /// <param name="ownerIds"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Dictionary<long, bool>> IsOnlineAsync(IEnumerable<long> ownerIds, CancellationToken token = default);

    /// <summary>
    /// 获取最后在线时间（聊天对象）
    /// </summary>
    /// <param name="ownertId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<IEnumerable<OwnerLatestOnline>> GetLatestOnlineAsync(long ownertId, CancellationToken token = default);

    /// <summary>
    /// 获取设备类型 聊天对象
    /// </summary>
    /// <param name="ownertId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<IEnumerable<string>> GetDeviceTypesAsync(long ownertId, CancellationToken token = default);

    /// <summary>
    /// 获取设备类型 聊天对象
    /// </summary>
    /// <param name="ownerIds"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Dictionary<long, IEnumerable<string>>> GetDeviceTypesAsync(List<long> ownerIds, CancellationToken token = default);

    /// <summary>
    /// 获取设备信息(DeviceId,DeviceType) 聊天对象
    /// </summary>
    /// <param name="ownerIds"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Dictionary<long, IEnumerable<DeviceModel>>> GetDevicesAsync(List<long> ownerIds, CancellationToken token = default);


    /// <summary>
    /// 获取设备类型 用户
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Dictionary<long, IEnumerable<DeviceModel>>> GetDevicesByUserAsync(Guid userId, CancellationToken token = default);


    /// <summary>
    /// 获取连接Id
    /// </summary>
    /// <param name="ownerIds"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Dictionary<long, IEnumerable<string>>> GetConnectionIdsByOwnerAsync(List<long> ownerIds, CancellationToken token = default);

    /// <summary>
    /// 获取连接信息
    /// </summary>
    /// <param name="ownerIds"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Dictionary<long, IEnumerable<ConnectionPoolCacheItem>>> GetConnectionsByOwnerAsync(List<long> ownerIds, CancellationToken token = default);

    /// <summary>
    /// 获取连接数量(用户)
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<long> GetCountByUserAsync(Guid userId, CancellationToken token = default);

    /// <summary>
    /// 获取连接(用户)
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<IEnumerable<string>> GetConnectionIdsByUserAsync(Guid userId, CancellationToken token = default);

    /// <summary>
    /// 获取连接数量(聊天对象)
    /// </summary>
    /// <param name="ownertId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<long> GetCountByOwnerAsync(long ownertId, CancellationToken token = default);

    /// <summary>
    /// 获取会话连接
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="token"></param>
    /// <returns>Dictionary: key = connId, value = chatObjectList</returns>
    Task<Dictionary<string, List<long>>> GetConnectionsBySessionAsync(Guid sessionId, CancellationToken token = default);

    /// <summary>
    /// 获取会话在线人数
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<long> GetCountBySessionAsync(Guid sessionId, CancellationToken token = default);

    /// <summary>
    /// 添加到连接池
    /// </summary>
    /// <param name="ownerSessions"></param>
    /// <returns></returns>
    Task AddSessionAsync(List<(Guid SessionId, long OwnerId)> ownerSessions);

    /// <summary>
    /// 获取主机最后在线时间
    /// </summary>
    /// <returns></returns>
    Task<Dictionary<string, DateTime?>> GetAllHostsAsync();

    /// <summary>
    /// 获取总连接数
    /// </summary>
    /// <returns></returns>
    Task<long> GetTotalCountAsync();

    /// <summary>
    /// 获取主机在线连接数
    /// </summary>
    /// <param name="hosts"></param>
    /// <returns></returns>
    Task<Dictionary<string, long>> GetCountByHostsAsync(IEnumerable<string> hosts = null);

    /// <summary>
    /// 获取连接数量(主机)
    /// </summary>
    /// <param name="host"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<long> GetCountByHostAsync(string host, CancellationToken token = default);

    /// <summary>
    /// 获取连接(主机)
    /// </summary>
    /// <param name="host"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<IEnumerable<string>> GetConnectionIdsByHostAsync(string host, CancellationToken token = default);

    /// <summary>
    /// 获取在线好友数量
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    Task<long> GetOnlineFriendsCountAsync(long ownerId);

    /// <summary>
    /// 获取在线好友
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    Task<IEnumerable<OnlineFriendInfo>> GetOnlineFriendsAsync(long ownerId);

    /// <summary>
    /// 获取在线好友连接列表
    /// </summary>
    /// <param name="ownerIds"></param>
    /// <returns></returns>
    Task<Dictionary<long, IEnumerable<string>>> GetOnlineFriendsConnectionIdsAsync(List<long> ownerIds);
}
