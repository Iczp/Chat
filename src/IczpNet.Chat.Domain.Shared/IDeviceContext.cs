using System.Threading.Tasks;

namespace IczpNet.Chat;

public interface IDeviceContext
{
    Task<string> GetDeviceIdAsync();
}
