using IczpNet.AbpCommons.Extensions;
using Microsoft.EntityFrameworkCore;
using Pipelines.Sockets.Unofficial.Buffers;
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

    public async Task<Dictionary<long, List<BoxInfo>>> GetCacheListByOwnersAsync(List<long> ownerIds)
    {
        if (ownerIds == null || ownerIds.Count == 0)
        {
            return [];
        }

        var keys = ownerIds
            .Distinct()
            .Select(BoxOwnerCacheKey.Create)
            .ToArray();

        var result = await OwnerCache.GetOrAddManyAsync(keys, async cacheKeys =>
        {
            var ownerIdList = cacheKeys
                .Select(x => x.OwnerId)
                .Distinct()
                .ToList();

            var queryable = await Repository.GetQueryableAsync();

            var entities = await queryable
                .Where(x => ownerIdList.Contains(x.OwnerId.Value))
                .ToListAsync();

            var groupMap = entities
                .GroupBy(x => x.OwnerId)
                .ToDictionary(x => x.Key, x => x.ToList());

            var result = new List<KeyValuePair<BoxOwnerCacheKey, BoxCacheList>>(
                ownerIdList.Count);

            foreach (var ownerId in ownerIdList)
            {
                if (!groupMap.TryGetValue(ownerId, out var list))
                {
                    // 空缓存，防穿透
                    result.Add(
                        new KeyValuePair<BoxOwnerCacheKey, BoxCacheList>(
                            BoxOwnerCacheKey.Create(ownerId),
                            BoxCacheList.Empty(ownerId)));

                    continue;
                }

                var cacheList = new BoxCacheList
                {
                    OwnerId = ownerId,
                    List = [.. list.Select(MapToInfo)]
                };

                result.Add(new KeyValuePair<BoxOwnerCacheKey, BoxCacheList>(BoxOwnerCacheKey.Create(ownerId), cacheList));
            }

            return [.. result];
        });
        return result.ToDictionary(x => x.Key.OwnerId, x => x.Value.List);
    }
}
