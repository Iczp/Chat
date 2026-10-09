using Microsoft.Extensions.Options;
using Volo.Abp.Caching;
using Volo.Abp.MultiTenancy;

namespace IczpNet.Chat;

// Permission grants are shared by the independent APIs; all other cache entries keep their host prefix.
public sealed class SharedPermissionCacheKeyNormalizer : DistributedCacheKeyNormalizer
{
    private const string PermissionGrantCacheName = "Volo.Abp.PermissionManagement.PermissionGrant";

    public SharedPermissionCacheKeyNormalizer(
        ICurrentTenant currentTenant,
        IOptions<AbpDistributedCacheOptions> options)
        : base(currentTenant, options)
    {
    }

    public override string NormalizeKey(DistributedCacheKeyNormalizeArgs args)
    {
        if (args.CacheName != PermissionGrantCacheName)
        {
            return base.NormalizeKey(args);
        }

        var key = $"c:{args.CacheName},k:SharedPermissions:{args.Key}";
        return !args.IgnoreMultiTenancy && CurrentTenant.Id.HasValue
            ? $"t:{CurrentTenant.Id.Value},{key}"
            : key;
    }
}
