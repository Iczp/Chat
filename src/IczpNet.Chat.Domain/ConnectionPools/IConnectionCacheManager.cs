using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.ConnectionPools;

public interface IConnectionCacheManager //: IConnectionPoolManager
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
    Task DeleteByHostNameAsync(string hostHame);

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
    /// 获取最后在线时间（聊天对象）
    /// </summary>
    /// <param name="ownertId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<DateTime> GetLatestOnlineAsync(long ownertId, CancellationToken token = default);

    /// <summary>
    /// 获取设备类型 聊天对象
    /// </summary>
    /// <param name="ownertId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<List<string>> GetDeviceTypesAsync(long ownertId, CancellationToken token = default);

    /// <summary>
    /// 获取设备类型 聊天对象
    /// </summary>
    /// <param name="chatObjectIdList"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Dictionary<long, List<string>>> GetDeviceTypesAsync(List<long> chatObjectIdList, CancellationToken token = default);

    /// <summary>
    /// 获取设备信息(DeviceId,DeviceType) 聊天对象
    /// </summary>
    /// <param name="chatObjectIdList"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Dictionary<long, List<DeviceModel>>> GetDevicesAsync(List<long> chatObjectIdList, CancellationToken token = default);

    /// <summary>
    /// 获取设备类型 用户
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<List<string>> GetDeviceTypesAsync(Guid userId, CancellationToken token = default);

    /// <summary>
    /// 获取连接数量(用户)
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<long> GetCountByUserAsync(Guid userId, CancellationToken token = default);

    /// <summary>
    /// 获取连接数量(聊天对象)
    /// </summary>
    /// <param name="ownertId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<long> GetCountByChatObjectAsync(long ownertId, CancellationToken token = default);

    /// <summary>
    /// 获取会话连接
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="token"></param>
    /// <returns>Dictionary: key = connId, value = chatObjectList</returns>
    Task<Dictionary<string, List<long>>> GetConnectionsBySessionAsync(Guid sessionId, CancellationToken token = default);

    /// <summary>
    /// 添加到连接池
    /// </summary>
    /// <param name="ownerSessions"></param>
    /// <returns></returns>
    Task AddSessionAsync(List<(Guid SessionId, long OwnerId)> ownerSessions);
}
