using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.ConnectionPools;

/// <summary>
/// 连接池管理器
/// </summary>
public interface IConnectionPoolManager
{
    /// <summary>
    /// 添加连接
    /// </summary>
    /// <param name="connectionPool"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> AddAsync(ConnectionPoolCacheItem connectionPool, CancellationToken token = default);

    /// <summary>
    /// 移除连接
    /// </summary>
    /// <param name="connectionPool"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> RemoveAsync(ConnectionPoolCacheItem connectionPool, CancellationToken token = default);

    /// <summary>
    /// 移除连接
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> RemoveAsync(string connectionId, CancellationToken token = default);

    /// <summary>
    /// 移除连接
    /// </summary>
    /// <param name="connectionId"></param>
    void Remove(string connectionId);

    /// <summary>
    /// 获取连接数量
    /// </summary>
    /// <param name="host"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<int> GetTotalCountAsync(string host, CancellationToken token = default);

    /// <summary>
    /// 获取连接数量
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<int> GetTotalCountAsync(CancellationToken token = default);

    /// <summary>
    /// 获取所有连接
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<ConnectionPoolCacheItem>> GetAllListAsync(CancellationToken token = default);

    /// <summary>
    /// 清空所有连接
    /// </summary>
    /// <param name="host"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task ClearAllAsync(string host, CancellationToken token = default);

    /// <summary>
    /// 创建查询
    /// </summary>
    /// <returns></returns>
    Task<IQueryable<ConnectionPoolCacheItem>> CreateQueryableAsync(CancellationToken token = default);

    /// <summary>
    /// 获取连接
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<ConnectionPoolCacheItem> GetAsync(string connectionId, CancellationToken token = default);

    /// <summary>
    /// 更新连接数量
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<int> UpdateConnectionIdsAsync(CancellationToken token = default);
}
