using IczpNet.Chat.SessionUnitTags;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using System.Linq;
using Volo.Abp.ObjectMapping;


namespace IczpNet.Chat.SessionTags;

public class SessionTagManager : DomainService, ISessionTagManager
{
    public IRepository<SessionTag, Guid> Repository => LazyServiceProvider.LazyGetRequiredService<IRepository<SessionTag, Guid>>();

    public IRepository<SessionUnitTag> SessionUnitTagRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<SessionUnitTag>>();
    public IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetRequiredService<IObjectMapper>();

    public IDistributedCache<SessionTagCacheItem, Guid> Cache => LazyServiceProvider.LazyGetRequiredService<IDistributedCache<SessionTagCacheItem, Guid>>();

    public async Task<Dictionary<Guid, List<SessionTagCacheItem>>> GetSessionUnitTagMapAsync(List<Guid> sessionUnitIds)
    {
        var unitTagIdMap = (await SessionUnitTagRepository.GetQueryableAsync())
                   .Where(x => sessionUnitIds.Contains(x.SessionUnitId))
                   .GroupBy(x => x.SessionUnitId)
                   .ToDictionary(x => x.Key, x => x.Select(x => x.SessionTagId).ToList());

        var tagIds = unitTagIdMap.SelectMany(x => x.Value).Distinct().ToList();

        var tagList = await Cache.GetOrAddManyAsync(tagIds, async ids =>
            {
                var tagMap = (await Repository.GetListAsync(x => tagIds.Contains(x.Id))).ToDictionary(x => x.Id);

                return [.. ids.Select(x => new KeyValuePair<Guid, SessionTagCacheItem>(x, MapToCacheItem(tagMap.GetValueOrDefault(x))))];

            });

        var tagMap = tagList.ToDictionary(x => x.Key, x => x.Value);

        return unitTagIdMap.ToDictionary(
            x => x.Key,
            x => x.Value.Select(v => tagMap.GetValueOrDefault(v)).ToList()
            );
    }

    private SessionTagCacheItem MapToCacheItem(SessionTag sessionTag)
    {
        return ObjectMapper.Map<SessionTag, SessionTagCacheItem>(sessionTag);
    }
}
