using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace IczpNet.Chat;

//public class Program
//{
//    public async static Task<int> Main(string[] args)
//    {
//        Log.Logger = new LoggerConfiguration()
//#if DEBUG
//            .MinimumLevel.Debug()
//#else
//            .MinimumLevel.Information()
//#endif
//            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
//            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
//            .Enrich.FromLogContext()
//            .WriteTo.Async(c => c.File("Logs/logs.txt"))
//            .WriteTo.Async(c => c.Console())
//            .CreateLogger();

//        try
//        {
//            Log.Information("Starting web host.");
//            var builder = WebApplication.CreateBuilder(args);
//            builder.Host.AddAppSettingsSecretsJson()
//                .UseAutofac()
//                .UseSerilog();
//            await builder.AddApplicationAsync<ChatHttpApiHostModule>();
//            var app = builder.Build();
//            await app.InitializeApplicationAsync();
//            await app.RunAsync();
//            return 0;
//        }
//        catch (Exception ex)
//        {
//            Log.Fatal(ex, "Host terminated unexpectedly!");
//            return 1;
//        }
//        finally
//        {
//            Log.CloseAndFlush();
//        }


//    }
//}


public class Program
{
    public async static Task<int> Main(string[] args)
    {
        //日志的输出模板
        string LogFilePath(LogEventLevel LogEvent) => $@"Logs\{DateTime.Now:yyyy-MM-dd}\{LogEvent}\log.log";
        string SerilogOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] [{SourceContext}] [{Method}] {Message}{NewLine}{Exception}{NewLine}" + new string('-', 50) + "{NewLine}";
        var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] [{SourceContext}] [{Method}] {Message}{NewLine}{Exception}{NewLine}";


        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            //.MinimumLevel.Override("Volo.Abp.Auditing", LogEventLevel.Information)
            //.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            //.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            //.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            //.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
            //.Enrich.FromLogContext()
            //.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            //.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)

            .Enrich.FromLogContext()
            .WriteTo.Logger(fileLogger => fileLogger.WriteTo.Async(c => c.File($"Logs/log_all_.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)))
            //.WriteTo.Async(c => c.File("Logs/logs.txt"))
#if DEBUG
            .WriteTo.Async(c => c.Console(outputTemplate: outputTemplate))
            .WriteTo.Async(c => c.Udp("127.0.0.1", 8899, System.Net.Sockets.AddressFamily.InterNetwork, outputTemplate: outputTemplate))
#else
                //.WriteTo.Async(c => c.Udp("host.docker.internal", 19999, System.Net.Sockets.AddressFamily.InterNetwork))
#endif
            .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Information).WriteTo.File(LogFilePath(LogEventLevel.Information), rollingInterval: RollingInterval.Day, outputTemplate: SerilogOutputTemplate))
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Warning).WriteTo.File(LogFilePath(LogEventLevel.Warning), rollingInterval: RollingInterval.Day, outputTemplate: SerilogOutputTemplate))
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Error).WriteTo.File(LogFilePath(LogEventLevel.Error), rollingInterval: RollingInterval.Day, outputTemplate: SerilogOutputTemplate))
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Fatal).WriteTo.File(LogFilePath(LogEventLevel.Fatal), rollingInterval: RollingInterval.Day, outputTemplate: SerilogOutputTemplate))
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Debug).WriteTo.File(LogFilePath(LogEventLevel.Debug), rollingInterval: RollingInterval.Day, outputTemplate: SerilogOutputTemplate))
            .CreateLogger();

        try
        {
            Log.Information($"Starting Iczp.Net.HttpApi.Host. HostName:{Dns.GetHostName()}");
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog();
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
            Log.CloseAndFlush();
        }
    }
}