using IczpNet.Chat.ChatPushers;
using IczpNet.Chat.CommandPayloads;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitIncrementJob(
    IUnitOfWorkManager unitOfWorkManager,
    ISessionUnitManager sessionUnitManager,
    IChatPusher chatPusher,
    IObjectMapper objectMapper) : AsyncBackgroundJob<SessionUnitIncrementJobArgs>, ITransientDependency
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    protected IChatPusher ChatPusher { get; } = chatPusher;
    protected IObjectMapper ObjectMapper { get; } = objectMapper;

    [UnitOfWork]
    public override async Task ExecuteAsync(SessionUnitIncrementJobArgs args)
    {
        using var uow = UnitOfWorkManager.Begin();

        var totalCount = await SessionUnitManager.IncremenetAsync(args);

        Logger.LogInformation($"SessionUnitIncrementJob Completed totalCount:{totalCount}.");

        //消息角标更新成功后通知前端
        await ChatPusher.ExecuteBySessionIdAsync(args.SessionId, new IncrementCompletedCommandPayload()
        {
            MessageId = args.LastMessageId
        });
    }
}
