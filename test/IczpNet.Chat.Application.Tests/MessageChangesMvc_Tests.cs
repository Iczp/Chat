using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.MessageServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using Xunit;

namespace IczpNet.Chat;

[DependsOn(typeof(AbpAspNetCoreMvcModule))]
public class MessageChangesMvcTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options => options.ConventionalControllers.Create(
            typeof(ChatApplicationModule).Assembly, settings =>
                settings.RootPath = ChatRemoteServiceConsts.RemoteServiceName.ToLowerInvariant()));
    }
}

public class MessageChangesMvc_Tests
{
    [Fact]
    public async Task ABP_conventions_register_message_change_auto_api_routes_and_message_by_ids()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
        await builder.AddApplicationAsync<MessageChangesMvcTestModule>();
        await using var app = builder.Build();
        await app.InitializeApplicationAsync();
        app.MapControllers();

        var allEndpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .ToList();

        var changeEndpoints = allEndpoints
            .Where(endpoint => endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType()
                == typeof(MessageChangeAppService))
            .ToList();

        foreach (var route in new[] { "pull", "ack" })
        {
            Assert.Contains(changeEndpoints, endpoint =>
                endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/message-change/" + route &&
                endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("POST") == true);
        }

        var messageEndpoints = allEndpoints
            .Where(endpoint => endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType()
                == typeof(MessageAppService))
            .ToList();

        Assert.Contains(messageEndpoints, endpoint =>
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/message/by-ids" &&
            endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("POST") == true);
    }
}
