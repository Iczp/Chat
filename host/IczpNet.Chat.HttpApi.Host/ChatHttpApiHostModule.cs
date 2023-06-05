using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.MultiTenancy;
using IczpNet.Pusher;
using IczpNet.Pusher.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Volo.Abp;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.MultiTenancy;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
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
public class ChatHttpApiHostModule : AbpModule
{

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

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

        });
        Configure<AbpSystemTextJsonSerializerOptions>(options =>
        {
            options.JsonSerializerOptions.MaxDepth = 16;
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            //options.JsonSerializerOptions.WriteIndented = true;
        });

        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options
                .ConventionalControllers
                .Create(typeof(ChatApplicationModule).Assembly);
            //options
            //    .ConventionalControllers
            //    .Create(typeof(ChatManagementApplicationModule).Assembly);
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseSqlServer();
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
                //options.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.ActionDescriptor.RouteValues["action"]}_{apiDesc.HttpMethod}");
                options.TagActionsBy(x => new[] { x.ActionDescriptor.RouteValues["controller"] });

                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Chat API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.SwaggerDoc(ChatRemoteServiceConsts.ModuleName, new OpenApiInfo { Title = "Chat", Version = ChatRemoteServiceConsts.ModuleName });
                //options.SwaggerDoc(ChatManagementRemoteServiceConsts.ModuleName, new OpenApiInfo { Title = "ChatManagement", Version = ChatManagementRemoteServiceConsts.ModuleName });
                //System.Diagnostics.Debugger.Launch();

                //options.DocInclusionPredicate((docName, apiDesc) =>
                //{
                //    //return true;
                //    if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo))
                //    {
                //        return false;
                //    }
                //    var versions = methodInfo.ReflectedType.GetCustomAttributes(true).OfType<ApiExplorerSettingsAttribute>().Select(x => x.GroupName);
                //    System.Diagnostics.Trace.WriteLine($"swagger-{docName}---{apiDesc.ActionDescriptor.SenderName} -- {string.Join(";", versions)}");
                //    if (docName.ToLower() == "v1" && !versions.Any())
                //    {
                //        return true;
                //    }
                //    return versions.Any(x => x.ToString() == docName);
                //});
                options.CustomSchemaIds(type => type.FullName);
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
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
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
                options.Audience = "Chat";
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

        app.UseStaticAutoMapper();

        //app.UsePusherSubscriber();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
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

            var configuration = context.GetConfiguration();
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            options.OAuthScopes("Chat");
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
