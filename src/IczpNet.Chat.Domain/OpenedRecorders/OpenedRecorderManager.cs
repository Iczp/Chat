using IczpNet.Chat.Bases;
using IczpNet.Chat.MessageSections.Counters;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.OpenedRecorders;

public class OpenedRecorderManager : RecorderManager<OpenedRecorder>, IOpenedRecorderManager
{
    public OpenedRecorderManager(IRepository<OpenedRecorder> repository) : base(repository)
    {

    }

    protected override OpenedRecorder CreateEntity(SessionUnit entity, Message message, string deviceId)
    {
        return new OpenedRecorder(entity, message.Id, deviceId);
    }

    protected override OpenedRecorder CreateEntity(Guid sessionUnitId, long messageId)
    {
        return new OpenedRecorder(sessionUnitId, messageId);
    }

    protected override async Task ChangeMessageIfNotContainsAsync(SessionUnit sessionUnit, Message message)
    {
        //message.OpenedCount++;

        message.OpenedCounter.Count++;

        await Task.Yield();

        //await MessageRepository.IncrementOpenedCountAsync(new List<long>() { message.Id});
    }

    protected override async Task ChangeMessagesIfNotContainsAsync(SessionUnit sessionUnit, List<Message> changeMessages)
    {
        //foreach (Message message in changeMessages)
        //{
        //    //message.OpenedCount++;

        //    message.OpenedCounter.Count++;
        //}
        //await Task.Yield();

        //await MessageRepository.IncrementOpenedCountAsync(changeMessages.Select(x => x.Id).ToList());

        await BackgroundJobManager.EnqueueAsync(new OpenedCounterArgs()
        {
            MessageIdList = changeMessages.Select(x => x.Id).ToList()
        });
    }
}
