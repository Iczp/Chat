using System.Threading.Tasks;

namespace IczpNet.Chat;

public interface IChatDeviceContext
{
    Task<string> GetDeviceIdAsync();
}
