using System.Collections.Generic;
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
    /// <returns></returns>
    Task<bool> AddAsync(ConnectionPoolCacheItem connectionPool);

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
    /// 激活连接
    /// </summary>
    /// <param name="connectionPool"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task ActiveAsync(ConnectionPoolCacheItem connectionPool, string message);

    /// <summary>
    /// 获取连接列表
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    Task<List<ConnectionPoolCacheItem>> GetListAsync(string connectionId);

    /// <summary>
    /// 发送消息给所有连接
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task SendToAllAsync(string message);

    /// <summary>
    /// 获取连接数量
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    Task<int> TotalCountAsync(string host);

    /// <summary>
    /// 获取所有连接
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<ConnectionPoolCacheItem>> GetAllListAsync();

    /// <summary>
    /// 发送消息给指定连接
    /// </summary>
    /// <param name="connectionPool"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<bool> SendMessageAsync(ConnectionPoolCacheItem connectionPool, string message);

    /// <summary>
    /// 清空所有连接
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    Task ClearAllAsync(string host);
    
}
