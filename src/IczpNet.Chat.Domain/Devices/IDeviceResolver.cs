using System.Threading.Tasks;

namespace IczpNet.Chat.Devices;

public interface IDeviceResolver
{
    string GetDeviceId();

    string GetDeviceIdKey();

    string GetDeviceType();

    string GetDeviceTypeKey();

    string GetHeader(string key);

    Task<string>  GetDeviceIdAsync();

    Task<string> GetDeviceTypeAsync();

    Task<bool> IsEqualsAsync(string inputDeviceId);

}
