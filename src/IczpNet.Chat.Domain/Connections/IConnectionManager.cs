using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.Connections
{
    public interface IConnectionManager
    {
        Task<Connection> OnlineAsync(Connection connection);
        Task<int> GetOnlineCountAsync(DateTime currentTime);
        Task<Connection> UpdateActiveTimeAsync(Guid connectionId);
        Task<Connection> GetAsync(Guid connectionId);
        Task OfflineAsync(Guid connectionId);
        Task<int> DeleteInactiveAsync();
    }
}
