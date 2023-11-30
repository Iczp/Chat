using IczpNet.Chat.ChatPushers;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionUnits
{
    public class SessionUnitIncrementJob : AsyncBackgroundJob<SessionUnitIncrementArgs>, ITransientDependency
    {
        protected IUnitOfWorkManager UnitOfWorkManager { get; }
        protected ISessionUnitManager SessionUnitManager { get; }
        protected IChatPusher ChatPusher { get; }
        protected IObjectMapper ObjectMapper { get; }

        public SessionUnitIncrementJob(
            IUnitOfWorkManager unitOfWorkManager,
            ISessionUnitManager sessionUnitManager,
            IChatPusher chatPusher,
            IObjectMapper objectMapper)
        {
            UnitOfWorkManager = unitOfWorkManager;
            SessionUnitManager = sessionUnitManager;
            ChatPusher = chatPusher;
            ObjectMapper = objectMapper;
        }

        [UnitOfWork]
        public override async Task ExecuteAsync(SessionUnitIncrementArgs args)
        {
            using var uow = UnitOfWorkManager.Begin();

            var totalCount = await SessionUnitManager.IncremenetAsync(args);

            Logger.LogInformation($"SessionUnitIncrementJob Completed totalCount:{totalCount}.");

            await ChatPusher.ExecuteBySessionIdAsync(args.SessionId, new IncrementCompletedCommandPayload()
            {
                MessageId = args.LastMessageId
            });
        }
    }
}
