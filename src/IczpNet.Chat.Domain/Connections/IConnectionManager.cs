using System;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.Connections;

public interface IConnectionManager
{
    /// <summary>
    /// Online
    /// </summary>
    /// <returns></returns>
    Task<ConnectionOptions> GetConfigAsync();

    /// <summary>
    /// Online
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Connection> CreateAsync(Connection connection, CancellationToken token = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentTime"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<int> GetOnlineCountAsync(DateTime currentTime, CancellationToken token = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Connection> UpdateActiveTimeAsync(string connectionId, CancellationToken token = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Connection> GetAsync(string connectionId, CancellationToken token = default);

    /// <summary>
    /// Offline
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task RemoveAsync(string connectionId, CancellationToken token = default);

    /// <summary>
    /// 清除不活跃的连接
    /// </summary>
    /// <returns></returns>
    Task<int> ClearUnactiveAsync(CancellationToken token = default);
}
