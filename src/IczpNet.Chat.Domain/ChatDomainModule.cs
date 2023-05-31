using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Connections;
using IczpNet.Chat.HttpRequests;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.TextTemplates;
using IczpNet.Chat.UnitTests;
using IczpNet.Pusher;
using IczpNet.Pusher.ShortIds;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AutoMapper;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain;
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
public class ChatDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<ChatDomainModule>();

        context.Services.AddHttpClient(HttpRequest.ClientName);

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
