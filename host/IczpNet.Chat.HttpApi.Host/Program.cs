using System;
using System.Net;
using System.Threading.Tasks;
using IczpNet.AbpCommons;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace IczpNet.Chat;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddAppSettingsSecretsJson().UseAutofac();
            Log.Logger = new LoggerConfiguration()
                .WithDailyLogFiles(builder.Configuration)
#if DEBUG
                .WriteTo.Async(sinks => sinks.Udp("127.0.0.1", 8899, System.Net.Sockets.AddressFamily.InterNetwork))
#endif
                .CreateLogger();
            builder.Host.UseSerilog();
            Log.Information("Starting IczpNet.Chat.HttpApi.Host. HostName:{HostName}", Dns.GetHostName());
            if(builder.Configuration.GetValue("Device:UseAppDevice",false))
                await builder.AddApplicationAsync<ChatAppDeviceHttpApiHostModule>();
            else
                await builder.AddApplicationAsync<ChatHttpApiHostModule>();
            var app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
