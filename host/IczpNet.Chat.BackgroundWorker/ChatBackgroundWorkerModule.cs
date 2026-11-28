using IczpNet.Chat.AiRuns;
using IczpNet.Chat.AiProviders;
using IczpNet.Chat.IdentityUsers;
using IczpNet.Chat.Medias;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Options;
using IczpNet.RedisDistributedEventBus;
using IczpNet.AbpCommons.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.AspNetCore;
using Volo.Abp.Mapperly;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.BackgroundWorkers.Quartz;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.Minio;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Caching;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace IczpNet.Chat;

[DependsOn(typeof(ChatApplicationModule), typeof(ChatEntityFrameworkCoreModule), typeof(AbpAutofacModule),
    typeof(AbpBackgroundJobsModule), typeof(AbpCachingStackExchangeRedisModule),
    typeof(ChatBackgroundJobsModule), typeof(ChatAiProvidersModule), typeof(AbpBackgroundWorkersQuartzModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule), typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpBlobStoringMinioModule), typeof(RedisDistributedEventBusModule),
    typeof(AbpAspNetCoreSerilogModule), typeof(Volo.Abp.DistributedLocking.AbpDistributedLockingModule))]
public class ChatBackgroundWorkerModule : AbpModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        // The initialization scope is disposed after startup. Entity mapping in
        // Quartz jobs must use the application's root provider instead.
        context.GetApplicationBuilder().ApplicationServices.UseStaticObjectMapper();
    }

    public override async System.Threading.Tasks.Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        if (context.ServiceProvider.GetRequiredService<IConfiguration>().GetValue("Chat:Workers:EnablePeriodic", true))
        {
            await context.AddBackgroundWorkerAsync<IczpNet.Chat.SessionUnits.SessionUnitFlushWorker>();
            await context.AddBackgroundWorkerAsync<IczpNet.Chat.Blobs.TempBlobCleanupWorker>();
            await context.AddBackgroundWorkerAsync<IczpNet.Chat.Blobs.VideoGifPropagationWorker>();
            await context.AddBackgroundWorkerAsync<IczpNet.Chat.SessionUnits.SessionUnitDisposalCleanupWorker>();
            await context.AddBackgroundWorkerAsync<IczpNet.Chat.CallCenters.CallCenterQueueWorker>();
            await context.AddBackgroundWorkerAsync<IczpNet.Chat.Calls.CallCompensationWorker>();
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddUnifiedAbpTiming();
        // UrlNormalizer is used by nested ChatObject mappings in this non-HTTP host.
        context.Services.AddHttpContextAccessor();
        context.Services.Replace(ServiceDescriptor.Transient<IMediaResolver, FFMpegCoreMediaResolver>());
        context.Services.AddMapperlyObjectMapper();
        context.Services.AddSingleton<Medallion.Threading.IDistributedLockProvider>(provider =>
            new Medallion.Threading.Redis.RedisDistributedSynchronizationProvider(
                provider.GetRequiredService<StackExchange.Redis.IConnectionMultiplexer>().GetDatabase()));
        var configuration = context.Services.GetConfiguration();
        context.Services.AddHttpClient();
        Configure<IczpNet.Chat.Calls.LiveKitOptions>(configuration.GetSection(IczpNet.Chat.Calls.LiveKitOptions.SectionName));
        Configure<MessageOptions>(options =>
        {
            var section = configuration.GetSection("MessageOptions");
            options.OutputTempPath = section["OutputTempPath"] ?? options.OutputTempPath;
            options.ImageSetting.ThumbnailSize = section.GetValue("ImageSetting:ThumbnailSize", options.ImageSetting.ThumbnailSize);
            options.ImageSetting.BigSize = section.GetValue("ImageSetting:BigSize", options.ImageSetting.BigSize);
            options.VideoSetting.IsGenerateSnapshot = section.GetValue("VideoSetting:IsGenerateSnapshot", options.VideoSetting.IsGenerateSnapshot);
            options.VideoSetting.ImgCaptureSeconds = section.GetValue("VideoSetting:ImgCaptureSeconds", options.VideoSetting.ImgCaptureSeconds);
            options.VideoSetting.GifSeconds = section.GetValue("VideoSetting:GifSeconds", options.VideoSetting.GifSeconds);
            options.VideoSetting.SnapshotThumbnailSize = section.GetValue("VideoSetting:SnapshotThumbnailSize", options.VideoSetting.SnapshotThumbnailSize);
            options.VideoSetting.ImgSnapshotSize = ParseSize(section["VideoSetting:ImgSnapshotSize"], options.VideoSetting.ImgSnapshotSize);
            options.VideoSetting.GifSnapshotSize = ParseSize(section["VideoSetting:GifSnapshotSize"], options.VideoSetting.GifSnapshotSize);
        });
        Configure<AttachmentUploadOptions>(options =>
        {
            configuration.GetSection("AttachmentUpload").Bind(options);
        });
        Configure<AbpBackgroundJobWorkerOptions>(options => options.DefaultTimeout = 864000);
        Configure<AbpDistributedCacheOptions>(options => options.KeyPrefix = configuration["Cache:KeyPrefix"] ?? "Chat:");
        Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = true);
        Configure<AbpBackgroundWorkerOptions>(options => options.IsEnabled = true);
        // Redis Pub/Sub broadcasts to every Host. This worker owns the two durable
        // Chat business handlers; SignalR/connection handlers remain on Chat API.
        PostConfigure<Volo.Abp.EventBus.Distributed.AbpDistributedEventBusOptions>(options =>
        {
            options.Handlers.Clear();
            options.Handlers.Add(typeof(MessageSentDistributedEventHandler));
            options.Handlers.Add(typeof(IczpNet.Chat.CallCenters.CallCenterMessageSentHandler));
            options.Handlers.Add(typeof(SeedDataByUserCreatedDistributedEventHandler));
        });
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

    private static (int Width, int Height) ParseSize(string value, (int Width, int Height) fallback)
    {
        if (string.IsNullOrWhiteSpace(value)) return fallback;
        var parts = value.Split(',');
        if (parts.Length != 2 || !int.TryParse(parts[0], out var width) || !int.TryParse(parts[1], out var height))
            throw new InvalidOperationException("MessageOptions video size must contain two comma-separated integers.");
        return (width, height);
    }
}
