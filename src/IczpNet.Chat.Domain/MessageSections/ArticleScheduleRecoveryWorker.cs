using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace IczpNet.Chat.MessageSections;

public class ArticleScheduleRecoveryWorker : AsyncPeriodicBackgroundWorkerBase
{
    public ArticleScheduleRecoveryWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration) : base(timer, serviceScopeFactory)
    {
        Timer.Period = System.Math.Max(10, configuration.GetValue("Chat:ArticleSchedules:RecoveryPeriodSeconds", 60)) * 1000;
    }
    protected override Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext) =>
        workerContext.ServiceProvider.GetRequiredService<ArticleScheduleManager>().RecoverAsync();
}
