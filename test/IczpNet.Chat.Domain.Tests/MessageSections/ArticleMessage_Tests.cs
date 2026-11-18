using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Templates;
using System;
using System.Linq;
using System.Text.Json;
using Volo.Abp;
using Xunit;

namespace IczpNet.Chat.MessageSections.Tests;

public class ArticleMessage_Tests
{
    private static ArticleItemInfo Item(string title) => new() { Title = title, OriginalUrl = "https://example.com/article" };
    [Fact]
    public void Group_is_an_ordered_snapshot_and_survives_extra_properties_roundtrip()
    {
        var input = new ArticleContentInput { Items = [Item("First"), Item("Second")] };
        var content = ArticleContent.Create(input);
        input.Items[0].Title = "Changed";
        var json = JsonSerializer.Serialize(content.ExtraProperties["ArticleItems"]);
        var restored = new ArticleContent();
        restored.ExtraProperties["ArticleItems"] = JsonSerializer.Deserialize<string>(json);
        Assert.Equal(new[] { "First", "Second" }, restored.GetItems().Select(x => x.Title));
        Assert.Equal("First", content.Title);
    }
    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("file:///secret")]
    [InlineData("https://user:secret@example.com")]
    public void Rejects_unsafe_reading_urls(string url)
    {
        var item = Item("Title"); item.OriginalUrl = url;
        Assert.Throws<UserFriendlyException>(() => ArticleContent.Create(new ArticleContentInput { Items = [item] }));
    }
    [Fact]
    public void Empty_or_oversized_group_is_rejected_and_legacy_row_still_reads()
    {
        Assert.Throws<UserFriendlyException>(() => ArticleContent.Create(new ArticleContentInput()));
        Assert.Throws<UserFriendlyException>(() => ArticleContent.Create(new ArticleContentInput { Items = Enumerable.Range(0, 9).Select(x => Item(x.ToString())).ToList() }));
        Assert.Equal("Legacy", new ArticleContent { Title = "Legacy", OriginalUrl = "https://example.com" }.GetItems().Single().Title);
    }
    [Fact]
    public void Cancel_and_reschedule_invalidate_old_jobs_and_sent_tasks_are_terminal()
    {
        var task = new ScheduledArticleMessage(Guid.NewGuid(), Guid.NewGuid(), 1, Guid.NewGuid(), "{}", DateTime.UtcNow);
        task.Reschedule(DateTime.UtcNow.AddHours(1)); Assert.Equal(2, task.Version);
        task.Cancel(); Assert.Equal(3, task.Version); Assert.Equal(ArticleScheduleStatus.Cancelled, task.Status);
        Assert.Throws<UserFriendlyException>(() => task.Reschedule(DateTime.UtcNow));
        var sent = new ScheduledArticleMessage(Guid.NewGuid(), Guid.NewGuid(), 1, Guid.NewGuid(), "{}", DateTime.UtcNow);
        sent.Start(DateTime.UtcNow.AddMinutes(5));
        Assert.Throws<UserFriendlyException>(() => sent.Cancel());
        sent.Complete(99, DateTime.UtcNow);
        Assert.Throws<UserFriendlyException>(() => sent.Cancel());
        Assert.Equal(99, sent.MessageId);
    }
}
