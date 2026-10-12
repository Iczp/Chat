using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.ConnectionPools.Dtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.SessionUnitSettings.Dtos;
using Volo.Abp.Linq;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Testing;
using Volo.Abp.Users;
using Xunit;

namespace IczpNet.Chat;

[DependsOn(typeof(ChatApplicationModule))]
public class MapperlyApplicationProviderTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper();
    }
}

public class MapperlyApplicationProviderRegistration_Tests
    : AbpIntegratedTest<MapperlyApplicationProviderTestModule>
{
    [Fact]
    public void Member_settings_cache_maps_through_root_and_application_providers()
    {
        var source = new SessionUnitSettingCacheItem
        {
            SessionUnitId = Guid.NewGuid(), MemberName = "member", JoinWay = JoinWays.Invitation,
            LastSendMessageId = 123, LastSendTime = new DateTime(2026, 10, 12),
            IsStatic = true, IsPublic = false, IsVisible = false, IsEnabled = false,
            IsCreator = true, MuteExpireTime = new DateTime(2026, 10, 13),
        };
        foreach (var mapper in new IObjectMapper[]
        {
            ServiceProvider.GetRequiredService<IObjectMapper>(),
            ServiceProvider.GetRequiredService<IObjectMapper<ChatApplicationModule>>(),
        })
        {
            var result = mapper.Map<SessionUnitSettingCacheItem, SessionUnitMemberSettingDto>(source);
            result.SessionUnitId.ShouldBe(source.SessionUnitId);
            result.MemberName.ShouldBe(source.MemberName);
            result.JoinWay.ShouldBe(source.JoinWay);
            result.LastSendMessageId.ShouldBe(source.LastSendMessageId);
            result.LastSendTime.ShouldBe(source.LastSendTime);
            result.IsStatic.ShouldBeTrue();
            result.IsPublic.ShouldBeFalse();
            result.IsVisible.ShouldBeFalse();
            result.IsEnabled.ShouldBeFalse();
            result.IsCreator.ShouldBeTrue();
            result.MuteExpireTime.ShouldBe(source.MuteExpireTime);
            result.JoinTime.ShouldBeNull();
            result.JoinWayDescription.ShouldBeNull();
            mapper.Map<SessionUnitSettingCacheItem, SessionUnitMemberSettingDto>(null).ShouldBeNull();
        }
    }

    [Fact]
    public async Task Already_projected_connection_page_does_not_require_an_identity_map()
    {
        var rows = Enumerable.Range(0, 4).Select(i => new ConnectionPoolDto { ConnectionId = $"connection-{i}" }).ToList();
        var result = await rows.AsQueryable().ToPagedListAsync<ConnectionPoolDto, ConnectionPoolDto>(
            ServiceProvider.GetRequiredService<IAsyncQueryableExecuter>(),
            ServiceProvider.GetRequiredService<IObjectMapper>(), maxResultCount: 2, skipCount: 1,
            queryableAction: query => query.OrderBy(row => row.ConnectionId));

        result.TotalCount.ShouldBe(4);
        result.Items.Select(row => row.ConnectionId).ShouldBe(new[] { "connection-1", "connection-2" });
    }

    [Fact]
    public async Task Connection_page_still_maps_cache_items_to_dtos()
    {
        var result = await new List<ConnectionPoolCacheItem> { new() { ConnectionId = "connection" } }
            .AsQueryable().ToPagedListAsync<ConnectionPoolCacheItem, ConnectionPoolDto>(
                ServiceProvider.GetRequiredService<IAsyncQueryableExecuter>(),
                ServiceProvider.GetRequiredService<IObjectMapper>());

        result.TotalCount.ShouldBe(1);
        result.Items.Single().ConnectionId.ShouldBe("connection");
    }


    [Fact]
    public void Root_provider_maps_identity_user_to_user_event_transfer_object()
    {
        var mapper = ServiceProvider.GetRequiredService<IObjectMapper>();
        var user = new IdentityUser(Guid.NewGuid(), "admin", "admin@example.test");

        var result = mapper.Map<IdentityUser, UserEto>(user);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(user.Id);
        result.UserName.ShouldBe(user.UserName);
    }
}
