using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.ConnectionPools;

/// <summary>
/// 连接池管理器
/// </summary>
public interface IConnectionPoolManagerBase<TCacheItem> where TCacheItem : IConnectionPool, new()
{
    /// <summary>
    /// 添加连接
    /// </summary>
    /// <param name="connectionPool"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> ConnectedAsync(TCacheItem connectionPool, CancellationToken token = default);

    /// <summary>
    /// 设置活跃时间
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<TCacheItem> UpdateActiveTimeAsync(string connectionId, CancellationToken token = default);

    /// <summary>
    /// 移除连接
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> DisconnectedAsync(string connectionId, CancellationToken token = default);

    ///// <summary>
    ///// 移除连接
    ///// </summary>
    ///// <param name="connectionId"></param>
    //void Remove(string connectionId);

    /// <summary>
    /// 获取连接数量
    /// </summary>
    /// <param name="host"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<int> GetTotalCountAsync(string host = null, CancellationToken token = default);

    /// <summary>
    /// 清空所有连接
    /// </summary>
    /// <param name="host"></param>
    /// <param name="reason"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task ClearAllAsync(string host, string reason, CancellationToken token = default);

    /// <summary>
    /// 创建查询
    /// </summary>
    /// <returns></returns>
    Task<IQueryable<TCacheItem>> CreateQueryableAsync(CancellationToken token = default);

    /// <summary>
    /// 获取连接
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<TCacheItem> GetAsync(string connectionId, CancellationToken token = default);

    /// <summary>
    /// 获取连接
    /// </summary>
    /// <param name="connectionIdList"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<KeyValuePair<string, TCacheItem>[]> GetManyAsync(IEnumerable<string> connectionIdList, CancellationToken token = default);

    /// <summary>
    /// 更新连接数量
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<int> UpdateAllConnectionIdsAsync(CancellationToken token = default);

    /// <summary>
    /// 获取连接
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<TCacheItem> GetByDeviceIdAsync(string deviceId, CancellationToken token = default);

    /// <summary>
    /// 获取连接
    /// </summary>
    /// <param name="deviceIdList"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<KeyValuePair<DeviceIdCacheKey, TCacheItem>[]> GetManyByDeviceIdAsync(IEnumerable<string> deviceIdList, CancellationToken token = default);
}
