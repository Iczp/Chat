using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundJobs.Quartz;
using Volo.Abp.Modularity;
using Volo.Abp.Quartz;

namespace IczpNet.Chat;

/// <summary>Persistent Chat job queue, isolated from the main IM Quartz scheduler.</summary>
[DependsOn(typeof(AbpBackgroundJobsQuartzModule))]
public class ChatBackgroundJobsModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        PreConfigure<AbpQuartzOptions>(options =>
        {
            var connectionString = configuration.GetConnectionString("Quartz");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("ConnectionStrings:Quartz is required for Chat jobs.");

            options.Properties["quartz.scheduler.instanceName"] = configuration["Quartz:SchedulerName"] ?? "IczpNet.Chat";
            options.Properties["quartz.scheduler.instanceId"] = "AUTO";
            options.Properties["quartz.threadPool.type"] = "Quartz.Simpl.DefaultThreadPool, Quartz";
            options.Properties["quartz.threadPool.maxConcurrency"] = configuration["Quartz:MaxConcurrency"] ?? "10";
            options.Properties["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";
            options.Properties["quartz.jobStore.driverDelegateType"] = "Quartz.Impl.AdoJobStore.SqlServerDelegate, Quartz";
            options.Properties["quartz.jobStore.dataSource"] = "ChatQuartz";
            options.Properties["quartz.jobStore.tablePrefix"] = "QRTZ_";
            options.Properties["quartz.jobStore.useProperties"] = "true";
            options.Properties["quartz.jobStore.clustered"] = "true";
            options.Properties["quartz.jobStore.clusterCheckinInterval"] = "10000";
            options.Properties["quartz.jobStore.performSchemaValidation"] = "true";
            options.Properties["quartz.dataSource.ChatQuartz.provider"] = "SqlServer";
            options.Properties["quartz.dataSource.ChatQuartz.connectionString"] = connectionString;
            options.Properties["quartz.serializer.type"] = "stj";
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
        var configuration = context.Services.GetConfiguration();
        Configure<AbpBackgroundJobQuartzOptions>(options =>
        {
            options.RetryCount = configuration.GetValue("Quartz:RetryCount", 10);
            options.RetryIntervalMillisecond = configuration.GetValue("Quartz:RetryIntervalMilliseconds", 3000);
        });
    }
}
