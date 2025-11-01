using DeviceDetectorNET.Parser.Device;
using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.HttpRequests;
using IczpNet.Chat.ListSets;
using IczpNet.Chat.SessionUnits;
using IczpNet.Pusher;
using IczpNet.Pusher.ShortIds;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AutoMapper;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain;
using Volo.Abp.Imaging;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.Identity;
using Volo.Abp.SimpleStateChecking;

namespace IczpNet.Chat;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(ChatDomainSharedModule),
    typeof(AbpAutoMapperModule)
)]

[DependsOn(typeof(AbpCommonsDomainModule))]
[DependsOn(typeof(AbpTreesDomainModule))]
//[DependsOn(typeof(AbpIdentityDomainModule))]
[DependsOn(typeof(AbpPermissionManagementDomainIdentityModule))]
[DependsOn(typeof(PusherDomainModule))]
[DependsOn(typeof(AbpBackgroundJobsAbstractionsModule))]

[DependsOn(typeof(AbpBlobStoringModule))]
//[DependsOn(typeof(AbpBlobStoringMinioModule))] //minio
[DependsOn(typeof(AbpImagingAbstractionsModule))]
    public class ChatDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<ChatDomainModule>();

        context.Services.AddHttpClient(HttpRequest.ClientName);

        context.Services.AddTransient(typeof(IDistributedCacheListSet<,>), typeof(DistributedCacheListSet<,>));

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<ChatDomainModule>(validate: true);
        });

        Configure<ShortIdOptions>(options =>
        {
            options.Length = 16;
            options.UseNumbers = false;
            options.UseSpecialCharacters = false;
        });

        Configure<AbpSimpleStateCheckerOptions<ChatObject>>(options =>
        {
            options.GlobalStateCheckers.Add<ChatObjectStateChecker>();
        });
        Configure<AbpSimpleStateCheckerOptions<SessionUnit>>(options =>
        {
            options.GlobalStateCheckers.Add<SessionUnitStateChecker>();
        });
    }

    public override async Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        //await context.AddBackgroundWorkerAsync<ConnectionWorker>();
        //await context.AddBackgroundWorkerAsync<SendMessageWorker>();


        //await context.AddBackgroundWorkerAsync<SendToRoomUnitTestWorker>();
        //await context.AddBackgroundWorkerAsync<SessionUnitRequestUnitTestWorker>();
        //await context.AddBackgroundWorkerAsync<RecorderUnitTestWorker>();

        await base.OnPostApplicationInitializationAsync(context);
    }
}
