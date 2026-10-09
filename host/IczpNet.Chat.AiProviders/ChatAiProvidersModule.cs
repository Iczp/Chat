using Aurora.AiConnector;
using IczpNet.Ai;
using IczpNet.Chat.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace IczpNet.Chat.AiProviders;

[DependsOn(typeof(ChatDomainModule), typeof(AiDomainModule))]
public class ChatAiProvidersModule : AbpModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        if (!context.GetConfiguration().GetValue("AiRunDispatcher:Enabled", true))
            return;

        using var scope = context.ServiceProvider.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<IAiResolver>();
        foreach (var name in new[] { "Aurora", "Gemini", "Tesseract", "Copilot", "Github" })
        {
            var providerType = resolver.GetProviderOrDefault(name)
                ?? throw new InvalidOperationException($"Chat AI provider '{name}' is not registered.");
            _ = scope.ServiceProvider.GetRequiredService(providerType);
        }

        scope.ServiceProvider.GetRequiredService<ILogger<ChatAiProvidersModule>>()
            .LogInformation("Chat AI providers resolved: Aurora, Gemini, Tesseract, Copilot, Github");
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var auroraOptions = new AuroraAiConnectorOptions();
        configuration.GetSection("Aurora").Bind(auroraOptions);
        context.Services.AddSingleton(auroraOptions);
        context.Services.AddHttpClient("AuroraAiConnector")
            .ConfigureHttpClient(client => client.Timeout = System.Threading.Timeout.InfiniteTimeSpan);
        context.Services.AddSingleton<IAuroraAiConnectorClient>(provider => new AuroraAiConnectorClient(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient("AuroraAiConnector"),
            provider.GetRequiredService<AuroraAiConnectorOptions>(),
            message => provider.GetRequiredService<ILogger<ChatAiProvidersModule>>().LogInformation("{Message}", message)));

        context.Services.AddTransient<AuroraAiProvider>();
        context.Services.AddTransient<GeminiAiProvider>();
        context.Services.AddTransient<TesseractAiProvider>();
        context.Services.AddTransient<CopilotAiProvider>();
        context.Services.AddTransient<GithubAiProvider>();
    }
}
