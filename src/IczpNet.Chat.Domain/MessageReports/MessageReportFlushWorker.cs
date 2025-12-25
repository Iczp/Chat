using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageReports;

public class MessageReportFlushWorker : AsyncPeriodicBackgroundWorkerBase
{
    public MessageReportFlushWorker(
        IMessageReportManager messageReportManager,
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<MessageReportOptions> options) : base(timer, serviceScopeFactory)
    {
        Timer.Period = options.Value.FlushToDbTimerPeriodSeconds * 1000; //60 seconds
        MessageReportManager = messageReportManager;
    }

    public IMessageReportManager MessageReportManager { get; }

    [UnitOfWork]
    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var stopwatch = Stopwatch.StartNew();

        await MessageReportManager.FlushMonthAsync();
        await MessageReportManager.FlushDayAsync();
        await MessageReportManager.FlushHourAsync();

        Logger.LogInformation("{handler} Flush Elapsed:{elapsed}ms", nameof(MessageReportFlushWorker), stopwatch.ElapsedMilliseconds);
    }
}
