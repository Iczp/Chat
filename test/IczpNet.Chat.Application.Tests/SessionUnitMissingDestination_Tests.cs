using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using NSubstitute;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace IczpNet.Chat;

public class SessionUnitMissingDestination_Tests
{
    [Fact]
    public async Task Missing_destination_does_not_break_other_items_or_remove_the_session()
    {
        var constructor = typeof(SessionUnitCacheAppService).GetConstructors().Single();
        var args = constructor.GetParameters().Select(x => Substitute.For([x.ParameterType], [])).ToArray();
        var service = (SessionUnitCacheAppService)constructor.Invoke(args);
        var objects = Substitute.For<IChatObjectManager>();
        var existing = new ChatObjectInfo { Id = 12 };
        objects.GetManyByCacheAsync(Arg.Any<List<long>>()).Returns(new List<ChatObjectInfo> { null, existing });
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<IChatObjectManager>().Returns(objects);
        service.LazyServiceProvider = lazy;
        var missing = new SessionUnitFriendDto { DestinationId = 11 };
        var present = new SessionUnitFriendDto { DestinationId = 12 };
        var items = new List<SessionUnitFriendDto> { missing, present };

        var method = typeof(SessionUnitCacheAppService).GetMethod("FillDestinationAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
        await (Task)method.Invoke(service, [items])!;

        Assert.Equal(2, items.Count);
        Assert.Null(missing.Destination);
        Assert.Same(existing, present.Destination);
        await objects.Received(1).GetManyByCacheAsync(Arg.Is<List<long>>(ids => ids.Count == 2 && ids.Contains(11) && ids.Contains(12)));
    }
}
