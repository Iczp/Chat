using IczpNet.Chat.Bases;
using IczpNet.Chat.MessageSections.Counters;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.DeletedRecorders;

public class DeletedRecorderManager(
    IDistributedCache<List<long>, DeletedRecorderCacheKey> deletedRecorderDistributedCache,
    IRepository<DeletedRecorder> repository) : RecorderManager<DeletedRecorder>(repository), IDeletedRecorderManager
{
    public IDistributedCache<List<long>, DeletedRecorderCacheKey> DeletedRecorderDistributedCache { get; } = deletedRecorderDistributedCache;

    protected override DeletedRecorder CreateEntity(SessionUnit entity, Message message, string deviceId)
    {
        return new DeletedRecorder(entity, message.Id, deviceId);
    }

    protected override DeletedRecorder CreateEntity(Guid sessionUnitId, long messageId)
    {
        return new DeletedRecorder(sessionUnitId, messageId);
    }

    protected override async Task ChangeMessageIfNotContainsAsync(SessionUnit sessionUnit, Message message)
    {
        //message.DeletedCount++;
        message.DeletedCounter ??= new DeletedCounter();

        message.DeletedCounter.Count++;

        await Task.Yield();

        //await MessageReadOnlyRepository.IncrementDeletedCountAsync(new List<long>() { message.Id});
    }

    protected override async Task ChangeMessagesIfNotContainsAsync(SessionUnit sessionUnit, List<Message> changeMessages)
    {
        //foreach (Message message in changeMessages)
        //{
        //    //message.DeletedCount++;

        //    message.DeletedCounter.Count++;
        //}
        //await Task.Yield();

        //await MessageReadOnlyRepository.IncrementDeletedCountAsync(changeMessages.Select(x => x.Id).ToList());

        await BackgroundJobManager.EnqueueAsync(new DeletedCounterArgs()
        {
            MessageIdList = changeMessages.Select(x => x.Id).ToList()
        });
    }

    public virtual async Task<List<long>> GetDeletedMessageIdListAsync(Guid sessionUnitId)
    {
        return await DeletedRecorderDistributedCache.GetOrAddAsync(new DeletedRecorderCacheKey(sessionUnitId), async () =>
        {
            var queryable = await Repository.GetQueryableAsync();
            var deletedIds = await queryable
                .Where(x => x.SessionUnitId == sessionUnitId)
                .Select(x => x.MessageId)
                .ToListAsync();
            return deletedIds;
        });
    }

    public virtual async Task RemoveCacheAsync(Guid sessionUnitId)
    {
         await DeletedRecorderDistributedCache.RemoveAsync(new DeletedRecorderCacheKey(sessionUnitId));
    }
}
