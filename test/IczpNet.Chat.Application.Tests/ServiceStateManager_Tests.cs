using System.Collections.Generic;
using System.Threading.Tasks;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.ServiceStates;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace IczpNet.Chat;

public class ServiceStateManager_Tests
{
    [Fact]
    public async Task Empty_device_list_is_not_online()
    {
        var manager = new TestManager([]);
        Assert.Null(await manager.GetStatusAsync(1));
        Assert.False(await manager.IsOnlineAsync(1));
    }

    [Theory]
    [InlineData(ServiceStatus.Online, true)]
    [InlineData(ServiceStatus.Offline, false)]
    [InlineData(ServiceStatus.Pending, false)]
    [InlineData(ServiceStatus.Stealth, false)]
    public async Task Only_online_status_is_available(ServiceStatus status, bool expected)
    {
        var manager = new TestManager([new ServiceStatusCacheItem(1, "device", status)]);
        Assert.Equal(expected, await manager.IsOnlineAsync(1));
    }

    private sealed class TestManager : ServiceStateManager
    {
        private readonly List<ServiceStatusCacheItem> items;

        public TestManager(List<ServiceStatusCacheItem> items) : base(Substitute.For<IChatObjectManager>(),
            Substitute.For<IDistributedCache<List<ServiceStatusCacheItem>, long>>())
        {
            this.items = items;
            var lazy = Substitute.For<IAbpLazyServiceProvider>();
            lazy.LazyGetService<ILogger>(Arg.Any<Func<IServiceProvider, ILogger>>()).Returns(NullLogger.Instance);
            LazyServiceProvider = lazy;
        }

        public override Task<List<ServiceStatusCacheItem>> GetAsync(long chatObjectId) => Task.FromResult(items);
    }
}
