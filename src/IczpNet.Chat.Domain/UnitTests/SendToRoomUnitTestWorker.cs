using AutoMapper.Internal;
using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace IczpNet.Chat.UnitTests;

public class SendToRoomUnitTestWorker : AsyncPeriodicBackgroundWorkerBase
{
    private static List<Guid> SessionUnitIdList;

    private static long? RoomId = 6019;

    private static int Index = 0;

    protected IMessageSender MessageSender { get; }

    protected IRoomManager RoomManager { get; }

    protected ISessionUnitManager SessionUnitManager { get; }

    protected ISessionUnitRepository SessionUnitRepository { get; }
    protected IFollowManager FollowManager { get; }
    protected IFavoritedRecorderManager FavoriteManager { get; }
    protected IOpenedRecorderManager OpenedRecorderManager { get; }
    protected IReadedRecorderManager ReadedRecorderManager { get; }

    public SendToRoomUnitTestWorker(AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        ISessionUnitRepository sessionUnitRepository,
        IMessageSender messageSender,
        IRoomManager roomManager,
        ISessionUnitManager sessionUnitManager,
        IFollowManager followManager,
        IFavoritedRecorderManager favoriteManager,
        IOpenedRecorderManager openedRecorderManager,
        IReadedRecorderManager readedRecorderManager) : base(timer, serviceScopeFactory)
    {
        Timer.Period = 500;
        SessionUnitRepository = sessionUnitRepository;
        MessageSender = messageSender;
        RoomManager = roomManager;
        SessionUnitManager = sessionUnitManager;
        FollowManager = followManager;
        FavoriteManager = favoriteManager;
        OpenedRecorderManager = openedRecorderManager;
        ReadedRecorderManager = readedRecorderManager;
    }

    [UnitOfWork]
    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        await Task.Yield();

        var stopWatch = Stopwatch.StartNew();

        Logger.LogInformation($" ------------------- SendToRoomUnitTestWorker Starting ------------------- ");

        RoomId ??= (await RoomManager.CreateByAllUsersAsync($"Auto-create:{DateTime.Now}")).Id;

        var items = await GetSessionIdListAsync(RoomId.Value);

        var sessionunitId = items[new Random().Next(0, items.Count - 1)];

        var sessionunit = await SessionUnitRepository.GetAsync(sessionunitId);

        Logger.LogInformation($"Sender sessionunit: id:{sessionunit?.Id},name:{sessionunit?.Owner?.Name}");

        //// Following
        //await FollowingAsync(items);

        Index++;

        var remindList = new List<Guid>();

        for (int i = 0; i < new Random().Next(1, 10); i++)
        {
            remindList.TryAdd(items[new Random().Next(0, items.Count - 1)]);
        }

        var text = $"RoomId:{RoomId}, sessionunitId:{sessionunitId}, remindCount:{remindList.Count}";

        if (new Random().Next(3) % 3 == 1)
        {
            text = $"@陈忠培 {text}";
        }

        var sendResult = await MessageSender.SendTextAsync(sessionunit, new MessageInput<TextContentInfo>()
        {
            Content = new TextContentInfo()
            {
                Text = text
            },
            RemindList = remindList
        });
        Logger.LogInformation($"SendText: {text}");

        ////Recorder
        //for (int i = 0; i < new Random().Next(1, 10); i++)
        //{
        //    await SetFavoritedAsync(items, sendResult.Id);

        //    await SetOpenedAsync(items, sendResult.Id);

        //    await SetReadedAsync(items, sendResult.Id);
        //}

        stopWatch.Stop();

        Logger.LogInformation($" ------------------- SendToRoomUnitTestWorker stopWatch:{stopWatch.ElapsedMilliseconds} -------------------");
    }

    private async Task SetReadedAsync(List<Guid> items, long messageId)
    {
        var sessionunit = await SessionUnitRepository.GetAsync(items[new Random().Next(0, items.Count - 1)]);

        var entity = await ReadedRecorderManager.CreateIfNotContainsAsync(sessionunit, messageId, "");

        Logger.LogInformation($"SetReadedAsync messageId:{entity.MessageId},sessionUnitId:{entity.SessionUnitId}");
    }

    private async Task SetOpenedAsync(List<Guid> items, long messageId)
    {
        var sessionunit = await SessionUnitRepository.GetAsync(items[new Random().Next(0, items.Count - 1)]);

        var entity = await OpenedRecorderManager.CreateIfNotContainsAsync(sessionunit, messageId, "");

        Logger.LogInformation($"SetOpenedAsync messageId:{entity.MessageId},sessionUnitId:{entity.SessionUnitId}");
    }

    private async Task SetFavoritedAsync(List<Guid> items, long messageId)
    {
        var sessionunit = await SessionUnitRepository.GetAsync(items[new Random().Next(0, items.Count - 1)]);

        var entity = await FavoriteManager.CreateIfNotContainsAsync(sessionunit, messageId, "");

        Logger.LogInformation($"SetFavoritedAsync messageId:{entity.MessageId},sessionUnitId:{entity.SessionUnitId}");
    }

    private async Task FollowingAsync(List<Guid> items)
    {
        var ownerId = items[new Random().Next(0, items.Count - 1)];

        if (await FollowManager.GetFollowingCountAsync(ownerId) > 10)
        {
            return;
        }

        var tagId = items[new Random().Next(0, items.Count - 1)];

        await FollowManager.CreateAsync(ownerId, new List<Guid>() { tagId });

        Logger.LogInformation($"Following sessionunit: id:{ownerId},tagId:{tagId}");
    }

    protected async Task<List<Guid>> GetSessionIdListAsync(long roomId)
    {
        return SessionUnitIdList ??= (await SessionUnitRepository.GetQueryableAsync())
            .Where(x => x.DestinationId == roomId)
            .Select(x => x.Id)
            .ToList();
    }
}
