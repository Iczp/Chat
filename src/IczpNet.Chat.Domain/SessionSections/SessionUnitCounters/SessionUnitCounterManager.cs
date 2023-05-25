using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using IczpNet.AbpCommons.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace IczpNet.Chat.SessionSections.SessionUnitCounters
{
    public class SessionUnitCounterManager : DomainService, ISessionUnitCounterManager
    {

        protected ISessionUnitCounterRepository Repository { get; set; }

        public SessionUnitCounterManager(ISessionUnitCounterRepository repository)
        {
            Repository = repository;
        }

        /// <inheritdoc/>
        public async Task<int> IncremenetAsync(SessionUnitCounterArgs args)
        {
            Logger.LogInformation($"Incremenet args:{args},starting.....................................");

            var stopwatch = Stopwatch.StartNew();

            var counter = 0;

            if (args.IsPrivate)
            {
                counter += await Repository.IncrementPrivateBadgeAndUpdateLastMessageIdAsync(args.SessionId, args.LastMessageId, args.MessageCreationTime, args.PrivateBadgeSessionUnitIdList);
            }
            else
            {
                counter += await Repository.IncrementPublicBadgeAndRemindAllCountAndUpdateLastMessageIdAsync(args.SessionId, args.LastMessageId, args.MessageCreationTime, args.IgnoreSessionUnitId, args.IsRemindAll);
            }

            if (args.RemindSessionUnitIdList.IsAny())
            {
                counter += await Repository.IncrementRemindMeCountAsync(args.SessionId, args.MessageCreationTime, args.RemindSessionUnitIdList);
            }

            if (args.FollowingSessionUnitIdList.IsAny())
            {
                counter += await Repository.IncrementRemindMeCountAsync(args.SessionId, args.MessageCreationTime, args.FollowingSessionUnitIdList);
            }

            stopwatch.Stop();

            Logger.LogInformation($"Incremenet counter:{counter}, stopwatch: {stopwatch.ElapsedMilliseconds}ms.");

            return counter;
        }
    }
}
