using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
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
