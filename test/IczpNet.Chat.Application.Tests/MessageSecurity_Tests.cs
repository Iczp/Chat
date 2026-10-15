using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.MessageServices;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using NSubstitute;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;
using Xunit;

namespace IczpNet.Chat;

public class MessageSecurity_Tests
{
    [DisableConventionalRegistration]
    public class Probe : ChatAppService
    {
        public Task Check(params long?[] owners) => CheckPolicyForUserAsync(owners);
    }

    private static IAbpLazyServiceProvider OwnerServices(params long[] owned)
    {
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        var user = Substitute.For<ICurrentUser>();
        var userId = Guid.NewGuid();
        user.Id.Returns(userId);
        var objects = Substitute.For<IChatObjectManager>();
        objects.GetIdListByUserIdAsync(userId).Returns(owned.ToList());
        lazy.LazyGetRequiredService<ICurrentUser>().Returns(user);
        lazy.LazyGetRequiredService<IChatObjectManager>().Returns(objects);
        return lazy;
    }

    [Fact]
    public async Task Mixed_own_and_foreign_ids_cannot_bypass_ownership_and_callback_is_optional()
    {
        var service = new Probe { LazyServiceProvider = OwnerServices(1) };
        await service.Check(1);
        await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.Check(1, 2));
        await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.Check((long?)null));
    }

    [Fact]
    public async Task Foreign_session_cannot_delete_messages()
    {
        var manager = Substitute.For<ISessionUnitSettingManager>();
        var service = new SessionUnitSettingAppService(manager, Substitute.For<ISessionUnitRepository>(),
            Substitute.For<Volo.Abp.Domain.Repositories.IRepository<IczpNet.Chat.ContactTags.ContactTag, Guid>>())
            { LazyServiceProvider = OwnerServices(1) };
        var unit = (SessionUnit)Activator.CreateInstance(typeof(SessionUnit), true);
        typeof(SessionUnit).GetProperty(nameof(unit.Id))!.SetValue(unit, Guid.NewGuid());
        typeof(SessionUnit).GetProperty(nameof(unit.OwnerId))!.SetValue(unit, 2L);
        typeof(SessionUnit).GetProperty(nameof(unit.Setting))!.SetValue(unit, Activator.CreateInstance(typeof(SessionUnitSetting), true));
        var units = Substitute.For<ISessionUnitManager>();
        units.GetAsync(unit.Id).Returns(unit);
        service.LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>().Returns(units);
        await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.DeleteMessageAsync(unit.Id, 105));
        await manager.DidNotReceiveWithAnyArgs().DeleteMessageAsync(default, default);
        var legacyManager = Substitute.For<IczpNet.Chat.DeletedRecorders.IDeletedRecorderManager>();
        var legacy = new IczpNet.Chat.DeletedRecorders.DeletedRecorderAppService(legacyManager,
            Substitute.For<Volo.Abp.Domain.Repositories.IRepository<IczpNet.Chat.DeletedRecorders.DeletedRecorder>>(),
            Substitute.For<IczpNet.Chat.Devices.IDeviceResolver>()) { LazyServiceProvider = service.LazyServiceProvider };
        await Assert.ThrowsAsync<AbpAuthorizationException>(() => legacy.SetDeletedAsync(new()
            { SessionUnitId = unit.Id, MessageId = 105 }));
        await legacyManager.DidNotReceiveWithAnyArgs().CreateIfNotContainsAsync(default, default, default);
    }

    [Fact]
    public async Task Foreign_session_cannot_send_and_foreign_message_cannot_be_recalled()
    {
        var repository = Substitute.For<IMessageRepository>();
        var manager = Substitute.For<IMessageManager>();
        var sender = Substitute.For<IMessageSender>();
        var service = new MessageSenderAppService(Substitute.For<ISessionUnitCacheManager>(), repository, manager, sender)
        { LazyServiceProvider = OwnerServices(1) };
        var units = Substitute.For<ISessionUnitManager>();
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 2, SessionId = Guid.NewGuid(), IsEnabled = true };
        units.GetCacheAsync(unit.Id).Returns(unit);
        service.LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>().Returns(units);

        await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.SendTextAsync(unit.Id,
            new MessageInput<TextContentInfo> { ClientMessageId = "foreign", Content = new TextContentInfo { Text = "blocked" } }));
        await sender.DidNotReceiveWithAnyArgs().SendTextAsync(default, default);
        var message = new Message(unit);
        repository.GetAsync(42, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(message);
        await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.RollbackAsync(42));
        await manager.DidNotReceiveWithAnyArgs().RollbackAsync(default);
    }
}
