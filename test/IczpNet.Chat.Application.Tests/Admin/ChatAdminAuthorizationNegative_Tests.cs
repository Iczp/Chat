using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.SessionAnnouncements;
using IczpNet.Chat.Admin.SessionRequests;
using IczpNet.Chat.Admin.Sessions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageTemplates;
using IczpNet.Chat.SessionRequests;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionSections.Announcements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

namespace IczpNet.Chat.Admin;

public class ChatAdminAuthorizationNegative_Tests : ChatTestBase<ChatAdminApplicationTestModule>
{
    private const string ValidTemplateJson =
        """{"schemaVersion":1,"name":"范围测试","fields":[],"layout":{"type":"card"}}""";

    private readonly IMessageTemplateAdminManager _templateAdminManager;
    private readonly IArticleScheduleAdminManager _scheduleAdminManager;
    private readonly IChatObjectManager _chatObjectManager;
    private readonly ISessionAdminAppService _sessionAppService;
    private readonly ISessionRequestAdminAppService _sessionRequestAppService;
    private readonly ISessionAnnouncementAdminAppService _announcementAppService;

    public ChatAdminAuthorizationNegative_Tests()
    {
        _templateAdminManager = GetRequiredService<IMessageTemplateAdminManager>();
        _scheduleAdminManager = GetRequiredService<IArticleScheduleAdminManager>();
        _chatObjectManager = GetRequiredService<IChatObjectManager>();
        _sessionAppService = GetRequiredService<ISessionAdminAppService>();
        _sessionRequestAppService = GetRequiredService<ISessionRequestAdminAppService>();
        _announcementAppService = GetRequiredService<ISessionAnnouncementAdminAppService>();
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
    public async Task MessageTemplate_scope_enforces_tenant_and_owner_isolation()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var ownerA = 8101L;
        var ownerB = 8102L;

        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            foreach (var (id, code) in new[] { (ownerA, "scope_owner_a"), (ownerB, "scope_owner_b") })
            {
                await InsertAsync<ChatObject>(context, new Dictionary<string, object>
                {
                    [nameof(ChatObject.Id)] = id,
                    [nameof(ChatObject.Name)] = code,
                    [nameof(ChatObject.Code)] = code,
                    [nameof(ChatObject.ChatObjectTypeId)] = "Official",
                    [nameof(ChatObject.ObjectType)] = ChatObjectTypeEnums.Official,
                    [nameof(ChatObject.IsEnabled)] = true,
                    [nameof(ChatObject.CreationTime)] = DateTime.UtcNow
                });
            }
        });

        var scopeOwnerA = new MessageTemplateAdminScope
        {
            TenantId = tenantA,
            HasManageAll = false,
            CanUse = true,
            AllowedOwnerIds = [ownerA]
        };

        var scopeOwnerB = new MessageTemplateAdminScope
        {
            TenantId = tenantA,
            HasManageAll = false,
            CanUse = true,
            AllowedOwnerIds = [ownerB]
        };

        var scopeManageAllTenantB = new MessageTemplateAdminScope
        {
            TenantId = tenantB,
            HasManageAll = true,
            CanUse = true
        };

        var currentTenant = GetRequiredService<Volo.Abp.MultiTenancy.ICurrentTenant>();

        // Owner A cannot create for Owner B
        await WithUnitOfWorkAsync(async () =>
        {
            using (currentTenant.Change(tenantA))
            {
                await Assert.ThrowsAsync<AbpAuthorizationException>(() =>
                    _templateAdminManager.CreateAdminAsync(
                        ownerB, "越权模板", null, MessageTemplateVisibility.Private, ValidTemplateJson, tenantA, scopeOwnerA));

                var tplA = await _templateAdminManager.CreateAdminAsync(
                    ownerA, "A私有模板", null, MessageTemplateVisibility.Private, ValidTemplateJson, tenantA, scopeOwnerA);

                // Owner B cannot see or read Owner A's private template
                var listForB = await _templateAdminManager.GetAdminListAsync(null, "A私有模板", "owned", null, 0, 10, scopeOwnerB);
                Assert.Equal(0, listForB.TotalCount);
                await Assert.ThrowsAsync<AbpAuthorizationException>(() =>
                    _templateAdminManager.GetAdminDetailAsync(tplA.Id, scopeOwnerB));

                // ManageAll in Tenant B cannot access Tenant A's template
                using (currentTenant.Change(tenantB))
                {
                    await Assert.ThrowsAsync<UserFriendlyException>(() =>
                        _templateAdminManager.GetAdminDetailAsync(tplA.Id, scopeManageAllTenantB));
                }
            }
        });
    }

    [Fact]
    public async Task ArticleSchedule_scope_enforces_tenant_and_owner_isolation()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var taskA = Guid.NewGuid();
        var taskB = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            await InsertAsync<ScheduledArticleMessage>(context, new Dictionary<string, object>
            {
                [nameof(ScheduledArticleMessage.Id)] = taskA,
                [nameof(ScheduledArticleMessage.SenderSessionUnitId)] = Guid.NewGuid(),
                [nameof(ScheduledArticleMessage.OwnerId)] = 8201L,
                [nameof(ScheduledArticleMessage.RequestedBy)] = Guid.NewGuid(),
                [nameof(ScheduledArticleMessage.RequestedByTenantId)] = tenantA,
                [nameof(ScheduledArticleMessage.InputJson)] = "{}",
                [nameof(ScheduledArticleMessage.ScheduledAt)] = DateTime.UtcNow.AddHours(1),
                [nameof(ScheduledArticleMessage.Status)] = ArticleScheduleStatus.Pending,
                [nameof(ScheduledArticleMessage.Version)] = 1
            });

            await InsertAsync<ScheduledArticleMessage>(context, new Dictionary<string, object>
            {
                [nameof(ScheduledArticleMessage.Id)] = taskB,
                [nameof(ScheduledArticleMessage.SenderSessionUnitId)] = Guid.NewGuid(),
                [nameof(ScheduledArticleMessage.OwnerId)] = 8202L,
                [nameof(ScheduledArticleMessage.RequestedBy)] = Guid.NewGuid(),
                [nameof(ScheduledArticleMessage.RequestedByTenantId)] = tenantB,
                [nameof(ScheduledArticleMessage.InputJson)] = "{}",
                [nameof(ScheduledArticleMessage.ScheduledAt)] = DateTime.UtcNow.AddHours(1),
                [nameof(ScheduledArticleMessage.Status)] = ArticleScheduleStatus.Pending,
                [nameof(ScheduledArticleMessage.Version)] = 1
            });
        });

        var scopeA = new ArticleScheduleAdminScope
        {
            TenantId = tenantA,
            HasManageAll = false,
            AllowedOwnerIds = [8201L]
        };

        await WithUnitOfWorkAsync(async () =>
        {
            var result = await _scheduleAdminManager.GetAdminListAsync(null, null, null, 0, 20, scopeA);
            Assert.Single(result.Items);
            Assert.Equal(taskA, result.Items[0].Id);
        });
    }

    [Fact]
    public async Task ChatObject_creation_rejects_room_waiter_mismatch_and_update_preserves_fields()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var typeRepo = GetRequiredService<IRepository<ChatObjectType, string>>();
            foreach (var (id, objType) in new[]
            {
                ("Personal", ChatObjectTypeEnums.Personal),
                ("Room", ChatObjectTypeEnums.Room),
                ("ShopWaiter", ChatObjectTypeEnums.ShopWaiter)
            })
            {
                if (await typeRepo.FindAsync(id) == null)
                {
                    await typeRepo.InsertAsync(new ChatObjectType(id) { Name = id, ObjectType = objType }, autoSave: true);
                }
            }
        });

        await WithUnitOfWorkAsync(async () =>
        {
            await Assert.ThrowsAsync<UserFriendlyException>(() => _chatObjectManager.CreateAdminAsync(new ChatObjectAdminCreateArgs
            {
                Name = "类型不匹配",
                ChatObjectTypeId = "Personal",
                ObjectType = ChatObjectTypeEnums.Robot
            }));

            await Assert.ThrowsAsync<UserFriendlyException>(() => _chatObjectManager.CreateAdminAsync(new ChatObjectAdminCreateArgs
            {
                Name = "禁止通用建群",
                ChatObjectTypeId = "Room",
                ObjectType = ChatObjectTypeEnums.Room
            }));

            await Assert.ThrowsAsync<UserFriendlyException>(() => _chatObjectManager.CreateAdminAsync(new ChatObjectAdminCreateArgs
            {
                Name = "禁止通用建店员",
                ChatObjectTypeId = "ShopWaiter",
                ObjectType = ChatObjectTypeEnums.ShopWaiter
            }));

            var created = await _chatObjectManager.CreateAdminAsync(new ChatObjectAdminCreateArgs
            {
                Name = "改名保留测试",
                Code = "keep_code_01",
                Description = "原始说明",
                ChatObjectTypeId = "Personal",
                ObjectType = ChatObjectTypeEnums.Personal
            });

            var renamed = await _chatObjectManager.UpdateAdminAsync(
                created.Entity.Id,
                "改名保留测试-新",
                code: null,
                gender: null,
                description: null,
                portrait: null,
                thumbnail: null);

            Assert.Equal("keep_code_01", renamed.Entity.Code);
            Assert.Equal("原始说明", renamed.Entity.Description);
        });
    }

    [Fact]
    public async Task Session_request_and_announcement_write_actions_work()
    {
        var sessionId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var announcementId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            await InsertAsync<ChatObject>(context, new Dictionary<string, object>
            {
                [nameof(ChatObject.Id)] = 8301L,
                [nameof(ChatObject.Name)] = "治理操作人",
                [nameof(ChatObject.Code)] = "gov_op_8301",
                [nameof(ChatObject.ChatObjectTypeId)] = "Personal",
                [nameof(ChatObject.ObjectType)] = ChatObjectTypeEnums.Personal,
                [nameof(ChatObject.IsEnabled)] = true,
                [nameof(ChatObject.CreationTime)] = DateTime.UtcNow
            });

            await InsertAsync<Session>(context, new Dictionary<string, object>
            {
                [nameof(Session.Id)] = sessionId,
                [nameof(Session.Title)] = "禁言测试群",
                [nameof(Session.Channel)] = Channels.RoomChannel,
                [nameof(Session.IsEnabled)] = true,
                [nameof(Session.IsAllMuted)] = false,
                [nameof(Session.CreationTime)] = DateTime.UtcNow
            });

            await InsertAsync<SessionRequest>(context, new Dictionary<string, object>
            {
                [nameof(SessionRequest.Id)] = requestId,
                [nameof(SessionRequest.OwnerId)] = 8301L,
                [nameof(SessionRequest.DestinationId)] = 8302L,
                [nameof(SessionRequest.RequestMessage)] = "申请入群",
                [nameof(SessionRequest.IsHandled)] = false,
                [nameof(SessionRequest.IsEnabled)] = true,
                [nameof(SessionRequest.CreationTime)] = DateTime.UtcNow
            });

            await InsertAsync<SessionAnnouncement>(context, new Dictionary<string, object>
            {
                [nameof(SessionAnnouncement.Id)] = announcementId,
                [nameof(SessionAnnouncement.SessionId)] = sessionId,
                [nameof(SessionAnnouncement.ChatObjectId)] = 8301L,
                [nameof(SessionAnnouncement.PublisherChatObjectId)] = 8301L,
                [nameof(SessionAnnouncement.Title)] = "置顶公告测试",
                [nameof(SessionAnnouncement.Content)] = "内容",
                [nameof(SessionAnnouncement.Status)] = AnnouncementStatus.Published,
                [nameof(SessionAnnouncement.IsPinned)] = false,
                [nameof(SessionAnnouncement.IsApproved)] = true,
                [nameof(SessionAnnouncement.Version)] = 1,
                [nameof(SessionAnnouncement.CreationTime)] = DateTime.UtcNow
            });
        });

        var muted = await _sessionAppService.SetIsAllMutedAsync(sessionId, new SessionAdminSetIsAllMutedInput { IsAllMuted = true });
        Assert.True(muted.IsAllMuted);

        var handled = await _sessionRequestAppService.HandleAsync(requestId, new SessionRequestAdminHandleInput
        {
            IsAgreed = true,
            HandleMessage = "管理员审核通过"
        });
        Assert.True(handled.IsHandled);
        Assert.True(handled.IsAgreed);

        await Assert.ThrowsAsync<BusinessException>(() =>
            _sessionRequestAppService.HandleAsync(requestId, new SessionRequestAdminHandleInput { IsAgreed = false }));

        var pinned = await _announcementAppService.SetPinnedAsync(announcementId, new SessionAnnouncementAdminSetPinnedInput { IsPinned = true });
        Assert.True(pinned.IsPinned);
    }
}
