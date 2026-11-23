using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Menus;

/// <summary>
/// 菜单统计数据定时落库后台任务（Write-Behind 异步刷盘保障）
/// </summary>
public class MenuStatFlushWorker : AsyncPeriodicBackgroundWorkerBase
{
    public MenuStatFlushWorker(
        IMenuStatisticManager menuStatisticManager,
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory) : base(timer, serviceScopeFactory)
    {
        Timer.Period = 60 * 1000; // 60秒刷盘一次
        MenuStatisticManager = menuStatisticManager;
    }

    public IMenuStatisticManager MenuStatisticManager { get; }

    [UnitOfWork]
    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var stopwatch = Stopwatch.StartNew();

        var affected = await MenuStatisticManager.FlushDirtyAsync();

        if (affected > 0)
        {
            Logger.LogInformation("{handler} Flush Affected:{affected}, Elapsed:{elapsed}ms", nameof(MenuStatFlushWorker), affected, stopwatch.ElapsedMilliseconds);
        }
    }
}
