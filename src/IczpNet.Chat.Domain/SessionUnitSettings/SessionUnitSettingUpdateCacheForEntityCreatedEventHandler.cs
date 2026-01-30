using System;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.SessionUnitSettings;

public class SessionUnitSettingUpdateCacheForEntityCreatedEventHandler(
    IObjectMapper objectMapper,
    IDistributedCache<SessionUnitSettingCacheItem, Guid> sessionUnitSettingCache)
    : DomainService,
    ILocalEventHandler<EntityUpdatedEventData<SessionUnitSetting>>,
    ITransientDependency
{
    public IObjectMapper ObjectMapper { get; } = objectMapper;
    public IDistributedCache<SessionUnitSettingCacheItem, Guid> SessionUnitSettingCache { get; } = sessionUnitSettingCache;

    public async Task HandleEventAsync(EntityUpdatedEventData<SessionUnitSetting> eventData)
    {
        // Update Cache
        var entity = eventData.Entity;

        var sessionUnitSettingCacheItem = ObjectMapper.Map<SessionUnitSetting, SessionUnitSettingCacheItem>(entity);

        await SessionUnitSettingCache.SetAsync(entity.SessionUnitId, sessionUnitSettingCacheItem);
    }
}
