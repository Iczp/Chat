using IczpNet.Chat.ConnectionPools;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;

namespace IczpNet.Chat.ScanLogins;

public class ScanLoginConnectionPoolManager : ConnectionPoolManagerBase<ConnectionPool, IndexCacheKey>, IScanLoginConnectionPoolManager
{

    public IOptions<ScanLoginOptions> Options { get; set; }

    protected ScanLoginOptions Config => Options.Value;

    protected override string ConnectionIdListSetCacheKey => nameof(ScanLoginConnectionPoolManager);

    protected override DistributedCacheEntryOptions DistributedCacheEntryOptions => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(Config.ExpiredSeconds)
    };
}
