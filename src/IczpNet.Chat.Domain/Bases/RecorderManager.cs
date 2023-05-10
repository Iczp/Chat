﻿using IczpNet.Chat.DataFilters;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Bases
{
    public abstract class RecorderManager<TEntity> : DomainService, IRecorderManager where TEntity : class, IEntity, IMessageId, ISessionUnitId
    {
        protected IRepository<TEntity> Repository { get; }
        protected IMessageRepository MessageRepository => LazyServiceProvider.LazyGetService<IMessageRepository>();
        protected ISessionUnitRepository SessionUnitRepository => LazyServiceProvider.LazyGetService<ISessionUnitRepository>();

        public RecorderManager(IRepository<TEntity> repository)
        {
            Repository = repository;
        }

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
                .Where(new MessageSessionUnitSpecification(message).ToExpression());

            if (message.IsPrivate)
            {
                return query.Where(x => x.Id == message.SessionUnitId || (x.OwnerId == message.ReceiverId && x.DestinationId == message.SenderId));
            }

            return query.Where(x => !readedSessionUnitIdList.Contains(x.Id));
        }
    }
}