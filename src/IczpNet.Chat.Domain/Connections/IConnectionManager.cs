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
    Task DeleteAsync(string connectionId);
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<int> ClearUnactiveAsync();
}
