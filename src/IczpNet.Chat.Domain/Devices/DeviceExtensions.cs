using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Users;

namespace IczpNet.Chat.Devices;

public static class DeviceExtensions
{
    public const string DeviceIdClaim = "device_id";

    public const string DeviceTypeClaim = "device_type";

    /// <summary>
    /// Get DeviceId by ICurrentUser
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public static string GetDeviceId(this ICurrentUser currentUser)
    {
        return currentUser.FindClaimValue(DeviceIdClaim);
    }

    /// <summary>
    /// Get DeviceType by ICurrentUser
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public static string GetDeviceType(this ICurrentUser currentUser)
    {
        return currentUser.FindClaimValue(DeviceTypeClaim);
    }
}