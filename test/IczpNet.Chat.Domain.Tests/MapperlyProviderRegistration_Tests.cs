#nullable enable
using IczpNet.AbpTrees;
using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.ChatObjectCategories;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.EntryNames;
using IczpNet.Chat.Menus;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.SessionOrganizations;
using IczpNet.Chat.SessionPermissionGroups;
using IczpNet.Chat.UrlNormalizers;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        context.Services.AddHttpContextAccessor();
        context.Services.AddSingleton<IConnectionPoolManager>(_ =>
            DispatchProxy.Create<IConnectionPoolManager,
                MapperlyProviderRegistration_Tests.DeviceTypeConnectionProxy>());
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
            message.ReadCount.ShouldBe(1, $"Content was evaluated repeatedly for {contentType.Name}");
        }
    }

    [Fact]
    public void Cache_and_job_message_maps_read_content_once()
    {
        typeof(MessageInfo<dynamic>).ShouldBe(typeof(MessageInfo<object>));
        var root = ServiceProvider.GetRequiredService<IObjectMapper>();
        var message = new CountingMessage(new SessionUnitCacheItem
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            OwnerId = 123,
        });

        root.Map<Message, MessageCacheItem>(message).Content.ShouldBeSameAs(message.ExpectedContent);
        message.ReadCount.ShouldBe(1);

        root.Map<Message, MessageQuoteCacheItem>(message).Content.ShouldBeSameAs(message.ExpectedContent);
        message.ReadCount.ShouldBe(2);

        root.Map<Message, MessageInfo<object>>(message).Content.ShouldBeSameAs(message.ExpectedContent);
        message.ReadCount.ShouldBe(3);

        ((object)root.Map<Message, MessageInfo<dynamic>>(message).Content)
            .ShouldBeSameAs(message.ExpectedContent);
        message.ReadCount.ShouldBe(4);
    }

    [Fact]
    public void Nested_sender_owner_uses_url_and_device_type_mapping()
    {
        var connections = DispatchProxy.Create<IConnectionPoolManager, DeviceTypeConnectionProxy>();
        var ownerMapper = new ChatDomainChatObjectToChatObjectInfoMapper(new TestUrlNormalizer(), connections);
        var senderMapper = new ChatDomainSessionUnitToSessionUnitSenderInfoMapper(ownerMapper);
        var messageMapper = new ChatDomainMessageToMessageInfoTextContentInfoMapper(senderMapper);
        var owner = new ChatObject("alice", "alice", new ChatObjectType("person"), null);
        owner.SetPortrait("/thumb.png", "/portrait.png");
        var sender = new SessionUnit(Guid.NewGuid(), 0, 0, 0, 0, 0, 0, 0, 0);
        typeof(BaseSessionEntity<Guid>).GetProperty(nameof(BaseSessionEntity<Guid>.Owner))!
            .SetValue(sender, owner);
        typeof(SessionUnit).GetProperty(nameof(SessionUnit.Setting))!
            .SetValue(sender, new SessionUnitSetting());
        var message = new MessageWithTestContent(new SessionUnitCacheItem
        {
            Id = sender.Id,
            SessionId = Guid.NewGuid(),
            OwnerId = owner.Id,
        }, new TextContentInfo { Text = "hello" });
        typeof(Message).GetProperty(nameof(Message.SenderSessionUnit))!
            .SetValue(message, sender);

        var direct = senderMapper.Map(sender);
        var nested = messageMapper.Map(message);
        var cached = new ChatDomainMessageToMessageCacheItemMapper(senderMapper).Map(message);
        var job = new ChatDomainMessageToMessageInfoObjectMapper(senderMapper).Map(message);
        var quoted = new ChatDomainMessageToMessageQuoteCacheItemMapper(senderMapper).Map(message);

        direct.Owner.Thumbnail.ShouldBe("https://chat.example/thumb.png");
        direct.Owner.Portrait.ShouldBe("https://chat.example/portrait.png");
        direct.Owner.DeviceTypes.ShouldBe(["ios"]);
        nested.SenderSessionUnit.Owner.Thumbnail.ShouldBe(direct.Owner.Thumbnail);
        nested.SenderSessionUnit.Owner.Portrait.ShouldBe(direct.Owner.Portrait);
        nested.SenderSessionUnit.Owner.DeviceTypes.ShouldBe(direct.Owner.DeviceTypes);
        cached.SenderSessionUnit.Owner.Thumbnail.ShouldBe(direct.Owner.Thumbnail);
        job.SenderSessionUnit.Owner.DeviceTypes.ShouldBe(direct.Owner.DeviceTypes);
        quoted.SenderSessionUnit.Owner.Portrait.ShouldBe(direct.Owner.Portrait);
        ((DeviceTypeConnectionProxy)connections).CallCount.ShouldBe(5);
    }

    [Fact]
    public void Active_tree_manager_closed_mappings_are_registered()
    {
        var pairs = new (Type Source, Type Target)[]
        {
            (typeof(ChatObject), typeof(ChatObjectInfo)),
            (typeof(ChatObjectCategory), typeof(ChatObjectCategoryInfo)),
            (typeof(SessionOrganization), typeof(SessionOrganizationInfo)),
            (typeof(SessionPermissionGroup), typeof(SessionPermissionGroupInfo)),
            (typeof(Menu), typeof(MenuInfo)),
            (typeof(EntryName), typeof(TreeInfo<Guid>)),
        };
        var root = ServiceProvider.GetRequiredService<IObjectMapper>();
        var map = typeof(IObjectMapper).GetMethods().Single(method =>
            method.Name == nameof(IObjectMapper.Map)
            && method.IsGenericMethodDefinition
            && method.GetParameters().Length == 1);

        foreach (var (sourceType, targetType) in pairs)
        {
            var source = RuntimeHelpers.GetUninitializedObject(sourceType);
            var mapped = map.MakeGenericMethod(sourceType, targetType).Invoke(root, [source]);
            mapped.ShouldNotBeNull($"Missing active TreeManager mapping: {sourceType.Name} → {targetType.Name}");
        }
    }

    private sealed class TestUrlNormalizer : IUrlNormalizer
    {
        public string Normalize(string url) => url is null ? null! : $"https://chat.example{url}";
    }

    public class DeviceTypeConnectionProxy : DispatchProxy
    {
        public int CallCount { get; private set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name == nameof(IConnectionPoolManager.GetDeviceTypesAsync)
                && args is { Length: > 0 } && args[0] is long)
            {
                CallCount++;
                return Task.FromResult(new List<string> { "ios" });
            }

            throw new NotSupportedException(targetMethod?.Name);
        }
    }

    private sealed class CountingMessage(SessionUnitCacheItem unit) : Message(unit)
    {
        public object ExpectedContent { get; } = new();
        public int ReadCount { get; private set; }

        public override object GetContentDto()
        {
            ReadCount++;
            return ExpectedContent;
        }
    }

    private sealed class MessageWithTestContent(SessionUnitCacheItem unit, object content) : Message(unit)
    {
        public int ReadCount { get; private set; }

        public override object GetContentDto()
        {
            ReadCount++;
            return content;
        }
    }
}
