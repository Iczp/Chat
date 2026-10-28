using IczpNet.AppDevice;
using IczpNet.AppDevice.Devices;
using IczpNet.Chat.ChatHubs;
using IczpNet.Chat.ConnectionPools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Modularity;

namespace IczpNet.Chat.AppDevice;

[DependsOn(typeof(ChatHttpApiModule), typeof(AppDeviceAspNetCoreModule))]
public class ChatAppDeviceModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Replace(ServiceDescriptor.Transient<IChatDeviceLogout, AppDeviceChatDeviceLogout>());
        context.Services.Replace(ServiceDescriptor.Transient<IChatDeviceContext, AppDeviceChatDeviceContext>());
        context.Services.Replace(ServiceDescriptor.Transient<IChatDeviceSessionValidator, AppDeviceChatSessionValidator>());
        Configure<DeviceParsingOptions>(options => options.AllowQuery = context.Services.GetConfiguration().GetValue("AppDevice:Parsing:AllowQuery", true));
    }
}
