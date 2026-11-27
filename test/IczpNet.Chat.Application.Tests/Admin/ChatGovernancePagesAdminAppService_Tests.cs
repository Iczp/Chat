using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.Messages;
using IczpNet.Chat.Admin.Messages.Dtos;
using IczpNet.Chat.Admin.Overview;
using IczpNet.Chat.Admin.SessionAnnouncements;
using IczpNet.Chat.Admin.SessionRequests;
using IczpNet.Chat.Admin.SessionRoles;
using IczpNet.Chat.Admin.Sessions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionRequests;
using IczpNet.Chat.SessionRoles;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionSections.Announcements;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

namespace IczpNet.Chat.Admin;

public class ChatGovernancePagesAdminAppService_Tests : ChatTestBase<ChatAdminApplicationTestModule>
{
    private readonly IOverviewAdminAppService _overviewAppService;
    private readonly ISessionAdminAppService _sessionAppService;
    private readonly ISessionRequestAdminAppService _sessionRequestAppService;
    private readonly ISessionRoleAdminAppService _sessionRoleAppService;
    private readonly ISessionAnnouncementAdminAppService _announcementAppService;
    private readonly IMessageAdminAppService _messageAppService;

    public ChatGovernancePagesAdminAppService_Tests()
    {
        _overviewAppService = GetRequiredService<IOverviewAdminAppService>();
        _sessionAppService = GetRequiredService<ISessionAdminAppService>();
        _sessionRequestAppService = GetRequiredService<ISessionRequestAdminAppService>();
        _sessionRoleAppService = GetRequiredService<ISessionRoleAdminAppService>();
        _announcementAppService = GetRequiredService<ISessionAnnouncementAdminAppService>();
        _messageAppService = GetRequiredService<IMessageAdminAppService>();
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
    public async Task Overview_session_request_role_message_and_announcement_admin_services_work()
    {
        var sessionId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var announcementId = Guid.NewGuid();
        var messageId = 770001L;

        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            await InsertAsync<ChatObject>(context, new Dictionary<string, object>
            {
                [nameof(ChatObject.Id)] = 3001L,
                [nameof(ChatObject.Name)] = "测试成员A",
                [nameof(ChatObject.Code)] = "gov_member_a",
                [nameof(ChatObject.ChatObjectTypeId)] = "Personal",
                [nameof(ChatObject.ObjectType)] = ChatObjectTypeEnums.Personal,
                [nameof(ChatObject.IsEnabled)] = true,
                [nameof(ChatObject.CreationTime)] = DateTime.UtcNow
            });

            await InsertAsync<ChatObject>(context, new Dictionary<string, object>
            {
                [nameof(ChatObject.Id)] = 3002L,
                [nameof(ChatObject.Name)] = "测试群B",
                [nameof(ChatObject.Code)] = "gov_room_b",
                [nameof(ChatObject.ChatObjectTypeId)] = "Room",
                [nameof(ChatObject.ObjectType)] = ChatObjectTypeEnums.Room,
                [nameof(ChatObject.IsEnabled)] = true,
                [nameof(ChatObject.CreationTime)] = DateTime.UtcNow
            });

            await InsertAsync<Session>(context, new Dictionary<string, object>
            {
                [nameof(Session.Id)] = sessionId,
                [nameof(Session.Title)] = "管理测试群",
                [nameof(Session.SessionKey)] = "room_test_001",
                [nameof(Session.Channel)] = Channels.RoomChannel,
                [nameof(Session.IsEnabled)] = true,
                [nameof(Session.CreationTime)] = DateTime.UtcNow
            });

            await InsertAsync<SessionRequest>(context, new Dictionary<string, object>
            {
                [nameof(SessionRequest.Id)] = requestId,
                [nameof(SessionRequest.OwnerId)] = 3001L,
                [nameof(SessionRequest.DestinationId)] = 3002L,
                [nameof(SessionRequest.RequestMessage)] = "请求加入群聊",
                [nameof(SessionRequest.IsHandled)] = false,
                [nameof(SessionRequest.IsEnabled)] = true,
                [nameof(SessionRequest.CreationTime)] = DateTime.UtcNow
            });

            await InsertAsync<SessionRole>(context, new Dictionary<string, object>
            {
                [nameof(SessionRole.Id)] = roleId,
                [nameof(SessionRole.SessionId)] = sessionId,
                [nameof(SessionRole.Name)] = "管理员",
                [nameof(SessionRole.IsDefault)] = true,
                [nameof(SessionRole.PermissionCount)] = 5,
                [nameof(SessionRole.CreationTime)] = DateTime.UtcNow
            });

            await InsertAsync<SessionAnnouncement>(context, new Dictionary<string, object>
            {
                [nameof(SessionAnnouncement.Id)] = announcementId,
                [nameof(SessionAnnouncement.SessionId)] = sessionId,
                [nameof(SessionAnnouncement.ChatObjectId)] = 3002L,
                [nameof(SessionAnnouncement.PublisherChatObjectId)] = 3001L,
                [nameof(SessionAnnouncement.Title)] = "群规公告",
                [nameof(SessionAnnouncement.Content)] = "请遵守群规",
                [nameof(SessionAnnouncement.Status)] = AnnouncementStatus.Published,
                [nameof(SessionAnnouncement.IsPinned)] = true,
                [nameof(SessionAnnouncement.IsApproved)] = true,
                [nameof(SessionAnnouncement.Version)] = 1,
                [nameof(SessionAnnouncement.CreationTime)] = DateTime.UtcNow
            });

            await InsertAsync<Message>(context, new Dictionary<string, object>
            {
                [nameof(Message.Id)] = messageId,
                [nameof(Message.SessionId)] = sessionId,
                [nameof(Message.SenderId)] = 3001L,
                [nameof(Message.Channel)] = Channels.RoomChannel,
                [nameof(Message.MessageType)] = MessageTypes.Text,
                [nameof(Message.ContentJson)] = "{\"text\":\"hello\"}",
                [nameof(Message.CreationTime)] = DateTime.UtcNow
            });
        });

        var overview = await _overviewAppService.GetSummaryAsync();
        Assert.True(overview.SessionCount >= 1);
        Assert.True(overview.MessageCount >= 1);

        var sessions = await _sessionAppService.GetListAsync(new SessionAdminGetListInput { Keyword = "管理测试群" });
        Assert.Single(sessions.Items);
        var disabledSession = await _sessionAppService.SetIsEnabledAsync(sessionId, new SessionAdminSetIsEnabledInput { IsEnabled = false });
        Assert.False(disabledSession.IsEnabled);

        var requests = await _sessionRequestAppService.GetListAsync(new SessionRequestAdminGetListInput { OwnerId = 3001L });
        Assert.Single(requests.Items);

        var roles = await _sessionRoleAppService.GetListAsync(new SessionRoleAdminGetListInput { SessionId = sessionId });
        Assert.Single(roles.Items);

        var announcements = await _announcementAppService.GetListAsync(new SessionAnnouncementAdminGetListInput { SessionId = sessionId });
        Assert.Single(announcements.Items);

        var messages = await _messageAppService.GetListAsync(new MessageAdminGetListInput { SessionId = sessionId });
        Assert.Single(messages.Items);
        var msgDetail = await _messageAppService.GetAsync(messageId);
        Assert.Contains("hello", msgDetail.ContentJson);
    }

    [Fact]
    public async Task Conventional_controllers_register_governance_page_routes()
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

        Assert.Contains(allEndpoints, e => e.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/overview/summary");
        Assert.Contains(allEndpoints, e => e.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/session");
        Assert.Contains(allEndpoints, e => e.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/session-request");
        Assert.Contains(allEndpoints, e => e.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/session-role");
        Assert.Contains(allEndpoints, e => e.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/session-announcement");
        Assert.Contains(allEndpoints, e => e.RoutePattern.RawText?.TrimStart('/') == "api/chat/admin/message");
    }
}
