using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;

namespace IczpNet.Chat.Devices;

/// <summary>
/// 
/// </summary>
/// <param name="deviceResolver"></param>
public class DeviceClaimsPrincipalContributor(IDeviceResolver deviceResolver) :  ITransientDependency, IAbpClaimsPrincipalContributor
{
    protected IDeviceResolver DeviceResolver { get; } = deviceResolver;

    public async Task ContributeAsync(AbpClaimsPrincipalContributorContext context)
    {
        var identity = context.ClaimsPrincipal.Identities.FirstOrDefault();

        var userId = identity?.FindUserId();

        if (!userId.HasValue)
        {
            return;
        }

        var deviceId = await DeviceResolver.GetDeviceIdAsync();

        if (!string.IsNullOrWhiteSpace(deviceId))
        {
            identity.AddIfNotContains(new Claim(DeviceExtensions.DeviceIdClaim, deviceId, ClaimValueTypes.Integer));
        }

        var deviceType = await DeviceResolver.GetDeviceTypeAsync();

        if (!string.IsNullOrWhiteSpace(deviceType))
        {
            identity.AddIfNotContains(new Claim(DeviceExtensions.DeviceTypeClaim, deviceType, ClaimValueTypes.Integer));
        }
    }
}
