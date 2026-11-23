using System;
using System.Threading.Tasks;
using IczpNet.Chat.CallCenters;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.ServiceStates;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using NSubstitute;
using Xunit;

namespace IczpNet.Chat;

public class CallCenterTransfer_Tests
{
    private static ChatObject MakeObject(long id, ChatObjectTypeEnums type, long? parentId)
    {
        var value = new ChatObject("name", "code", new ChatObjectType(type.ToString()) { ObjectType = type }, parentId);
        typeof(ChatObject).GetProperty(nameof(ChatObject.Id))!.SetValue(value, id);
        return value;
    }

    [Fact]
    public async Task Waiter_cannot_transfer_to_a_different_shop_under_the_same_root()
    {
        var sessionUnitId = Guid.NewGuid();
        var unit = new SessionUnit(sessionUnitId, 0, 0, 0, 0, 0, 0, 0, 0);
        typeof(SessionUnit).GetProperty(nameof(SessionUnit.OwnerId))!.SetValue(unit, 11L);
        typeof(SessionUnit).GetProperty(nameof(SessionUnit.OwnerObjectType))!.SetValue(unit, ChatObjectTypeEnums.ShopWaiter);
        typeof(SessionUnit).GetProperty(nameof(SessionUnit.DestinationObjectType))!.SetValue(unit, ChatObjectTypeEnums.Personal);
        var units = Substitute.For<ISessionUnitManager>();
        units.GetAsync(sessionUnitId).Returns(unit);
        var objects = Substitute.For<IChatObjectManager>();
        objects.GetAsync(11).Returns(MakeObject(11, ChatObjectTypeEnums.ShopWaiter, 10));
        objects.GetAsync(10).Returns(MakeObject(10, ChatObjectTypeEnums.ShopKeeper, 1));
        objects.GetAsync(21).Returns(MakeObject(21, ChatObjectTypeEnums.ShopWaiter, 20));
        var manager = new CallCenterManager(units, Substitute.For<ISessionGenerator>(),
            Substitute.For<ISessionUnitIdGenerator>(), objects, Substitute.For<IMessageSender>(),
            Substitute.For<IServiceStateManager>());

        var error = await Assert.ThrowsAnyAsync<Exception>(() => manager.TransferToAsync(sessionUnitId, 21, false));
        Assert.Contains("same shop", error.Message);
        await units.DidNotReceiveWithAnyArgs().CreateIfNotContainsAsync(default, default, default, default);
    }
}
