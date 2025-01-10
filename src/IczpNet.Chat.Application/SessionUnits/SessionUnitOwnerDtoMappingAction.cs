using AutoMapper;
using IczpNet.Chat.SessionUnits.Dtos;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitOwnerDtoMappingAction : DomainService, IMappingAction<SessionUnit, SessionUnitOwnerDto>, ITransientDependency
{
    protected ISessionUnitManager SessionUnitManager { get; }

    public SessionUnitOwnerDtoMappingAction(
        ISessionUnitManager sessionUnitManager)
    {
        SessionUnitManager = sessionUnitManager;
    }

    public void Process(SessionUnit source, SessionUnitOwnerDto destination, ResolutionContext context)
    {
        var item = SessionUnitManager.GetCacheItemAsync(source).GetAwaiter().GetResult();

        if (item != null)
        {
            //destination.PublicBadge = item.PublicBadge;
            //destination.PrivateBadge = item.PrivateBadge;
            //destination.FollowingCount = item.FollowingCount;
            //destination.RemindAllCount = item.RemindAllCount;
            //destination.RemindMeCount = item.RemindMeCount;
            //destination.LastMessageId = item.LastMessageId;
            //destination.Ticks = item.Ticks;

            //Logger.LogError($"SessionUnitId:{source.SessionId}");
        }
        else
        {
            //Logger.LogWarning($"CacheItem = null, sessionUnitId:{source.Id}");
        }
    }
}
