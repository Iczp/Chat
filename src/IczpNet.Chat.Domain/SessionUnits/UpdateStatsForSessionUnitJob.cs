using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionUnits;


[Obsolete("弃用")]
public class UpdateStatsForSessionUnitJob(
    IMessageRepository mssageRepository,
    ISessionUnitManager sessionUnitManager,
    IUnitOfWorkManager unitOfWorkManager) : AsyncBackgroundJob<UpdateStatsForSessionUnitArgs>, ITransientDependency
{
    protected IMessageRepository MessageRepository { get; } = mssageRepository;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;

    [UnitOfWork]
    public override async Task ExecuteAsync(UpdateStatsForSessionUnitArgs args)
    {
        using var uow = UnitOfWorkManager.Begin();

        var senderSessionUnit = await SessionUnitManager.GetAsync(args.SenderSessionUnitId);

        var message = await MessageRepository.GetAsync(args.MessageId);

        var result = await SessionUnitManager.BatchUpdateAsync(senderSessionUnit, message);

        Logger.LogInformation($"UpdateStatsForSessionUnitJob Completed:{result}.");
    }
}
