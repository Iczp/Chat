using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.SessionAnnouncements;
using IczpNet.Chat.Admin.SessionRequests;
using IczpNet.Chat.Admin.Sessions;
using IczpNet.Chat.AiRuns;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageTemplates;
using IczpNet.Chat.SessionRequests;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionSections.Announcements;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Security.Claims;
using Xunit;

namespace IczpNet.Chat.Admin;

public class ChatAdminAuthorizationNegative_Tests : ChatTestBase<ChatAdminApplicationTestModule>
{
    private const string ValidTemplateJson =
        """{"schemaVersion":1,"name":"范围测试","fields":[],"layout":{"type":"card"}}""";

    private readonly IMessageTemplateAdminManager _templateAdminManager;
    private readonly IArticleScheduleAdminManager _scheduleAdminManager;
    private readonly IChatObjectManager _chatObjectManager;
    private readonly IMessageAdminManager _messageAdminManager;
    private readonly IAiRunManager _aiRunManager;
    private readonly ISessionAdminAppService _sessionAppService;
    private readonly ISessionRequestAdminAppService _sessionRequestAppService;
    private readonly ISessionAnnouncementAdminAppService _announcementAppService;

    public ChatAdminAuthorizationNegative_Tests()
    {
        _templateAdminManager = GetRequiredService<IMessageTemplateAdminManager>();
        _scheduleAdminManager = GetRequiredService<IArticleScheduleAdminManager>();
        _chatObjectManager = GetRequiredService<IChatObjectManager>();
        _messageAdminManager = GetRequiredService<IMessageAdminManager>();
        _aiRunManager = GetRequiredService<IAiRunManager>();
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
        var privateSessionId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var announcementId = Guid.NewGuid();
        var operatorUserId = Guid.NewGuid();
        var outsiderUserId = Guid.NewGuid();
        var operatorUnitId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var typeRepo = GetRequiredService<IRepository<ChatObjectType, string>>();
            foreach (var (id, objType) in new[]
            {
                ("Personal", ChatObjectTypeEnums.Personal),
                ("Room", ChatObjectTypeEnums.Room)
            })
            {
                if (await typeRepo.FindAsync(id) == null)
                {
                    await typeRepo.InsertAsync(new ChatObjectType(id) { Name = id, ObjectType = objType }, autoSave: true);
                }
            }

            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            await InsertAsync<ChatObject>(context, new Dictionary<string, object>
            {
                [nameof(ChatObject.Id)] = 8301L,
                [nameof(ChatObject.Name)] = "治理申请人",
                [nameof(ChatObject.Code)] = "gov_op_8301",
                [nameof(ChatObject.ChatObjectTypeId)] = "Personal",
                [nameof(ChatObject.ObjectType)] = ChatObjectTypeEnums.Personal,
                [nameof(ChatObject.IsEnabled)] = true,
                [nameof(ChatObject.CreationTime)] = DateTime.UtcNow
            });

            await InsertAsync<ChatObject>(context, new Dictionary<string, object>
            {
                [nameof(ChatObject.Id)] = 8302L,
                [nameof(ChatObject.Name)] = "目标好友",
                [nameof(ChatObject.Code)] = "gov_target_8302",
                [nameof(ChatObject.AppUserId)] = operatorUserId,
                [nameof(ChatObject.ChatObjectTypeId)] = "Personal",
                [nameof(ChatObject.ObjectType)] = ChatObjectTypeEnums.Personal,
                [nameof(ChatObject.IsEnabled)] = true,
                [nameof(ChatObject.CreationTime)] = DateTime.UtcNow
            });

            await InsertAsync<ChatObject>(context, new Dictionary<string, object>
            {
                [nameof(ChatObject.Id)] = 8303L,
                [nameof(ChatObject.Name)] = "越权操作人",
                [nameof(ChatObject.Code)] = "gov_outsider_8303",
                [nameof(ChatObject.AppUserId)] = outsiderUserId,
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

            await InsertAsync<Session>(context, new Dictionary<string, object>
            {
                [nameof(Session.Id)] = privateSessionId,
                [nameof(Session.Title)] = "单聊会话",
                [nameof(Session.Channel)] = Channels.PrivateChannel,
                [nameof(Session.IsEnabled)] = true,
                [nameof(Session.IsAllMuted)] = false,
                [nameof(Session.CreationTime)] = DateTime.UtcNow
            });

            await InsertAsync<SessionUnit>(context, new Dictionary<string, object>
            {
                [nameof(SessionUnit.Id)] = operatorUnitId,
                [nameof(SessionUnit.SessionId)] = sessionId,
                [nameof(SessionUnit.OwnerId)] = 8302L,
                [nameof(SessionUnit.DestinationId)] = 8302L,
                [nameof(SessionUnit.CreationTime)] = DateTime.UtcNow
            });

            await InsertAsync<SessionUnitSetting>(context, new Dictionary<string, object>
            {
                [nameof(SessionUnitSetting.SessionUnitId)] = operatorUnitId,
                [nameof(SessionUnitSetting.IsEnabled)] = true,
                [nameof(SessionUnitSetting.IsKilled)] = false
            });

            await InsertAsync<SessionRequest>(context, new Dictionary<string, object>
            {
                [nameof(SessionRequest.Id)] = requestId,
                [nameof(SessionRequest.OwnerId)] = 8301L,
                [nameof(SessionRequest.DestinationId)] = 8302L,
                [nameof(SessionRequest.RequestMessage)] = "申请加为好友",
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

        var principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();

        // 越权用户即使拥有模块策略，也不能对未授权的目标会话/申请执行禁言、审批、置顶
        using (principalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity([new Claim(AbpClaimTypes.UserId, outsiderUserId.ToString())]))))
        {
            await Assert.ThrowsAsync<AbpAuthorizationException>(() =>
                _sessionAppService.SetIsAllMutedAsync(sessionId, new SessionAdminSetIsAllMutedInput { IsAllMuted = true }));
            await Assert.ThrowsAsync<AbpAuthorizationException>(() =>
                _sessionRequestAppService.HandleAsync(requestId, new SessionRequestAdminHandleInput { IsAgreed = true }));
            await Assert.ThrowsAsync<AbpAuthorizationException>(() =>
                _announcementAppService.SetPinnedAsync(announcementId, new SessionAnnouncementAdminSetPinnedInput { IsPinned = true }));
        }

        // 授权用户可正常执行禁言、审批、置顶，且单聊不可禁言
        using (principalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity([new Claim(AbpClaimTypes.UserId, operatorUserId.ToString())]))))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var privateScope = new SessionAdminScope
                {
                    HasManageAll = false,
                    AllowedSessionIds = [privateSessionId]
                };
                await Assert.ThrowsAsync<BusinessException>(() =>
                    GetRequiredService<ISessionAdminManager>().SetIsAllMutedAdminAsync(privateSessionId, true, privateScope));
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

            await WithUnitOfWorkAsync(async () =>
            {
                var unitRepo = GetRequiredService<ISessionUnitRepository>();
                var units = await unitRepo.GetListAsync(x =>
                    (x.OwnerId == 8301L && x.DestinationId == 8302L) ||
                    (x.OwnerId == 8302L && x.DestinationId == 8301L));
                Assert.Equal(2, units.Count);
            });

            await Assert.ThrowsAsync<BusinessException>(() =>
                _sessionRequestAppService.HandleAsync(requestId, new SessionRequestAdminHandleInput { IsAgreed = false }));

            var pinned = await _announcementAppService.SetPinnedAsync(announcementId, new SessionAnnouncementAdminSetPinnedInput { IsPinned = true });
            Assert.True(pinned.IsPinned);
        }
    }

    [Fact]
    public async Task Message_scope_enforces_session_and_owner_isolation()
    {
        var sessionA = Guid.NewGuid();
        var sessionB = Guid.NewGuid();
        const long messageAId = 91001L;
        const long messageBId = 91002L;
        const long privateInSessionAId = 91003L;
        const long publicOtherSenderInSessionAId = 91004L;

        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            await InsertAsync<Message>(context, new Dictionary<string, object>
            {
                [nameof(Message.Id)] = messageAId,
                [nameof(Message.SessionId)] = sessionA,
                [nameof(Message.SessionSeq)] = 1L,
                [nameof(Message.SenderId)] = 9101L,
                [nameof(Message.ReceiverId)] = 9102L,
                [nameof(Message.MessageType)] = MessageTypes.Text,
                [nameof(Message.Channel)] = Channels.RoomChannel,
                [nameof(Message.IsPrivate)] = false,
                [nameof(Message.ContentJson)] = """{"text":"secret-A"}""",
                [nameof(Message.CreationTime)] = DateTime.UtcNow
            });

            await InsertAsync<Message>(context, new Dictionary<string, object>
            {
                [nameof(Message.Id)] = privateInSessionAId,
                [nameof(Message.SessionId)] = sessionA,
                [nameof(Message.SessionSeq)] = 2L,
                [nameof(Message.SenderId)] = 9102L,
                [nameof(Message.ReceiverId)] = 9103L,
                [nameof(Message.MessageType)] = MessageTypes.Text,
                [nameof(Message.Channel)] = Channels.RoomChannel,
                [nameof(Message.IsPrivate)] = true,
                [nameof(Message.ContentJson)] = """{"text":"private-other"}""",
                [nameof(Message.CreationTime)] = DateTime.UtcNow
            });

            await InsertAsync<Message>(context, new Dictionary<string, object>
            {
                [nameof(Message.Id)] = publicOtherSenderInSessionAId,
                [nameof(Message.SessionId)] = sessionA,
                [nameof(Message.SessionSeq)] = 3L,
                [nameof(Message.SenderId)] = 9102L,
                [nameof(Message.ReceiverId)] = 9101L,
                [nameof(Message.MessageType)] = MessageTypes.Text,
                [nameof(Message.Channel)] = Channels.RoomChannel,
                [nameof(Message.IsPrivate)] = false,
                [nameof(Message.ContentJson)] = """{"text":"public-other"}""",
                [nameof(Message.CreationTime)] = DateTime.UtcNow
            });

            await InsertAsync<Message>(context, new Dictionary<string, object>
            {
                [nameof(Message.Id)] = messageBId,
                [nameof(Message.SessionId)] = sessionB,
                [nameof(Message.SenderId)] = 9201L,
                [nameof(Message.ReceiverId)] = 9202L,
                [nameof(Message.MessageType)] = MessageTypes.Text,
                [nameof(Message.Channel)] = Channels.PrivateChannel,
                [nameof(Message.IsPrivate)] = false,
                [nameof(Message.ContentJson)] = """{"text":"secret-B"}""",
                [nameof(Message.CreationTime)] = DateTime.UtcNow
            });
        });

        var scopeA = new MessageAdminScope
        {
            HasManageAll = false,
            AllowedOwnerIds = [9101L],
            AllowedSessionIds = [sessionA]
        };

        var crossTenantScope = new MessageAdminScope
        {
            TenantId = Guid.NewGuid(),
            HasManageAll = true
        };

        await WithUnitOfWorkAsync(async () =>
        {
            var list = await _messageAdminManager.GetAdminListAsync(
                null, null, null, null, null, null, null, null, null, 0, 20, scopeA);
            Assert.Equal(2, list.Items.Count);
            Assert.DoesNotContain(list.Items, x => x.Id == privateInSessionAId || x.Id == messageBId);

            var detailA = await _messageAdminManager.GetAdminDetailAsync(messageAId, scopeA);
            Assert.Equal(messageAId, detailA.Id);

            // 同群但非私密消息参与者的成员，不得查看私密消息详情，也不得撤回他人消息
            await Assert.ThrowsAsync<AbpAuthorizationException>(() =>
                _messageAdminManager.GetAdminDetailAsync(privateInSessionAId, scopeA));
            await Assert.ThrowsAsync<AbpAuthorizationException>(() =>
                _messageAdminManager.RollbackAdminAsync(privateInSessionAId, scopeA));
            await Assert.ThrowsAsync<AbpAuthorizationException>(() =>
                _messageAdminManager.RollbackAdminAsync(publicOtherSenderInSessionAId, scopeA));

            // 跨会话与跨租户均应拒绝
            await Assert.ThrowsAsync<AbpAuthorizationException>(() =>
                _messageAdminManager.GetAdminDetailAsync(messageBId, scopeA));
            await Assert.ThrowsAsync<AbpAuthorizationException>(() =>
                _messageAdminManager.RollbackAdminAsync(messageBId, scopeA));
            await Assert.ThrowsAsync<AbpAuthorizationException>(() =>
                _messageAdminManager.GetAdminDetailAsync(messageAId, crossTenantScope));
        });
    }

    [Fact]
    public async Task AiRun_batch_delete_validates_empty_limit_and_running_state()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var emptyEx = await Assert.ThrowsAsync<BusinessException>(() =>
                _aiRunManager.DeleteManyAdminAsync(new List<Guid>()));
            Assert.Equal("ChatAdmin:AiRunBatchEmpty", emptyEx.Code);

            var tooMany = Enumerable.Range(0, 101).Select(_ => Guid.NewGuid()).ToList();
            var limitEx = await Assert.ThrowsAsync<BusinessException>(() =>
                _aiRunManager.DeleteManyAdminAsync(tooMany));
            Assert.Equal("ChatAdmin:AiRunBatchLimitExceeded", limitEx.Code);

            var aiRunRepo = GetRequiredService<IRepository<AiRun, Guid>>();
            var sessionId = Guid.NewGuid();
            var unitId = Guid.NewGuid();
            var run1 = await _aiRunManager.CreateAsync(93001L, sessionId, unitId, "openai");
            var run2 = await _aiRunManager.CreateAsync(93002L, sessionId, unitId, "openai");
            var running = await _aiRunManager.CreateAsync(93003L, sessionId, unitId, "openai");
            running.Claim("worker-1", DateTime.UtcNow, DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow.AddMinutes(10));
            await aiRunRepo.UpdateAsync(running, autoSave: true);

            var runningEx = await Assert.ThrowsAsync<BusinessException>(() =>
                _aiRunManager.DeleteManyAdminAsync(new List<Guid> { run1.Id, running.Id }));
            Assert.Equal("ChatAdmin:CannotDeleteRunningAiRun", runningEx.Code);

            await _aiRunManager.DeleteManyAdminAsync(new List<Guid> { run1.Id, run2.Id });
            Assert.Null(await aiRunRepo.FindAsync(run1.Id));
            Assert.Null(await aiRunRepo.FindAsync(run2.Id));
        });
    }
}
