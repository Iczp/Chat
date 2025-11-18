using System.Threading.Tasks;

namespace IczpNet.Chat.Devices;

public interface IDeviceResolver
{
    string GetDeviceId();

    string GetDeviceIdKey();

    Task<string>  GetDeviceIdAsync();

    string GetDeviceTypeKey();

    string GetDeviceType();

    Task<string> GetDeviceTypeAsync();

    string GetAppId();

    string GetAppIdKey();

    Task<string> GetAppIdAsync();

    string GetAppVersion();

    string GetAppVersionKey();

    Task<string> GetAppVersionAsync();


   

    string GetParameter(string key);

    Task<bool> IsEqualsAsync(string inputDeviceId);

}
