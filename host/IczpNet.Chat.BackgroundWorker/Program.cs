using Serilog;
using IczpNet.AbpCommons;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore;

namespace IczpNet.Chat;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddAppSettingsSecretsJson().UseAutofac();
            Log.Logger = new LoggerConfiguration().WithDailyLogFiles(builder.Configuration).CreateLogger();
            builder.Host.UseSerilog();
            Log.Information("Starting IczpNet.Chat.BackgroundWorker");
            if (builder.Configuration.GetValue("Device:UseAppDevice", false))
                await builder.AddApplicationAsync<ChatAppDeviceBackgroundWorkerModule>();
            else
                await builder.AddApplicationAsync<ChatBackgroundWorkerModule>();
            var app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "IczpNet.Chat.BackgroundWorker terminated unexpectedly");
            throw;
        }
        finally { await Log.CloseAndFlushAsync(); }
    }
}
