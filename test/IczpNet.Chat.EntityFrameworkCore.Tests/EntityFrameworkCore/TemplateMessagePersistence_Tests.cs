using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.MessageTemplates;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace IczpNet.Chat.EntityFrameworkCore;

public class TemplateMessagePersistence_Tests : ChatTestBase<MessageQueryReviewTestModule>
{
    private const string Definition = """{"schemaVersion":1,"name":"通知","layout":{"type":"card"}}""";

    [Fact]
    public async Task Snapshot_and_scalar_metadata_roundtrip_without_loading_template_definitions()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var db = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var id = Guid.NewGuid();
            var snapshot = new TemplateContentInfo { TemplateId = id, TemplateVersion = 2, SchemaVersion = 1,
                FallbackText = "固定摘要", TemplateName = "通知", Data = new { title = "已发送正文" }, Layout = new { type = "card" }, Actions = Array.Empty<object>() };
            var json = JsonSerializer.Serialize(snapshot);
            var content = new TemplateContent(id, 2, 1, snapshot.FallbackText, json);
            db.Set<TemplateContent>().Add(content);
            await db.SaveChangesAsync();
            var contentId = content.Id;
            db.ChangeTracker.Clear();
            var restored = await db.Set<TemplateContent>().SingleAsync(x => x.Id == contentId);
            Assert.Equal(id, restored.TemplateId);
            Assert.Equal(2, restored.TemplateVersion);
            Assert.Equal(json, restored.SnapshotJson);
            Assert.Equal("固定摘要", restored.GetBody());
            Assert.Empty(db.ChangeTracker.Entries<MessageTemplateDefinition>());
        });
    }

    [Fact]
    public async Task Tenant_filter_and_revision_unique_constraint_are_active_in_relational_model()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        // 唯一约束失败终止整个测试事务，不在失败的工作单元上继续提交。
        await Assert.ThrowsAsync<DbUpdateException>(() => WithUnitOfWorkAsync(async () =>
        {
            var currentTenant = GetRequiredService<ICurrentTenant>();
            var db = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            // Isolated SQLite fixture; owner rows use existing generic fixture seeding.
            await MessageVisibilityQuery_Tests.InsertAsync<ChatObject>(db, new() { ["Id"] = 781L, ["Name"] = "owner" });
            var first = new MessageTemplateDefinition(Guid.NewGuid(), 781, "A", null, MessageTemplateVisibility.Private, Definition, tenantA);
            var second = new MessageTemplateDefinition(Guid.NewGuid(), 781, "B", null, MessageTemplateVisibility.Private, Definition, tenantB);
            db.Add(first); db.Add(second);
            await db.SaveChangesAsync();
            using (currentTenant.Change(tenantA))
            {
                Assert.Equal(new[] { first.Id }, await db.Set<MessageTemplateDefinition>().Select(x => x.Id).ToArrayAsync());
            }
            using (currentTenant.Change(tenantB))
            {
                Assert.Equal(new[] { second.Id }, await db.Set<MessageTemplateDefinition>().Select(x => x.Id).ToArrayAsync());
            }
            var revision = new MessageTemplateRevision(Guid.NewGuid(), first.Id, 1, 1, Definition, "hash", tenantA);
            db.Add(revision);
            await db.SaveChangesAsync();
            var duplicate = new MessageTemplateRevision(Guid.NewGuid(), first.Id, 1, 1, Definition, "hash", tenantA);
            db.Add(duplicate);
            await db.SaveChangesAsync();
        }));
    }

    [Fact]
    public async Task Stale_template_stamp_is_rejected_by_database_update()
    {
        await Assert.ThrowsAsync<AbpDbConcurrencyException>(() => WithUnitOfWorkAsync(async () =>
        {
            var db = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await MessageVisibilityQuery_Tests.InsertAsync<ChatObject>(db, new() { ["Id"] = 782L, ["Name"] = "owner" });
            var template = new MessageTemplateDefinition(Guid.NewGuid(), 782, "A", null, MessageTemplateVisibility.Private, Definition);
            db.Add(template);
            await db.SaveChangesAsync();
            var stale = await db.Set<MessageTemplateDefinition>().AsNoTracking().SingleAsync(x => x.Id == template.Id);
            template.SetPublishedVersion(1);
            await db.SaveChangesAsync();
            db.Entry(template).State = EntityState.Detached;
            db.Attach(stale);
            stale.SetPublishedVersion(1);
            await db.SaveChangesAsync();
        }));
    }
}
