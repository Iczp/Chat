using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Connections
{
    public class ConnectionWorker : AsyncPeriodicBackgroundWorkerBase
    {
        protected IConnectionManager ConnectionManager { get; }
        public ConnectionWorker(AbpAsyncTimer timer,
            IServiceScopeFactory serviceScopeFactory,
            IOptions<ConnectionOptions> options,

            IConnectionManager connectionManager) : base(timer, serviceScopeFactory)
        {
            Timer.Period = options.Value.TimerPeriodSeconds * 1000; //5 seconds
            ConnectionManager = connectionManager;
        }

        [UnitOfWork]
        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            var startTicks = DateTime.Now.Ticks;

            Logger.LogInformation($"ConnectionWorker running:{DateTime.Now}, Timer.Period:{Timer.Period}ms");

            var count = await ConnectionManager.DeleteInactiveAsync();

            double ticks = (DateTime.Now.Ticks - startTicks); /// 10000;

            Logger.LogInformation($"ConnectionWorker delete inactive connection count:{count},run ticks:{ticks}");
        }
    }
}
