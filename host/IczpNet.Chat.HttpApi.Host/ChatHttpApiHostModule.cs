using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.MultiTenancy;
using IczpNet.Pusher.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.MultiTenancy;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Json;
using Volo.Abp.Json.SystemTextJson;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.Swashbuckle;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Volo.Abp.VirtualFileSystem;

namespace IczpNet.Chat;
[DependsOn(
typeof(ChatApplicationModule),
    typeof(ChatEntityFrameworkCoreModule),
    typeof(ChatHttpApiModule),
    typeof(AbpAspNetCoreMvcUiMultiTenancyModule),
    typeof(AbpAutofacModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule)
    )]
//[DependsOn(typeof(ChatManagementApplicationModule))]
[DependsOn(typeof(AbpBackgroundJobsModule))]
[DependsOn(typeof(AbpBlobStoringFileSystemModule))]
//[DependsOn(typeof(AbpEventBusRebusModule))]
public class ChatHttpApiHostModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        base.PreConfigureServices(context);

        //PreConfigure<AbpRebusEventBusOptions>(options =>
        //{
        //    options.InputQueueName = "IczpNet.Chat:eventbus";
        //});
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        //SameSiteCookiePolicy
        context.Services.AddSameSiteCookiePolicy();

        if (hostingEnvironment.IsDevelopment())
        {
            //----------alert------------

            context.Services.AddAlwaysAllowAuthorization();
        }

        Configure<AbpBackgroundJobWorkerOptions>(options =>
        {
            options.DefaultTimeout = 864000; //10 days (as seconds)
        });

        Configure<PusherOptions>(options =>
        {
            options.Redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);
        });

        Configure<AbpExceptionHandlingOptions>(options =>
        {
            options.SendExceptionsDetailsToClients = true;
            options.SendStackTraceToClients = true;
        });

        Configure<AbpJsonOptions>(options =>
        {
            options.OutputDateTimeFormat = "yyyy-MM-dd hh:mm:ss";
            options.InputDateTimeFormats = new List<string>() {
            "yyyy-MM-dd hh:mm:ss","yyyy-MM-dd"

            };
        });
        Configure<AbpSystemTextJsonSerializerOptions>(options =>
        {
            options.JsonSerializerOptions.MaxDepth = 16;
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            //options.JsonSerializerOptions.WriteIndented = true;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options
                .ConventionalControllers
                .Create(typeof(ChatApplicationModule).Assembly, conf =>
                {
                    conf.RootPath = ChatRemoteServiceConsts.RemoteServiceName.ToLower();
                });
            //options
            //    .ConventionalControllers
            //    .CreateAsync(typeof(ChatManagementApplicationModule).Assembly);
        });

        Configure<AbpBlobStoringOptions>(options =>
        {
            options.Containers.ConfigureDefault(container =>
            {
                container.UseFileSystem(filesystem =>
                {
                    filesystem.BasePath = configuration["Blob:FileSystem:BasePath"];
                });
            });
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseSqlServer(x =>
            {
                x.CommandTimeout(60000);
            });
        });

        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = MultiTenancyConsts.IsEnabled;
        });

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<ChatDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}IczpNet.Chat.Domain.Shared", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<ChatDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}IczpNet.Chat.Domain", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<ChatApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}IczpNet.Chat.Application.Contracts", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<ChatApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}IczpNet.Chat.Application", Path.DirectorySeparatorChar)));

                //options.FileSets.ReplaceEmbeddedByPhysical<ChatManagementApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}IczpNet.Chat.ManagementApplication.Contracts", Path.DirectorySeparatorChar)));
                //options.FileSets.ReplaceEmbeddedByPhysical<ChatManagementApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}IczpNet.Chat.ManagementApplication", Path.DirectorySeparatorChar)));
            });
        }

        context.Services.AddAbpSwaggerGenWithOAuth(
            configuration["AuthServer:Authority"],
            new Dictionary<string, string>
            {
                {"Chat", "Chat API"}
            },
            options =>
            {
                options.UseInlineDefinitionsForEnums();
                options.OrderActionsBy((x) => $"{x.ActionDescriptor.RouteValues["controller"]}_{x.ActionDescriptor.RouteValues["action"]}_{x.HttpMethod}");
                //options.OrderActionsBy((x) => $"{x.GroupName}_{x.ActionDescriptor.RouteValues["controller"]}_{x.HttpMethod}");
                options.TagActionsBy(x => new[] { x.ActionDescriptor.RouteValues["controller"] });

                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Chat API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.SwaggerDoc(ChatRemoteServiceConsts.ModuleName, new OpenApiInfo { Title = "Chat", Version = ChatRemoteServiceConsts.ModuleName });
                //options.SchemaFilter<EnumSchemaFilter>();

                //options.SwaggerDoc(ChatManagementRemoteServiceConsts.ModuleName, new OpenApiInfo { Title = "ChatManagement", Version = ChatManagementRemoteServiceConsts.ModuleName });
                //System.Diagnostics.Debugger.Launch();

                //options.DocInclusionPredicate((docName, x) =>
                //{
                //    //return true;
                //    if (!x.TryGetMethodInfo(out MethodInfo methodInfo))
                //    {
                //        return false;
                //    }
                //    var versions = methodInfo.ReflectedType.GetCustomAttributes(true).OfType<ApiExplorerSettingsAttribute>().Select(x => x.GroupName);
                //    System.Diagnostics.Trace.WriteLine($"swagger-{docName}---{x.ActionDescriptor.DisplayName} -- {string.Join(";", versions)}");
                //    if (docName.ToLower() == "v1" && !versions.Any())
                //    {
                //        return true;
                //    }
                //    return versions.Any(x => x.ToString() == docName);
                //});
                options.CustomSchemaIds(type => type.FullName);
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                var includeModuleXmlComments = (string[] nameSpaces) =>
                {
                    var xmls = new[] { ".Application.xml", ".Application.Contracts.xml", ".Domain.xml", ".Domain.Shared.xml", ".HttpApi.xml", };
                    foreach (var nameSpace in nameSpaces)
                    {
                        foreach (var xml in xmls)
                        {
                            options.IncludeXmlComments(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{nameSpace}{xml}"), true);
                        }
                    }
                };
                includeModuleXmlComments(new[] { $"IczpNet.{ChatRemoteServiceConsts.RemoteServiceName}", });
            });


        Configure<AbpLocalizationOptions>(options =>
        {
            options.Languages.Add(new LanguageInfo("zh-Hans", "zh-Hans", "简体中文"));
            options.Languages.Add(new LanguageInfo("en", "en", "English"));

            //options.Languages.Add(new LanguageInfo("ar", "ar", "العربية"));
            //options.Languages.Add(new LanguageInfo("cs", "cs", "Čeština"));

            //options.Languages.Add(new LanguageInfo("en-GB", "en-GB", "English (UK)"));
            //options.Languages.Add(new LanguageInfo("fi", "fi", "Finnish"));
            //options.Languages.Add(new LanguageInfo("fr", "fr", "Français"));
            //options.Languages.Add(new LanguageInfo("hi", "hi", "Hindi", "in"));
            //options.Languages.Add(new LanguageInfo("is", "is", "Icelandic", "is"));
            //options.Languages.Add(new LanguageInfo("it", "it", "Italiano", "it"));
            //options.Languages.Add(new LanguageInfo("hu", "hu", "Magyar"));
            //options.Languages.Add(new LanguageInfo("pt-BR", "pt-BR", "Português"));
            //options.Languages.Add(new LanguageInfo("ro-RO", "ro-RO", "Română"));
            //options.Languages.Add(new LanguageInfo("ru", "ru", "Русский"));
            //options.Languages.Add(new LanguageInfo("sk", "sk", "Slovak"));
            //options.Languages.Add(new LanguageInfo("tr", "tr", "Türkçe"));

            //options.Languages.Add(new LanguageInfo("zh-Hant", "zh-Hant", "繁體中文"));
            //options.Languages.Add(new LanguageInfo("de-DE", "de-DE", "Deutsch"));
            //options.Languages.Add(new LanguageInfo("es", "es", "Español"));
            //options.Languages.Add(new LanguageInfo("el", "el", "Ελληνικά"));
        });

        context.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddCookie("Cookies", options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromDays(365);
                // add an instance of the patched manager to the options:
                options.CookieManager = new ChunkingCookieManager();
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.None;
                //Correlation failed in net.core / asp.net identity / openid connect
                //https://stackoverflow.com/questions/50262561/correlation-failed-in-net-core-asp-net-identity-openid-connect
                //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
                options.Audience = "Chat";

                // We have to hook the OnMessageReceived event in order to
                // allow the JWT authentication handler to read the access
                // token from the query string when a WebSocket or 
                // Server-Sent Events request comes in.

                // Sending the access token in the query string is required when using WebSockets or ServerSentEvents
                // due to a limitation in Browser APIs. We restrict it to only calls to the
                // SignalR hub in this code.
                // See https://docs.microsoft.com/aspnet/core/signalr/security#access-token-logging
                // for more information about security considerations when using
                // the query string to transmit the access token.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/signalr-hubs/")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        Configure<AbpDistributedCacheOptions>(options =>
        {
            options.KeyPrefix = "Chat:";
        });

        var dataProtectionBuilder = context.Services.AddDataProtection().SetApplicationName("Chat");
        if (!hostingEnvironment.IsDevelopment())
        {
            var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);
            dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, "Chat-Protection-Keys");
        }

        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.RemovePostFix("/"))
                            .ToArray()
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });


    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        app.ApplicationServices.UseStaticAutoMapper();

        //app.UsePusherSubscriber();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
        }

        //app.UseHttpsRedirection();
        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseCookiePolicy();
        app.UseAuthentication();
        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }
        app.UseAbpRequestLocalization();
        app.UseAuthorization();
        app.UseSwagger(x => x.RouteTemplate = "swagger/{documentName}/swagger.json");
        //app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            //options.SwaggerEndpoint(string.Format("/swagger/{0}/swagger.json", ChatRemoteServiceConsts.ModuleName), "Chat Api");
            options.SwaggerEndpoint(string.Format("/swagger/{0}/swagger.json", "v1"), "Support APP API");
            //options.SwaggerEndpoint(string.Format("/swagger/{0}/swagger.json", ChatManagementRemoteServiceConsts.ModuleName), "Chat Management Api");
            options.EnableDeepLinking();
            options.EnableFilter();
            options.EnableTryItOutByDefault();
            options.EnablePersistAuthorization();
            var configuration = context.GetConfiguration();
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            options.OAuthScopes("Chat");
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
