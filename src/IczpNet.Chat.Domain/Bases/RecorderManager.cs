using IczpNet.AbpCommons;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Bases
{
    public abstract class RecorderManager<TEntity> : DomainService, IRecorderManager<TEntity> where TEntity : class, IEntity, IMessageId, ISessionUnitId
    {
        protected IRepository<TEntity> Repository { get; }
        protected IMessageRepository MessageRepository => LazyServiceProvider.LazyGetService<IMessageRepository>();
        protected ISessionUnitRepository SessionUnitRepository => LazyServiceProvider.LazyGetService<ISessionUnitRepository>();

        public RecorderManager(IRepository<TEntity> repository)
        {
            Repository = repository;
        }

        protected abstract TEntity CreateEntity(SessionUnit sessionUnit, Message message, string deviceId);

        /// <inheritdoc/>
        public virtual async Task<Dictionary<long, int>> GetCountsAsync(List<long> messageIdList)
        {
            var dict = messageIdList.ToDictionary(x => x, x => 0);

            var groups = (await Repository.GetQueryableAsync())
                .Where(x => messageIdList.Contains(x.MessageId))
                .GroupBy(x => x.MessageId);

            foreach (var item in groups)
            {
                dict[item.Key] = item.Count();
            }

            return dict;
        }

        /// <inheritdoc/>
        public virtual async Task<IQueryable<SessionUnit>> QueryRecordedAsync(long messageId)
        {
            var readedSessionUnitIdList = (await Repository.GetQueryableAsync())
                .Where(x => x.MessageId == messageId)
                .Select(x => x.SessionUnitId);

            return (await SessionUnitRepository.GetQueryableAsync())
                .Where(x => readedSessionUnitIdList.Contains(x.Id));
        }

        /// <inheritdoc/>
        public virtual async Task<IQueryable<SessionUnit>> QueryUnrecordedAsync(long messageId)
        {
            var message = await MessageRepository.GetAsync(messageId);

            var readedSessionUnitIdList = (await Repository.GetQueryableAsync())
                .Where(x => x.MessageId == messageId)
                .Select(x => x.SessionUnitId);

            var query = (await SessionUnitRepository.GetQueryableAsync())
                .Where(x => x.SessionId == message.SessionId)
                .Where(SessionUnit.GetActivePredicate(message.CreationTime));

            if (message.IsPrivate)
            {
                return query.Where(x => x.Id == message.SessionUnitId || (x.OwnerId == message.ReceiverId && x.DestinationId == message.SenderId));
            }

            return query.Where(x => !readedSessionUnitIdList.Contains(x.Id));
        }

        public virtual async Task<TEntity> CreateIfNotContainsAsync(SessionUnit sessionUnit, long messageId, string deviceId)
        {
            var message = await MessageRepository.GetAsync(messageId);

            Assert.If(sessionUnit.SessionId != message.SessionId, $"Not in same session,messageId:{messageId}");

            var recorder = await Repository.FindAsync(x => x.SessionUnitId == sessionUnit.Id && x.MessageId == messageId);

            if (recorder == null)
            {
                return await Repository.InsertAsync(CreateEntity(sessionUnit, message, deviceId), autoSave: true);
            }

            return recorder;
        }

        

        public virtual async Task<List<TEntity>> CreateManyAsync(SessionUnit sessionUnit, List<long> messageIdList, string deviceId)
        {
            var dbMessageList = (await MessageRepository.GetQueryableAsync())
                .Where(x => x.SessionId == sessionUnit.SessionId && messageIdList.Contains(x.Id))
                .ToList()
                ;

            if (!dbMessageList.Any())
            {
                return new List<TEntity>();
            }

            var dbMessageIdList = dbMessageList.Select(x => x.Id).ToList();

            var recordedMessageIdList = (await Repository.GetQueryableAsync())
                .Where(x => x.SessionUnitId == sessionUnit.Id && dbMessageIdList.Contains(x.MessageId))
                .Select(x => x.MessageId)
                .ToList()
                ;

            var newMessageIdList = dbMessageIdList.Except(recordedMessageIdList);

            var newMessages = dbMessageList.Where(x => newMessageIdList.Contains(x.Id))
                .Select(x => CreateEntity(sessionUnit, x, deviceId))
                .ToList();

            if (newMessageIdList.Any())
            {
                await Repository.InsertManyAsync(newMessages, autoSave: true);
            }

            //notice :IChatPusher.
            //...

            return newMessages;
        }
    }
}
