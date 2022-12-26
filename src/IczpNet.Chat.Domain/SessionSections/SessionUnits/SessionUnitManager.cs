using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.ReadedRecorders;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class SessionUnitManager : DomainService, ISessionUnitManager
    {
        protected IRepository<SessionUnit, Guid> Repository { get; }
        protected IRepository<ReadedRecorder, Guid> ReadedRecorderRepository { get; }
        protected IRepository<Message, Guid> MessageRepository { get; }
        public SessionUnitManager(
            IRepository<SessionUnit, Guid> repository,
            IRepository<ReadedRecorder, Guid> readedRecorderRepository,
            IRepository<Message, Guid> messageRepository)
        {
            Repository = repository;
            ReadedRecorderRepository = readedRecorderRepository;
            MessageRepository = messageRepository;
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

        public async Task<SessionUnit> SetReadedAsync(SessionUnit entity, Guid messageId, bool isForce = false)
        {
            var message = await MessageRepository.GetAsync(messageId);

            // add readedRecorder
            /// ...
            return await SetEntityAsync(entity, x => x.SetReaded(message.AutoId, isForce = false));
        }

        public Task<SessionUnit> RemoveSessionAsync(SessionUnit entity)
        {
            return SetEntityAsync(entity, x => x.RemoveSession(Clock.Now));
        }

        public Task<SessionUnit> KillSessionAsync(SessionUnit entity)
        {
            return SetEntityAsync(entity, x => x.KillSession(Clock.Now));
        }

        public Task<SessionUnit> ClearMessageAsync(SessionUnit entity)
        {
            return SetEntityAsync(entity, x => x.ClearMessage(Clock.Now));
        }

        public Task<SessionUnit> DeleteMessageAsync(SessionUnit entity, Guid messageId)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetBadgeAsync(Guid ownerId)
        {
            var badge = (await Repository.GetQueryableAsync())
                .Where(x => x.OwnerId == ownerId)
                .Select(x => new
                {
                    Badge = x.Session.MessageList.Count(d =>
                    //!x.IsRollbacked &&
                    d.AutoId > x.ReadedMessageAutoId &&
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
    }
}
