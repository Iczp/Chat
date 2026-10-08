using System;
using System.Collections.Generic;
using IczpNet.Chat.DeletedRecorders;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.Specifications;
using Xunit;

namespace IczpNet.Chat;

public class MessageVisibility_Tests
{
    private static void Set(object target, string name, object value)
        => target.GetType().GetProperty(name).SetValue(target, value);

    [Fact]
    public void Visibility_covers_private_participants_deleted_records_and_all_history_boundaries()
    {
        var session = new Session(Guid.NewGuid(), "visibility", default);
        var unit = new SessionUnit(Guid.NewGuid(), 0, 0, 0, 0, 0, 0, 0, 0);
        var setting = new SessionUnitSetting();
        Set(unit, nameof(SessionUnit.OwnerId), 1L);
        Set(unit, nameof(SessionUnit.Setting), setting);
        Set(session, nameof(Session.UnitList), new List<SessionUnit> { unit });
        var other = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 2, SessionId = session.Id };
        var message = new Message(other) { CreationTime = new DateTime(2026, 10, 6, 1, 0, 0) };
        Set(message, nameof(Message.Id), 103L);
        Set(message, nameof(Message.Session), session);
        Set(message, nameof(Message.DeletedList), new List<DeletedRecorder>());
        var visible = new OwnerMessageSpecification(1, unit.Id).ToExpression().Compile();
        Assert.True(visible(message));

        Set(message, nameof(Message.IsPrivate), true);
        Set(message, nameof(Message.ReceiverSessionUnitId), Guid.NewGuid());
        Assert.False(visible(message));
        Set(message, nameof(Message.ReceiverSessionUnitId), unit.Id);
        Assert.True(visible(message));
        Set(message, nameof(Message.IsPrivate), false);

        message.DeletedList.Add(new DeletedRecorder(unit.Id, 103));
        Assert.False(visible(message));
        message.DeletedList.Clear();
        Set(setting, nameof(SessionUnitSetting.ClearTime), message.CreationTime);
        Assert.False(visible(message));
        Set(setting, nameof(SessionUnitSetting.ClearTime), null);
        Set(setting, nameof(SessionUnitSetting.HistoryFristTime), message.CreationTime.AddSeconds(1));
        Assert.False(visible(message));
        Set(setting, nameof(SessionUnitSetting.HistoryFristTime), null);
        Set(setting, nameof(SessionUnitSetting.HistoryLastTime), message.CreationTime);
        Assert.False(visible(message));
        Set(setting, nameof(SessionUnitSetting.HistoryLastTime), null);
        Set(setting, nameof(SessionUnitSetting.StartMessageId), 103L);
        Assert.False(visible(message));
        Set(setting, nameof(SessionUnitSetting.StartMessageId), null);
        Set(setting, nameof(SessionUnitSetting.EndMessageId), 102L);
        Assert.False(visible(message));
        Set(setting, nameof(SessionUnitSetting.EndMessageId), null);
        Set(setting, nameof(SessionUnitSetting.IsKilled), true);
        Assert.False(visible(message));
    }
}
