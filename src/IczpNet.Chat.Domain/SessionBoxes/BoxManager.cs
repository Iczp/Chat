using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.SessionBoxes;

public class BoxManager(
    IDistributedCache<BoxCacheList, BoxOwnerCacheKey> ownerCache,
    IObjectMapper objectMapper,
    IRepository<Box, Guid> repository) : DomainService, IBoxManager
{
    public IDistributedCache<BoxCacheList, BoxOwnerCacheKey> OwnerCache { get; } = ownerCache;
    public IObjectMapper ObjectMapper { get; } = objectMapper;
    public IRepository<Box, Guid> Repository { get; } = repository;

    protected BoxInfo MapToInfo(Box entity, BoxInfo info)
    {
        return ObjectMapper.Map(entity, info);
    }
    protected BoxInfo MapToInfo(Box entity)
    {
        return ObjectMapper.Map<Box, BoxInfo>(entity);
    }

    public async Task<IEnumerable<Box>> GetListByOwnerAsync(long ownerId)
    {
        return (await Repository.GetQueryableAsync()).Where(x => x.OwnerId == ownerId);
    }

    public Task<BoxCacheList> GetCacheListByOwnerAsync(long ownerId)
    {
        return OwnerCache.GetOrAddAsync(BoxOwnerCacheKey.Create(ownerId), async () =>
        {
            return new BoxCacheList()
            {
                OwnerId = ownerId,
                List = (await GetListByOwnerAsync(ownerId)).Select(MapToInfo).ToList(),
            };
        });
    }
}
