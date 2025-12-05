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

}
