using AutoMapper;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Microsoft.Extensions.Logging;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using System.Linq;

namespace IczpNet.Chat.SessionUnits
{
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
            var cacheItems = SessionUnitManager.GetCacheListBySessionIdAsync(source.SessionId.Value).GetAwaiter().GetResult();

            if (cacheItems != null)
            {
                //destination.
                var item = cacheItems.FirstOrDefault(x => x.Id == source.Id);

                if (item != null)
                {
                    destination.PublicBadge = item.PublicBadge;
                    destination.PrivateBadge = item.PrivateBadge;
                    destination.FollowingCount = item.FollowingCount;
                    destination.RemindAllCount = item.RemindAllCount;
                    destination.RemindMeCount = item.RemindMeCount;
                    destination.LastMessageId = item.LastMessageId;
                    destination.Ticks = item.Ticks;

                    Logger.LogError($"SessionUnitId:{source.SessionId}");
                }


                Logger.LogWarning($"SessionUnitOwnerDtoMappingAction");
            }
            else
            {
                Logger.LogWarning($"SessionUnit CacheItems = null");
            }

        }
    }
}
