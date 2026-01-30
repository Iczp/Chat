using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Caching;

namespace IczpNet.Chat.SessionTags;

public interface ISessionTagManager
{
    public IDistributedCache<SessionTagCacheItem, Guid> Cache { get; }
    Task<Dictionary<Guid, List<SessionTagCacheItem>>> GetSessionUnitTagMapAsync(List<Guid> sessionUnitIds);
}
