using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Linq;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Testing;
using Xunit;

namespace IczpNet.Chat;

[DependsOn(typeof(ChatDomainModule))]
public class MapperlyProviderTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper();
    }
}

public class MapperlyProviderRegistration_Tests : AbpIntegratedTest<MapperlyProviderTestModule>
{
    [Fact]
    public void Domain_context_and_root_provider_resolve_message_and_content_mappings()
    {
        var unit = new SessionUnitCacheItem
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            OwnerId = 123,
        };
        var message = new Message(unit) { ClientMessageId = "probe" };
        var scoped = ServiceProvider.GetRequiredService<IObjectMapper<ChatDomainModule>>();
        var root = ServiceProvider.GetRequiredService<IObjectMapper>();

        scoped.Map<Message, MessageCacheItem>(message).ClientMessageId.ShouldBe("probe");
        root.Map<Message, MessageCacheItem>(message).ClientMessageId.ShouldBe("probe");
        scoped.Map<TextContent, TextContentInfo>(new TextContent(Guid.NewGuid()) { Text = "hello" })
            .Text.ShouldBe("hello");
    }

    [Fact]
    public void Root_provider_maps_every_active_message_content_output_type()
    {
        var contentTypes = new[]
        {
            typeof(TextContentInfo), typeof(CmdContentInfo), typeof(LinkContentInfo),
            typeof(HtmlContentInfo), typeof(ImageContentInfo), typeof(SoundContentInfo),
            typeof(VideoContentInfo), typeof(FileContentInfo), typeof(LocationContentInfo),
            typeof(ContactsContentInfo), typeof(HistoryContentOutput),
            typeof(RedEnvelopeContentOutput),
        };
        var root = ServiceProvider.GetRequiredService<IObjectMapper>();
        var map = typeof(IObjectMapper).GetMethods().Single(method =>
            method.Name == nameof(IObjectMapper.Map)
            && method.IsGenericMethodDefinition
            && method.GetParameters().Length == 1);

        foreach (var contentType in contentTypes)
        {
            var content = Activator.CreateInstance(contentType)!;
            var message = new MessageWithTestContent(new SessionUnitCacheItem
            {
                Id = Guid.NewGuid(),
                SessionId = Guid.NewGuid(),
                OwnerId = 123,
            }, content);
            var resultType = typeof(MessageInfo<>).MakeGenericType(contentType);
            var result = map.MakeGenericMethod(typeof(Message), resultType)
                .Invoke(root, [message]);

            result.ShouldNotBeNull($"Missing Mapperly mapping for {contentType.Name}");
            resultType.GetProperty(nameof(MessageInfo<object>.Content))!.GetValue(result)
                .ShouldBeSameAs(content, $"Incorrect content mapping for {contentType.Name}");
        }
    }

    private sealed class MessageWithTestContent(SessionUnitCacheItem unit, object content) : Message(unit)
    {
        public override object GetContentDto() => content;
    }
}
