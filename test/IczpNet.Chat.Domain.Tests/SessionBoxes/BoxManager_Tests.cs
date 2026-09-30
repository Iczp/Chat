using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Xunit;

namespace IczpNet.Chat.SessionBoxes;

[DependsOn(typeof(ChatDomainTestModule))]
public class BoxManagerTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Replace(ServiceDescriptor.Singleton(Substitute.For<IDataSeeder>()));
    }
}

public class BoxManager_Tests : ChatTestBase<BoxManagerTestModule>
{
    [Fact]
    public async Task GetCacheListByOwnersAsync_should_return_empty_lists_on_cache_miss_and_mixed_hit()
    {
        var manager = ServiceProvider.GetRequiredService<IBoxManager>();
        const long firstOwnerId = -100001;
        const long secondOwnerId = -100002;

        await WithUnitOfWorkAsync(async () =>
        {
            var first = await manager.GetCacheListByOwnersAsync([firstOwnerId]);
            Assert.Empty(first[firstOwnerId]);

            var mixed = await manager.GetCacheListByOwnersAsync([firstOwnerId, secondOwnerId]);
            Assert.Equal(2, mixed.Count);
            Assert.Empty(mixed[firstOwnerId]);
            Assert.Empty(mixed[secondOwnerId]);
        });
    }
}
