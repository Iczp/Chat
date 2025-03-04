using System;
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
    /// <returns></returns>
    Task<Connection> CreateAsync(Connection connection);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentTime"></param>
    /// <returns></returns>
    Task<int> GetOnlineCountAsync(DateTime currentTime);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    Task<Connection> UpdateActiveTimeAsync(string connectionId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    Task<Connection> GetAsync(string connectionId);

    /// <summary>
    /// Offline
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    Task RemoveAsync(string connectionId);

    /// <summary>
    /// 清除不活跃的连接
    /// </summary>
    /// <returns></returns>
    Task<int> ClearUnactiveAsync();
}
