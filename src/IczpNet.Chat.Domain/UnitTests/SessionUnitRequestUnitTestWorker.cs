using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.SessionRequests;
using IczpNet.Chat.SessionSections.SessionUnits;
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
    public class SessionUnitRequestUnitTestWorker : AsyncPeriodicBackgroundWorkerBase
    {
        private static List<long> ChatObjectIdList;

        private static readonly long? RoomId = 6019;

        protected ISessionUnitRepository SessionUnitRepository { get; }
        protected ISessionRequestManager SessionRequestManager { get; }
        protected IChatObjectManager ChatObjectManager { get; }


        public SessionUnitRequestUnitTestWorker(AbpAsyncTimer timer,
            IServiceScopeFactory serviceScopeFactory,
            ISessionUnitRepository sessionUnitRepository,
            ISessionRequestManager sessionRequestManager,
            IChatObjectManager chatObjectManager) : base(timer, serviceScopeFactory)
        {
            Timer.Period = 444;
            SessionUnitRepository = sessionUnitRepository;
            SessionRequestManager = sessionRequestManager;
            ChatObjectManager = chatObjectManager;
        }

        [UnitOfWork]
        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            await Task.CompletedTask;

            var stopWatch = Stopwatch.StartNew();

            Logger.LogInformation($"SessionUnitRequestUnitTestWorker");

            var items = await GetChatObjectIdListAsync(RoomId.Value);

            var owner = await ChatObjectManager.GetAsync(items[new Random().Next(0, items.Count - 1)]);

            var destination = await ChatObjectManager.GetAsync(items[new Random().Next(0, items.Count - 1)]);

            var room = await ChatObjectManager.GetAsync(RoomId.Value);

            var requestMessage = $"{owner.Name} 通过群'{room.Name}' 请求添加 {destination.Name} 为好友";

            Logger.LogInformation(requestMessage);
            try
            {
                var entity = await SessionRequestManager.CreateRequestAsync(owner.Id, destination.Id, requestMessage);

                Logger.LogInformation($"请求成功: {requestMessage}");
            }
            catch (Exception ex)
            {

                Logger.LogWarning($"请求失败: {ex.Message}");
                throw;
            }

            stopWatch.Stop();

            Logger.LogInformation($"SessionUnitRequestUnitTestWorker completed:{stopWatch.ElapsedMilliseconds}ms");

        }

        protected async Task<List<long>> GetChatObjectIdListAsync(long roomId)
        {
            return ChatObjectIdList ??= (await SessionUnitRepository.GetQueryableAsync())
                .Where(x => x.DestinationId == roomId)
                .Select(x => x.OwnerId)
                .ToList();
        }
    }
}
