using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Templates;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

namespace IczpNet.Chat.EntityFrameworkCore;

public class ArticleMessagePersistence_Tests : ChatTestBase<MessageQueryReviewTestModule>
{
    [Fact]
    public async Task Article_group_roundtrips_through_the_existing_extra_properties_column()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var content = ArticleContent.Create(new ArticleContentInput { Items = [
                new ArticleItemInfo { Title = "First", OriginalUrl = "https://example.com/first" },
                new ArticleItemInfo { Title = "Second", OriginalUrl = "https://example.com/second" }] });
            db.Set<ArticleContent>().Add(content);
            await db.SaveChangesAsync();
            var id = content.Id;
            db.ChangeTracker.Clear();
            var restored = await db.Set<ArticleContent>().SingleAsync(x => x.Id == id);
            Assert.Equal(2, restored.GetItems().Count);
            Assert.Equal("Second", restored.GetItems()[1].Title);
        });
    }
    [Fact]
    public async Task Schedule_state_version_and_tenant_roundtrip_through_the_mapped_table()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var tenantId = Guid.NewGuid();
            var task = new ScheduledArticleMessage(Guid.NewGuid(), Guid.NewGuid(), 1, Guid.NewGuid(), "{}", DateTime.UtcNow.AddHours(1), tenantId);
            db.Set<ScheduledArticleMessage>().Add(task); await db.SaveChangesAsync();
            task.Cancel(); await db.SaveChangesAsync(); db.ChangeTracker.Clear();
            var restored = await db.Set<ScheduledArticleMessage>().SingleAsync(x => x.Id == task.Id);
            Assert.Equal(ArticleScheduleStatus.Cancelled, restored.Status);
            Assert.Equal(2, restored.Version);
            Assert.Equal(tenantId, restored.RequestedByTenantId);
        });
    }
}
