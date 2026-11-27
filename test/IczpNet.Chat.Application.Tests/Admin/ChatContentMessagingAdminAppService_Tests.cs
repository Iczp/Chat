using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.ChatObjects;
using IczpNet.Chat.Admin.ChatObjects.Dtos;
using IczpNet.Chat.Admin.MessageTemplates;
using IczpNet.Chat.Admin.MessageTemplates.Dtos;
using IczpNet.Chat.Admin.SessionUnits;
using IczpNet.Chat.Admin.SessionUnits.Dtos;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageTemplates;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

namespace IczpNet.Chat.Admin;

public class ChatContentMessagingAdminAppService_Tests : ChatTestBase<ChatAdminApplicationTestModule>
{
    private const string ValidTemplateJson =
        """{"schemaVersion":1,"name":"订单通知","fields":[{"name":"orderNo","type":"string","required":true}],"layout":{"type":"card"}}""";

    private readonly IChatObjectAdminAppService _chatObjectAppService;
    private readonly IMessageTemplateAdminAppService _templateAppService;
    private readonly ISessionUnitAdminAppService _sessionUnitAppService;

    public ChatContentMessagingAdminAppService_Tests()
    {
        _chatObjectAppService = GetRequiredService<IChatObjectAdminAppService>();
        _templateAppService = GetRequiredService<IMessageTemplateAdminAppService>();
        _sessionUnitAppService = GetRequiredService<ISessionUnitAdminAppService>();
    }

    private static async Task InsertAsync<T>(ChatDbContext context, Dictionary<string, object> values)
    {
        var entity = context.Model.FindEntityType(typeof(T))!;
        var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
        var columns = new List<string>();
        var parameters = new List<object>();
        foreach (var property in entity.GetProperties())
        {
            if (property.GetComputedColumnSql() != null) continue;
            if (!values.TryGetValue(property.Name, out var value))
            {
                if (property.IsNullable) continue;
                var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                value = type == typeof(string) ? "" : type == typeof(Guid) ? Guid.NewGuid() :
                    type == typeof(DateTime) ? DateTime.UtcNow : Activator.CreateInstance(type);
            }
            var converter = property.GetTypeMapping().Converter;
            if (converter != null) value = converter.ConvertToProvider(value);
            columns.Add("\"" + property.GetColumnName(store) + "\"");
            parameters.Add(value ?? DBNull.Value);
        }

        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO \"" + entity.GetTableName() + "\" (" +
            string.Join(",", columns) + ") VALUES (" +
            string.Join(",", Enumerable.Range(0, parameters.Count).Select(i => "{" + i + "}")) + ")",
            parameters.ToArray());
    }

    [Fact]
    public async Task ChatObject_create_and_delete_with_static_guard_work()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            var typeRepo = GetRequiredService<IRepository<ChatObjectType, string>>();
            if (await typeRepo.FindAsync("Personal") == null)
            {
                await typeRepo.InsertAsync(
                    new ChatObjectType("Personal") { Name = "个人", ObjectType = ChatObjectTypeEnums.Personal },
                    autoSave: true);
            }
        });

        var created = await _chatObjectAppService.CreateAsync(new ChatObjectAdminCreateInput
        {
            Name = "后台新建对象A",
            Code = "admin_created_a",
            ChatObjectTypeId = "Personal",
            ObjectType = ChatObjectTypeEnums.Personal,
            Description = "测试创建",
            IsEnabled = true,
            IsPublic = true
        });
        Assert.True(created.Id > 0);
        Assert.Equal("后台新建对象A", created.Name);

        await _chatObjectAppService.DeleteAsync(created.Id);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            _chatObjectAppService.GetAsync(created.Id));

        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            await InsertAsync<ChatObject>(context, new Dictionary<string, object>
            {
                [nameof(ChatObject.Id)] = 9555L,
                [nameof(ChatObject.Name)] = "内置禁止删除",
                [nameof(ChatObject.Code)] = "static_no_delete",
                [nameof(ChatObject.ChatObjectTypeId)] = "Personal",
                [nameof(ChatObject.ObjectType)] = ChatObjectTypeEnums.Personal,
                [nameof(ChatObject.IsStatic)] = true,
                [nameof(ChatObject.IsEnabled)] = true,
                [nameof(ChatObject.CreationTime)] = DateTime.UtcNow
            });
        });

        await Assert.ThrowsAsync<BusinessException>(() =>
            _chatObjectAppService.DeleteAsync(9555L));
    }

    [Fact]
    public async Task MessageTemplate_admin_lifecycle_and_preview_work()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            await InsertAsync<ChatObject>(context, new Dictionary<string, object>
            {
                [nameof(ChatObject.Id)] = 1001L,
                [nameof(ChatObject.Name)] = "模板测试主体",
                [nameof(ChatObject.Code)] = "tpl_owner_1001",
                [nameof(ChatObject.ChatObjectTypeId)] = "Official",
                [nameof(ChatObject.ObjectType)] = ChatObjectTypeEnums.Official,
                [nameof(ChatObject.IsEnabled)] = true,
                [nameof(ChatObject.CreationTime)] = DateTime.UtcNow
            });
        });

        var created = await _templateAppService.CreateAsync(new MessageTemplateAdminCreateInput
        {
            OwnerId = 1001L,
            Name = "测试模板",
            Description = "订单通知模板",
            Visibility = MessageTemplateVisibility.Private,
            Definition = ValidTemplateJson
        });
        Assert.Equal("测试模板", created.Name);
        Assert.Null(created.PublishedVersion);

        var list = await _templateAppService.GetListAsync(new MessageTemplateAdminGetListInput
        {
            OwnerId = 1001L,
            Keyword = "测试模板"
        });
        Assert.Equal(1, list.TotalCount);

        var updated = await _templateAppService.UpdateAsync(created.Id, new MessageTemplateAdminUpdateInput
        {
            Name = "测试模板-v2",
            Description = "更新说明",
            Visibility = MessageTemplateVisibility.Private,
            Definition = ValidTemplateJson,
            ConcurrencyStamp = created.ConcurrencyStamp
        });
        Assert.Equal("测试模板-v2", updated.Name);

        var published = await _templateAppService.PublishAsync(created.Id, new MessageTemplateAdminPublishInput
        {
            ConcurrencyStamp = updated.ConcurrencyStamp
        });
        Assert.Equal(1, published.Version);

        var revisions = await _templateAppService.GetRevisionsAsync(created.Id, new PagedResultRequestDto());
        Assert.Equal(1, revisions.TotalCount);

        var revDetail = await _templateAppService.GetRevisionAsync(created.Id, 1);
        Assert.Equal("订单通知", System.Text.Json.JsonDocument.Parse(revDetail.DefinitionJson).RootElement.GetProperty("name").GetString());

        var toggled = await _templateAppService.SetEnabledAsync(created.Id, new MessageTemplateAdminSetEnabledInput
        {
            IsEnabled = false,
            ConcurrencyStamp = published.ConcurrencyStamp
        });
        Assert.False(toggled.IsEnabled);

        var preview = await _templateAppService.PreviewAsync(new MessageTemplateAdminPreviewInput
        {
            TemplateId = created.Id,
            Definition = ValidTemplateJson,
            Data = new Dictionary<string, object> { ["orderNo"] = "ORD-1001" }
        });
        Assert.True(preview.IsValid);
    }

    [Fact]
    public async Task SessionUnit_admin_list_and_find_id_work()
    {
        var sessionUnitId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            await InsertAsync<Session>(context, new Dictionary<string, object>
            {
                [nameof(Session.Id)] = sessionId,
                [nameof(Session.IsEnabled)] = true,
                [nameof(Session.CreationTime)] = DateTime.UtcNow
            });

            await InsertAsync<SessionUnit>(context, new Dictionary<string, object>
            {
                [nameof(SessionUnit.Id)] = sessionUnitId,
                [nameof(SessionUnit.OwnerId)] = 2001L,
                [nameof(SessionUnit.DestinationId)] = 2002L,
                [nameof(SessionUnit.SessionId)] = sessionId,
                [nameof(SessionUnit.DestinationObjectType)] = ChatObjectTypeEnums.Personal,
                [nameof(SessionUnit.CreationTime)] = DateTime.UtcNow
            });

            await InsertAsync<SessionUnitSetting>(context, new Dictionary<string, object>
            {
                [nameof(SessionUnitSetting.SessionUnitId)] = sessionUnitId,
                [nameof(SessionUnitSetting.SessionId)] = sessionId,
                [nameof(SessionUnitSetting.IsKilled)] = false,
                [nameof(SessionUnitSetting.IsEnabled)] = true,
                [nameof(SessionUnitSetting.IsInputEnabled)] = true,
                [nameof(SessionUnitSetting.CreationTime)] = DateTime.UtcNow
            });
        });

        var list = await _sessionUnitAppService.GetListAsync(new SessionUnitAdminGetListInput
        {
            OwnerId = 2001L,
            IsKilled = false
        });
        Assert.Equal(1, list.TotalCount);
        Assert.Equal(sessionUnitId, list.Items[0].Id);

        var foundId = await _sessionUnitAppService.FindIdAsync(2001L, 2002L);
        Assert.Equal(sessionUnitId, foundId);
    }

    [Fact]
    public async Task Conventional_controllers_register_content_and_messaging_admin_routes()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
        await builder.AddApplicationAsync<ChatAdminMvcTestModule>();
        await using var app = builder.Build();
        await app.InitializeApplicationAsync();
        app.MapControllers();

        var allEndpoints = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .ToList();

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(IczpNet.Chat.Admin.MessageTemplates.MessageTemplateAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/message-template");

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(IczpNet.Chat.Admin.ArticleMessageSchedules.ArticleMessageScheduleAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/article-message-schedule");

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(IczpNet.Chat.Admin.SessionUnits.SessionUnitAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/session-unit");

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(IczpNet.Chat.Admin.SessionUnits.SessionUnitAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/session-unit/find-id" &&
            endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("POST") == true);

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(IczpNet.Chat.Admin.MessageTemplates.MessageTemplateAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/message-template/{id}/revision");

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(IczpNet.Chat.Admin.Messages.MessageAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/message/history");

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(IczpNet.Chat.Admin.Messages.MessageAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/message/send-article-content-message/{sessionUnitId}");

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(IczpNet.Chat.Admin.Messages.MessageAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/message/send-template-content-message/{sessionUnitId}");

        Assert.Contains(allEndpoints, endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(IczpNet.Chat.Admin.Messages.MessageAppService) &&
            endpoint.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/message/rollback/{messageId}");
    }
}
