using IczpNet.AbpCommons;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Devices;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Settings;

namespace IczpNet.Chat.Bases;

public abstract class RecorderManager<TEntity>(IRepository<TEntity> repository) : DomainService, IRecorderManager<TEntity> where TEntity : class, IEntity, IMessageId, ISessionUnitId
{
    protected IRepository<TEntity> Repository { get; } = repository;
    protected virtual IReadOnlyRepository<Message, long> MessageRepository => LazyServiceProvider.LazyGetService<IReadOnlyRepository<Message, long>>();
    protected virtual IReadOnlyRepository<SessionUnit, Guid> SessionUnitRepository => LazyServiceProvider.LazyGetService<IReadOnlyRepository<SessionUnit, Guid>>();
    protected IBackgroundJobManager BackgroundJobManager => LazyServiceProvider.LazyGetService<IBackgroundJobManager>();
    protected IDeviceResolver DeviceIdResolver => LazyServiceProvider.LazyGetService<IDeviceResolver>();
    protected ISettingProvider SettingProvider => LazyServiceProvider.LazyGetService<ISettingProvider>();
    protected abstract TEntity CreateEntity(SessionUnit sessionUnit, Message message, string deviceId);
    protected abstract TEntity CreateEntity(Guid sessionUnitId, long messageId);
    protected abstract Task ChangeMessageIfNotContainsAsync(SessionUnit sessionUnit, Message message);
    protected abstract Task ChangeMessagesIfNotContainsAsync(SessionUnit sessionUnit, List<Message> changeMessages);

    /// <inheritdoc/>
    public virtual Task<bool> IsAnyAsync(Guid sessionUnitId, long messageId)
    {
        return Repository.AnyAsync(x => x.SessionUnitId == sessionUnitId && x.MessageId == messageId);
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

    public async Task<int> GetCountByMessageIdAsync(long messageId)
    {
        return await Repository.CountAsync(x => x.MessageId == messageId);
    }

    public virtual async Task<List<long>> GetRecorderMessageIdListAsync(Guid sessionUnitId, List<long> messageIdList)
    {
        if (messageIdList == null || messageIdList.Count == 0)
        {
            return [];
        }
        return [.. (await Repository.GetQueryableAsync())
            .Where(x => x.SessionUnitId == sessionUnitId)
            .Where(x => messageIdList.Contains(x.MessageId))
            .Select(x => x.MessageId)];
    }

    /// <inheritdoc/>
    public virtual async Task<IQueryable<SessionUnit>> QueryRecordedAsync(long messageId)
    {
        var recordedSessionUnitIdList = (await Repository.GetQueryableAsync())
            .Where(x => x.MessageId == messageId)
            .Select(x => x.SessionUnitId);

        return (await SessionUnitRepository.GetQueryableAsync())
            .Where(x => recordedSessionUnitIdList.Contains(x.Id));
    }

    /// <inheritdoc/>
    public virtual async Task<IQueryable<SessionUnit>> QueryUnrecordedAsync(long messageId)
    {
        var message = await MessageRepository.GetAsync(messageId);

        var recordedSessionUnitIdList = (await Repository.GetQueryableAsync())
            .Where(x => x.MessageId == messageId)
            .Select(x => x.SessionUnitId);

        var query = (await SessionUnitRepository.GetQueryableAsync())
            .Where(x => x.SessionId == message.SessionId)
            .Where(SessionUnit.GetActivePredicate(message.CreationTime));

        if (message.IsPrivate)
        {
            return query.Where(x => x.Id == message.SenderSessionUnitId || (x.OwnerId == message.ReceiverId && x.DestinationId == message.SenderId));
        }

        return query.Where(x => !recordedSessionUnitIdList.Contains(x.Id));
    }

    public virtual async Task<TEntity> CreateIfNotContainsAsync(SessionUnit sessionUnit, long messageId, string deviceId)
    {
        var message = await MessageRepository.GetAsync(messageId);

        Assert.If(sessionUnit.SessionId != message.SessionId, $"Not in same session,messageId:{messageId}");

        var recorder = await Repository.FindAsync(x => x.SessionUnitId == sessionUnit.Id && x.MessageId == messageId);

        if (recorder == null)
        {
            await ChangeMessageIfNotContainsAsync(sessionUnit, message);
            return await Repository.InsertAsync(CreateEntity(sessionUnit, message, deviceId), autoSave: true);
        }
        else
        {
            //recorder.DeviceId = deviceId;
        }

        return recorder;
    }


    public virtual async Task<List<TEntity>> CreateManyAsync(SessionUnit sessionUnit, List<long> messageIdList, string deviceId)
    {
        var dbMessageList = (await MessageRepository.GetQueryableAsync())
            .Where(x => x.SessionId == sessionUnit.SessionId && messageIdList.Contains(x.Id))
            .ToList()
            ;

        if (dbMessageList.Count == 0)
        {
            return [];
        }

        var dbMessageIdList = dbMessageList.Select(x => x.Id).ToList();

        var recordedMessageIdList = (await Repository.GetQueryableAsync())
            .Where(x => x.SessionUnitId == sessionUnit.Id && dbMessageIdList.Contains(x.MessageId))
            .Select(x => x.MessageId)
            .ToList()
            ;

        var newMessageIdList = dbMessageIdList.Except(recordedMessageIdList);

        if (!newMessageIdList.Any())
        {
            return [];
        }

        var changeMessages = dbMessageList.Where(x => newMessageIdList.Contains(x.Id)).ToList();

        var entities = changeMessages
            .Select(x => CreateEntity(sessionUnit, x, deviceId))
            .ToList();

        await ChangeMessagesIfNotContainsAsync(sessionUnit, changeMessages);

        await Repository.InsertManyAsync(entities, autoSave: true);

        //notice :IChatPusher.
        //...

        return entities;
    }



    protected virtual async Task<IQueryable<Guid>> QuerySessionUnitIdListAsync(long messageId)
    {
        var message = await MessageRepository.GetAsync(messageId);

        var recordedSessionUnitIdList = (await Repository.GetQueryableAsync())
            .Where(x => x.MessageId == messageId)
            .Select(x => x.SessionUnitId)
            ;

        return (await SessionUnitRepository.GetQueryableAsync())
             .Where(x => x.SessionId == message.SessionId)
             .Where(x => !recordedSessionUnitIdList.Contains(x.Id))
             .Select(x => x.Id)
             ;
    }

    public virtual async Task<string> CheckDeviceIdAsync(string inputDeviceId, bool allowEmpty = true)
    {
        Assert.If(!allowEmpty && string.IsNullOrWhiteSpace(inputDeviceId), $"Fail DeviceId(Empty):{inputDeviceId}");

        var deviceId = await DeviceIdResolver.GetDeviceIdAsync();

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return inputDeviceId;
        }

        Assert.If(!deviceId.Equals(inputDeviceId), $"Fail DeviceId:{inputDeviceId}");

        return deviceId;
    }
}
