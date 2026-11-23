using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageTemplates;
using IczpNet.Chat.MessageTemplates.Dtos;
using IczpNet.Chat.Permissions;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Volo.Abp.Users;
using Xunit;

namespace IczpNet.Chat;

public class MessageTemplateSecurity_Tests
{
    private const string Definition = """{"schemaVersion":1,"name":"通知","fields":[],"layout":{"type":"card"}}""";

    [DisableConventionalRegistration]
    public class EnabledManager : MessageTemplateManager
    {
        public override void EnsureEnabled() { }
    }

    private static (MessageTemplateAppService Service, IRepository<MessageTemplateDefinition, Guid> Templates,
        IAbpAuthorizationService Authorization) Create(params long[] owners)
    {
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        var authorization = Substitute.For<IAbpAuthorizationService>();
        authorization.AuthorizeAsync(Arg.Any<object>(), Arg.Any<string>()).Returns(AuthorizationResult.Success());
        authorization.AuthorizeAsync(Arg.Any<object>(), ChatPermissions.MessageTemplatePermission.ManageAll).Returns(AuthorizationResult.Failed());
        lazy.LazyGetRequiredService<IAuthorizationService>().Returns(authorization);
        var user = Substitute.For<ICurrentUser>();
        var userId = Guid.NewGuid();
        user.Id.Returns(userId);
        lazy.LazyGetRequiredService<ICurrentUser>().Returns(user);
        var objects = Substitute.For<IChatObjectManager>();
        objects.GetIdListByUserIdAsync(userId).Returns(owners.ToList());
        lazy.LazyGetRequiredService<IChatObjectManager>().Returns(objects);
        lazy.LazyGetRequiredService<ICancellationTokenProvider>().Returns(Substitute.For<ICancellationTokenProvider>());
        var templates = Substitute.For<IRepository<MessageTemplateDefinition, Guid>>();
        return (new MessageTemplateAppService(templates, Substitute.For<IRepository<MessageTemplateRevision, Guid>>(), new EnabledManager())
            { LazyServiceProvider = lazy }, templates, authorization);
    }

    [Fact]
    public async Task Preview_requires_an_existing_template_not_an_arbitrary_definition()
    {
        var probe = Create(1);
        var missing = Assert.ThrowsAsync<UserFriendlyException>(() => probe.Service.PreviewAsync(new()
            { Definition = Definition, Data = [] }));
        Assert.Equal(ChatErrorCodes.TemplateMessages.TemplateNotFound, (await missing).Code);
        Assert.Equal(ChatErrorCodes.TemplateMessages.TemplateNotFound, (await Assert.ThrowsAsync<UserFriendlyException>(() =>
            probe.Service.PreviewAsync(new() { TemplateId = Guid.NewGuid(), Definition = Definition, Data = [] }))).Code);
    }

    [Fact]
    public async Task Use_only_permission_cannot_preview_edit_or_publish()
    {
        var probe = Create(1);
        probe.Authorization.AuthorizeAsync(Arg.Any<object>(), ChatPermissions.MessageTemplatePermission.Update).Returns(AuthorizationResult.Failed());
        probe.Authorization.AuthorizeAsync(Arg.Any<object>(), ChatPermissions.MessageTemplatePermission.Publish).Returns(AuthorizationResult.Failed());
        await Assert.ThrowsAsync<AbpAuthorizationException>(() => probe.Service.PreviewAsync(new()
            { TemplateId = Guid.NewGuid(), Definition = Definition, Data = [] }));
        await Assert.ThrowsAsync<AbpAuthorizationException>(() => probe.Service.UpdateAsync(Guid.NewGuid(), new()));
        await Assert.ThrowsAsync<AbpAuthorizationException>(() => probe.Service.PublishAsync(Guid.NewGuid(), new()));
        Assert.Empty(probe.Templates.ReceivedCalls());
    }

    [Fact]
    public async Task Preview_rejects_a_foreign_private_template_even_with_update_permission()
    {
        var probe = Create(1);
        var template = new MessageTemplateDefinition(Guid.NewGuid(), 2, "foreign", null, MessageTemplateVisibility.Private, Definition);
        probe.Templates.FindAsync(template.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(template);
        await Assert.ThrowsAsync<AbpAuthorizationException>(() => probe.Service.PreviewAsync(new()
            { TemplateId = template.Id, Definition = Definition, Data = [] }));
    }

    [Fact]
    public async Task Missing_feature_configuration_rejects_before_reading_template_tables()
    {
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>()
            .Returns(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        var templates = Substitute.For<IRepository<MessageTemplateDefinition, Guid>>();
        var service = new MessageTemplateAppService(templates, Substitute.For<IRepository<MessageTemplateRevision, Guid>>(),
            new MessageTemplateManager { LazyServiceProvider = lazy });
        var error = await Assert.ThrowsAsync<UserFriendlyException>(() => service.GetAsync(Guid.NewGuid()));
        Assert.Equal(ChatErrorCodes.TemplateMessages.TemplateDisabled, error.Code);
        Assert.Empty(templates.ReceivedCalls());
    }
}
