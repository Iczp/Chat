using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.Controllers;
using IczpNet.Chat.MessageTemplates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using Xunit;

namespace IczpNet.Chat;

[DependsOn(typeof(AbpAspNetCoreMvcModule))]
public class MessageTemplateMvcTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddControllers().AddApplicationPart(typeof(MessageTemplateController).Assembly);
        Configure<AbpAspNetCoreMvcOptions>(options => options.ConventionalControllers.Create(
            typeof(ChatApplicationModule).Assembly, settings => settings.RootPath = "chat"));
    }
}

public class MessageTemplateMvc_Tests
{
    [Fact]
    public async Task Management_routes_are_unique_and_complex_post_inputs_bind_from_body()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
        await builder.AddApplicationAsync<MessageTemplateMvcTestModule>();
        await using var app = builder.Build();
        await app.InitializeApplicationAsync();
        app.MapControllers();
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(x => x.Endpoints).OfType<RouteEndpoint>().ToArray();
        foreach (var (path, verb) in new[] {
            ("api/chat/message-template", "GET"), ("api/chat/message-template", "POST"),
            ("api/chat/message-template/{id}", "GET"), ("api/chat/message-template/{id}", "PUT"),
            ("api/chat/message-template/{id}/publish", "POST"), ("api/chat/message-template/{id}/set-enabled", "POST"),
            ("api/chat/message-template/{id}/revisions", "GET"), ("api/chat/message-template/{id}/revisions/{version}", "GET"),
            ("api/chat/message-template/preview", "POST"),
            ("api/chat/message-sender/send-template-content-message/{sessionUnitId}", "POST") })
        {
            var endpoint = Assert.Single(endpoints, x => x.RoutePattern.RawText?.TrimStart('/') == path &&
                x.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains(verb) == true);
            if (verb is "POST" or "PUT")
            {
                var action = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                var input = Assert.Single(action!.Parameters, p => p.Name == "input");
                Assert.Equal("Body", input.BindingInfo?.BindingSource?.Id);
            }
        }
    }
}
