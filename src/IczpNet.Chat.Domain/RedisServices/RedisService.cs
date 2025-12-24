using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.RedisServices;

public abstract class RedisService : DomainService
{
    protected IOptions<AbpDistributedCacheOptions> Options => LazyServiceProvider.LazyGetRequiredService<IOptions<AbpDistributedCacheOptions>>();
    protected IConnectionMultiplexer ConnectionMultiplexer => LazyServiceProvider.LazyGetRequiredService<IConnectionMultiplexer>();
    protected IDatabase Database => ConnectionMultiplexer.GetDatabase();
}
