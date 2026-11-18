using IczpNet.AbpCommons.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
            var missingKeys = cacheKeys.ToArray();
            var ownerIdList = missingKeys
                .Select(x => x.OwnerId)
                .Distinct()
                .ToList();
            var boxMap = await LoadBoxInfoMapByOwnersAsync(ownerIdList);

            var result = new List<KeyValuePair<BoxOwnerCacheKey, BoxCacheList>>(
                ownerIdList.Count);

            foreach (var key in missingKeys)
            {
                var cacheList = boxMap.TryGetValue(key.OwnerId, out var boxes)
                    ? new BoxCacheList { OwnerId = key.OwnerId, List = boxes }
                    : BoxCacheList.Empty(key.OwnerId); // 空缓存，防穿透

                // GetOrAddManyAsync uses the original key instances to join factory results.
                result.Add(new KeyValuePair<BoxOwnerCacheKey, BoxCacheList>(key, cacheList));
            }

            return [.. result];
        });

        var missingOwnerIds = result
            .Where(x => x.Value?.List == null)
            .Select(x => x.Key.OwnerId)
            .ToList();
        Dictionary<long, List<BoxInfo>> fallbackMap = [];

        if (missingOwnerIds.Count > 0)
        {
            Logger.LogWarning("Box cache returned {MissingCount} null values; loading from database", missingOwnerIds.Count);
            fallbackMap = await LoadBoxInfoMapByOwnersAsync(missingOwnerIds);
        }

        return result.ToDictionary(
            x => x.Key.OwnerId,
            x => x.Value?.List ?? fallbackMap.GetValueOrDefault(x.Key.OwnerId) ?? []);
    }

    private async Task<Dictionary<long, List<BoxInfo>>> LoadBoxInfoMapByOwnersAsync(List<long> ownerIds)
    {
        var queryable = await Repository.GetQueryableAsync();
        var entities = await queryable
            .Where(x => ownerIds.Contains(x.OwnerId.Value))
            .ToListAsync();

        return entities
            .GroupBy(x => x.OwnerId.Value)
            .ToDictionary(x => x.Key, x => x.Select(MapToInfo).ToList());
    }
}
