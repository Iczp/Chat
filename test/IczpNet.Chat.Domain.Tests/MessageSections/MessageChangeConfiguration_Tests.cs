using System.Collections.Generic;
using IczpNet.Chat.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace IczpNet.Chat;

public class MessageChangeConfiguration_Tests
{
    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData(null, false)]
    public void Domain_module_reads_the_change_sync_deployment_switch(string configured, bool expected)
    {
        var values = new Dictionary<string, string>();
        if (configured != null) values["MessageOptions:ChangeSyncEnabled"] = configured;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = new ServiceCollection();
        services.AddApplication<ChatDomainModule>(options => options.Services.ReplaceConfiguration(configuration));
        using var provider = services.BuildServiceProvider();
        Assert.Equal(expected, provider.GetRequiredService<IOptions<MessageOptions>>().Value.ChangeSyncEnabled);
    }
}
