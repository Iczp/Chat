﻿using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ReadedRecorders
{
    public class ReadedRecorderManager : DomainService, IReadedRecorderManager
    {
        protected IRepository<ReadedRecorder> Repository { get; }

        protected IMessageRepository MessageRepository { get; }
        protected ISessionUnitRepository SessionUnitRepository { get; }

        public ReadedRecorderManager(
            IRepository<ReadedRecorder> repository, 
            IMessageRepository messageRepository, 
            ISessionUnitRepository sessionUnitRepository)
        {
            Repository = repository;
            MessageRepository = messageRepository;
            SessionUnitRepository = sessionUnitRepository;
        }

        /// <inheritdoc/>
        public virtual async Task<Dictionary<long, int>> GetCountsAsync(List<long> messageIdList)
        {
            var dict = (await Repository.GetQueryableAsync())
                .Where(x => messageIdList.Contains(x.MessageId))
                .GroupBy(x => x.MessageId)
                .ToDictionary(x => x.Key, x => x.Count());

            foreach (var messageId in messageIdList.Except(dict.Keys))
            {
                dict[messageId] = 0;
            }

            return dict;
        }

        /// <inheritdoc/>
        public virtual async Task<IQueryable<SessionUnit>> QueryReadedAsync(long messageId)
        {
            var readedSessionUnitIdList = (await Repository.GetQueryableAsync())
                .Where(x => x.MessageId == messageId)
                .Select(x => x.SessionUnitId);

            return (await SessionUnitRepository.GetQueryableAsync())
                .Where(x => readedSessionUnitIdList.Contains(x.Id));
        }

        /// <inheritdoc/>
        public virtual async Task<IQueryable<SessionUnit>> QueryUnreadedAsync(long messageId)
        {
            var message = await MessageRepository.GetAsync(messageId);

            var readedSessionUnitIdList = (await Repository.GetQueryableAsync())
                .Where(x => x.MessageId == messageId)
                .Select(x => x.SessionUnitId);

            var query = (await SessionUnitRepository.GetQueryableAsync())
                .Where(x => x.SessionId == message.SessionId)
                .Where(x => x.IsEnabled && x.IsPublic && !x.IsKilled)
                .Where(x => !x.HistoryFristTime.HasValue || message.CreationTime > x.HistoryFristTime)
                .Where(x => !x.HistoryLastTime.HasValue || message.CreationTime < x.HistoryLastTime);

            if (message.IsPrivate)
            {
                return query.Where(x => x.Id == message.SessionUnitId || (x.OwnerId == message.ReceiverId && x.DestinationId == message.SenderId));
            }

            return query.Where(x => !readedSessionUnitIdList.Contains(x.Id));
        }
    }
}