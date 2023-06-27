using AutoMapper.Internal;
using IczpNet.Chat.DataFilters;
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

namespace IczpNet.Chat.UnitTests
{
    public class RecorderUnitTestWorker : AsyncPeriodicBackgroundWorkerBase
    {
        private static List<Guid> SessionUnitIdList;

        private static long? RoomId = 6019;

        protected IMessageSender MessageSender { get; }

        protected IRoomManager RoomManager { get; }

        protected ISessionUnitManager SessionUnitManager { get; }

        protected ISessionUnitRepository SessionUnitRepository { get; }
        protected IFollowManager FollowManager { get; }
        protected IFavoritedRecorderManager FavoriteManager { get; }
        protected IOpenedRecorderManager OpenedRecorderManager { get; }
        protected IReadedRecorderManager ReadedRecorderManager { get; }
        protected IMessageRepository MessageRepository { get; }

        public RecorderUnitTestWorker(AbpAsyncTimer timer,
            IServiceScopeFactory serviceScopeFactory,
            ISessionUnitRepository sessionUnitRepository,
            IMessageSender messageSender,
            IRoomManager roomManager,
            ISessionUnitManager sessionUnitManager,
            IFollowManager followManager,
            IFavoritedRecorderManager favoriteManager,
            IOpenedRecorderManager openedRecorderManager,
            IReadedRecorderManager readedRecorderManager,
            IMessageRepository messageRepository) : base(timer, serviceScopeFactory)
        {
            Timer.Period = 345;
            SessionUnitRepository = sessionUnitRepository;
            MessageSender = messageSender;
            RoomManager = roomManager;
            SessionUnitManager = sessionUnitManager;
            FollowManager = followManager;
            FavoriteManager = favoriteManager;
            OpenedRecorderManager = openedRecorderManager;
            ReadedRecorderManager = readedRecorderManager;
            MessageRepository = messageRepository;
        }

        [UnitOfWork]
        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            await Task.Yield();

            var stopWatch = Stopwatch.StartNew();

            Logger.LogInformation($" ------------------- RecorderUnitTestWorker Starting ------------------- ");

            RoomId ??= (await RoomManager.CreateByAllUsersAsync($"Auto-create:{DateTime.Now}")).Id;

            var items = await GetSessionIdListAsync(RoomId.Value);

            var sessionunitId = items[new Random().Next(0, items.Count - 1)];

            var sessionunit = await SessionUnitRepository.GetAsync(sessionunitId);

            var messageIdList = await GetMessageIdListAsync(sessionunit.SessionId.Value);

            await ReadedRecorderManager.CreateManyAsync(sessionunit, messageIdList, "RecorderUnitTestWorker");

            Logger.LogInformation($"SetReaded sessionunitId:{sessionunitId},messageIdList:[{messageIdList.JoinAsString(",")}]");

            stopWatch.Stop();

            Logger.LogInformation($" ------------------- RecorderUnitTestWorker stopWatch:{stopWatch.ElapsedMilliseconds} -------------------");
        }

        private async Task<List<long>> GetMessageIdListAsync(Guid sessionId, int count = 20)
        {
            return (await MessageRepository.GetQueryableAsync())
                .Where(x => x.SessionId == sessionId)
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id)
                .Take(count)
                .ToList()
                ;
        }

        protected async Task<List<Guid>> GetSessionIdListAsync(long roomId)
        {
            return SessionUnitIdList ??= (await SessionUnitRepository.GetQueryableAsync())
                .Where(x => x.DestinationId == roomId)
                .Select(x => x.Id)
                .ToList();
        }
    }
}
