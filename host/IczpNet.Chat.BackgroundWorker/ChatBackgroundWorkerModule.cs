using IczpNet.Chat.AiRuns;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.RedisDistributedEventBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.Minio;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace IczpNet.Chat;

[DependsOn(typeof(ChatApplicationModule), typeof(ChatEntityFrameworkCoreModule), typeof(AbpAutofacModule),
    typeof(AbpBackgroundJobsModule), typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule), typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpBlobStoringMinioModule), typeof(RedisDistributedEventBusModule),
    typeof(AbpAspNetCoreSerilogModule), typeof(Volo.Abp.DistributedLocking.AbpDistributedLockingModule))]
public class ChatBackgroundWorkerModule : AbpModule
{
    public override async System.Threading.Tasks.Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<IczpNet.Chat.SessionUnits.SessionUnitFlushWorker>();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<Medallion.Threading.IDistributedLockProvider>(provider =>
            new Medallion.Threading.Redis.RedisDistributedSynchronizationProvider(
                provider.GetRequiredService<StackExchange.Redis.IConnectionMultiplexer>().GetDatabase()));
        var configuration = context.Services.GetConfiguration();
        Configure<AbpBackgroundJobWorkerOptions>(options => options.DefaultTimeout = 864000);
        Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = true);
        Configure<AbpBackgroundWorkerOptions>(options => options.IsEnabled = true);
        Configure<AbpDbContextOptions>(options => options.UseSqlServer(sql => sql.CommandTimeout(60000)));
        Configure<AbpBlobStoringOptions>(options => options.Containers.ConfigureDefault(container => container.UseMinio(minio =>
        {
            minio.EndPoint = configuration["Blob:Minio:EndPoint"] ?? throw new InvalidOperationException("Blob:Minio:EndPoint is required.");
            minio.AccessKey = configuration["Blob:Minio:AccessKey"] ?? throw new InvalidOperationException("Blob:Minio:AccessKey is required.");
            minio.SecretKey = configuration["Blob:Minio:SecretKey"] ?? throw new InvalidOperationException("Blob:Minio:SecretKey is required.");
            minio.WithSSL = string.Equals(configuration["Blob:Minio:WithSSL"], "true", StringComparison.OrdinalIgnoreCase);
            minio.CreateBucketIfNotExists = true;
        })));
        context.Services.AddHostedService<AiRunDispatcher>();
    }
}
