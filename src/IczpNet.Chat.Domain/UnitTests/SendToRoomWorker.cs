using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Connections
{
    public class SendToRoomWorker : AsyncPeriodicBackgroundWorkerBase
    {
        private static List<Guid> SessionUnitIdList;

        private static long? RoomId = null;

        private static int Index = 0;

        protected IMessageSender MessageSender { get; }

        protected IRoomManager RoomManager { get; }

        protected ISessionUnitManager SessionUnitManager { get; }

        protected ISessionUnitRepository SessionUnitRepository { get; }


        public SendToRoomWorker(AbpAsyncTimer timer,
            IServiceScopeFactory serviceScopeFactory,
            ISessionUnitRepository sessionUnitRepository,
            IMessageSender messageSender,
            IRoomManager roomManager,
            ISessionUnitManager sessionUnitManager) : base(timer, serviceScopeFactory)
        {
            Timer.Period = 1 * 1000; //1 seconds
            SessionUnitRepository = sessionUnitRepository;
            MessageSender = messageSender;
            RoomManager = roomManager;
            SessionUnitManager = sessionUnitManager;
        }

        [UnitOfWork]
        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            await Task.CompletedTask;

            Logger.LogInformation($"SendToRoomWorker");

            RoomId ??= (await RoomManager.CreateByAllUsersAsync($"Auto-create:{DateTime.Now}")).Id;

            var items = await GetSessionIdListAsync(RoomId.Value);

            var sessionunitId = items[new Random().Next(0, items.Count - 1)];

            var sessionunit = await SessionUnitManager.GetAsync(sessionunitId);

            Logger.LogInformation($"sessionunit: id:{sessionunit?.Id},name:{sessionunit?.Owner?.Name}");

            Index++;

            await MessageSender.SendTextAsync(sessionunit, new MessageSendInput<TextContentInfo>()
            {
                Content = new TextContentInfo()
                {
                    Text = $"RoomId:{RoomId}-Index-{Index}-{sessionunitId}"
                }
            });

            Logger.LogInformation($"SendText: RoomId:{RoomId}-Index-{Index}-{sessionunitId}-Name:{sessionunit?.Owner?.Name}");

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
