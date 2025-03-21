﻿using IczpNet.Chat.Bases;
using IczpNet.Chat.MessageSections.Counters;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.ReadedRecorders;

public class ReadedRecorderManager(IRepository<ReadedRecorder> repository) : RecorderManager<ReadedRecorder>(repository), IReadedRecorderManager
{
    public virtual async Task<int> CreateAllAsync(long messageId)
    {
        var sessionUnitIdList = await QuerySessionUnitIdListAsync(messageId);

        var entities = sessionUnitIdList.Select(x => new ReadedRecorder(x, messageId));

        await Repository.InsertManyAsync(entities);

        return entities.Count();

    }

    protected override ReadedRecorder CreateEntity(SessionUnit sessionUnit, Message message, string deviceId)
    {
        return new ReadedRecorder(sessionUnit, message.Id, deviceId);
    }

    protected override ReadedRecorder CreateEntity(Guid sessionUnitId, long messageId)
    {
        return new ReadedRecorder(sessionUnitId, messageId);
    }

    protected override async Task ChangeMessageIfNotContainsAsync(SessionUnit sessionUnit, Message message)
    {
        //message.ReadedCount++;

        message.ReadedCounter.Count++;

        await Task.Yield();

        //await MessageReadOnlyRepository.IncrementReadedCountAsync(new List<long>() { message.Id });


    }

    protected override async Task ChangeMessagesIfNotContainsAsync(SessionUnit sessionUnit, List<Message> changeMessages)
    {
        //foreach (var message in changeMessages)
        //{
        //    //message.ReadedCount++;
        //    message.ReadedCounter.Count++;
        //}
        //await Task.Yield();

        //await MessageReadOnlyRepository.IncrementReadedCountAsync(changeMessages.Select(x => x.Id).ToList());

        await BackgroundJobManager.EnqueueAsync(new ReadedCounterArgs()
        {
             MessageIdList = changeMessages.Select(x => x.Id).ToList()
        });
    }
}
