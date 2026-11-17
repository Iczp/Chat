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
    public async Task ABP_conventions_register_the_explicit_V2_routes_and_existing_V1_routes()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
        await builder.AddApplicationAsync<MessageChangesMvcTestModule>();
        await using var app = builder.Build();
        await app.InitializeApplicationAsync();
        app.MapControllers();

        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType()
                == typeof(MessageAppService)).ToList();

        foreach (var route in new[] { "changes/pull", "changes/ack", "by-ids" })
            Assert.Contains(endpoints, endpoint =>
                endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/message/" + route &&
                endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("POST") == true);
    }
}
