using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.ReadedRecorders;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class SessionUnitManager : DomainService, ISessionUnitManager
    {
        protected ISessionUnitRepository Repository { get; }
        protected IRepository<ReadedRecorder, Guid> ReadedRecorderRepository { get; }
        protected IMessageRepository MessageRepository { get; }
        protected IDistributedCache<List<SessionUnitInfo>, Guid> UnitListCache { get; }
        protected IDistributedCache<string, Guid> UnitCountCache { get; }

        public SessionUnitManager(
            ISessionUnitRepository repository,
            IRepository<ReadedRecorder, Guid> readedRecorderRepository,
            IMessageRepository messageRepository,
            IDistributedCache<List<SessionUnitInfo>, Guid> unitListCache,
            IDistributedCache<string, Guid> unitCountCache)
        {
            Repository = repository;
            ReadedRecorderRepository = readedRecorderRepository;
            MessageRepository = messageRepository;
            UnitListCache = unitListCache;
            UnitCountCache = unitCountCache;
        }

        protected async Task<SessionUnit> SetEntityAsync(SessionUnit entity, Action<SessionUnit> action = null)
        {
            action?.Invoke(entity);

            return await Repository.UpdateAsync(entity);
        }

        public async Task<SessionUnit> SetToppingAsync(SessionUnit entity, bool isTopping)
        {
            return await SetEntityAsync(entity, x => x.SetTopping(isTopping));
        }

        public async Task<SessionUnit> SetReadedAsync(SessionUnit entity, long messageId, bool isForce = false)
        {
            var message = await MessageRepository.GetAsync(messageId);

            // add readedRecorder
            /// ...
            return await SetEntityAsync(entity, x => x.SetReaded(message.Id, isForce = false));
        }

        public async Task<SessionUnit> SetImmersedAsync(SessionUnit entity, bool isImmersed)
        {
            return await SetEntityAsync(entity, x => x.SetImmersed(isImmersed));
        }

        public Task<SessionUnit> RemoveAsync(SessionUnit entity)
        {
            return SetEntityAsync(entity, x => x.Remove(Clock.Now));
        }

        public Task<SessionUnit> KillAsync(SessionUnit entity)
        {
            return SetEntityAsync(entity, x => x.Kill(Clock.Now));
        }

        public Task<SessionUnit> ClearMessageAsync(SessionUnit entity)
        {
            return SetEntityAsync(entity, x => x.ClearMessage(Clock.Now));
        }

        public Task<SessionUnit> DeleteMessageAsync(SessionUnit entity, long messageId)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetBadgeAsync(long ownerId, bool? isImmersed = null)
        {
            var badge = (await Repository.GetQueryableAsync())
                .Where(x => x.OwnerId == ownerId)
                .WhereIf(isImmersed.HasValue, x => x.IsImmersed == isImmersed)
                .Select(x => new
                {
                    Badge = x.Session.MessageList.Count(d =>
                    //!x.IsRollbacked &&
                    (x.ReadedMessageId == null || d.Id > x.ReadedMessageId) &&
                    d.SenderId != x.OwnerId &&
                    (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                    (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                    (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime))
                })
                .Where(x => x.Badge > 0)
                .ToList()
            .Sum(x => x.Badge);

            return badge;
        }

        public async Task<int> GetCountAsync(Guid sessionId)
        {
            var value = await UnitCountCache.GetOrAddAsync(sessionId, async () =>
            {
                var count = await Repository.CountAsync(x => x.SessionId == sessionId);
                return count.ToString();
            });
            return int.Parse(value);
        }

        public Task<int> BatchUpdateAsync(Guid sessionId, long lastMessageId)
        {
            return Repository.BatchUpdateAsync(sessionId, lastMessageId);
        }

        public Task<List<SessionUnitInfo>> GetCacheListBySessionIdAsync(Guid sessionId)
        {
            return UnitListCache.GetAsync(sessionId);
        }

        public Task<List<SessionUnitInfo>> GetOrAddCacheListBySessionIdAsync(Guid sessionId)
        {
            return UnitListCache.GetOrAddAsync(sessionId, () => GetListBySessionIdAsync(sessionId));
        }

        public async Task SetCacheListBySessionIdAsync(Guid sessionId)
        {
            var sessionUnitInfoList = await GetListBySessionIdAsync(sessionId);
            await UnitListCache.SetAsync(sessionId, sessionUnitInfoList);
        }

        public async Task<List<SessionUnitInfo>> GetListBySessionIdAsync(Guid sessionId)
        {
            var list = (await Repository.GetQueryableAsync())
                .Where(x => x.SessionId == sessionId)
                .Select(x => new SessionUnitInfo()
                {
                    Id = x.Id,
                    SessionId = x.SessionId,
                    DestinationId = x.DestinationId,
                    OwnerId = x.OwnerId,
                    DestinationObjectType = x.DestinationObjectType,
                })
                .ToList();

            await UnitCountCache.SetAsync(sessionId, list.Count.ToString());

            return list;
        }
    }
}
