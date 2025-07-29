using IczpNet.Chat.Bases;
using IczpNet.Chat.MessageSections.Counters;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.DeletedRecorders;

public class DeletedRecorderManager(IRepository<DeletedRecorder> repository) : RecorderManager<DeletedRecorder>(repository), IDeletedRecorderManager
{
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
}
