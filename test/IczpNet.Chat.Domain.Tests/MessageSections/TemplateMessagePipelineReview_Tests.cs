using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using NSubstitute;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace IczpNet.Chat.Domain.Tests.MessageSections;

public class TemplateMessagePipelineReview_Tests
{
    [Fact]
    public void Persisted_template_snapshot_can_be_read_without_a_content_collection_or_template_query()
    {
        var message = new Message(new SessionUnitCacheItem { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), OwnerId = 1 });
        typeof(Message).GetProperty(nameof(message.MessageType))!.SetValue(message, MessageTypes.Template);
        var snapshot = new TemplateContentInfo { TemplateId = Guid.NewGuid(), TemplateVersion = 1, SchemaVersion = 1,
            FallbackText = "persisted summary", Data = new { title = "original" }, Layout = new { type = "card" }, Actions = Array.Empty<object>() };
        typeof(Message).GetProperty(nameof(message.ContentJson))!.SetValue(message, JsonSerializer.Serialize(snapshot));
        Assert.Empty(message.TemplateContentList);
        var result = Assert.IsType<TemplateContentInfo>(message.GetContentDto());
        Assert.Equal("persisted summary", result.FallbackText);
        Assert.Equal(snapshot.TemplateId, result.TemplateId);
    }

    [Fact]
    public async Task Combined_history_forward_cannot_wrap_template_messages()
    {
        var cache = new SessionUnitCacheItem { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), OwnerId = 1 };
        var source = new Message(cache);
        source.DeletedList = [];
        typeof(Message).GetProperty(nameof(source.Id))!.SetValue(source, 88L);
        typeof(Message).GetProperty(nameof(source.MessageType))!.SetValue(source, MessageTypes.Template);
        var session = new Session(cache.SessionId!.Value, "test", Channels.RoomChannel);
        var unit = (SessionUnit)Activator.CreateInstance(typeof(SessionUnit), true)!;
        typeof(SessionUnit).GetProperty(nameof(unit.OwnerId))!.SetValue(unit, 1L);
        typeof(SessionUnit).GetProperty(nameof(unit.Setting))!.SetValue(unit, Activator.CreateInstance(typeof(SessionUnitSetting), true));
        session.UnitList.Add(unit);
        typeof(Message).GetProperty(nameof(source.Session))!.SetValue(source, session);
        var repository = Substitute.For<IMessageRepository>();
        repository.GetQueryableAsync().Returns(new[] { source }.AsQueryable());
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<IMessageRepository>().Returns(repository);
        var sender = new MessageSender(Substitute.For<IRepository<RedEnvelopeContent, Guid>>(), Substitute.For<IRedPacketManager>())
            { LazyServiceProvider = lazy };
        var error = await Assert.ThrowsAnyAsync<Exception>(() => sender.SendHistoryAsync(cache,
            new MessageInput<HistoryContentInput> { Content = new HistoryContentInput { MessageIdList = [88] } }));
        Assert.Contains("Template messages", error.Message);
    }
}
