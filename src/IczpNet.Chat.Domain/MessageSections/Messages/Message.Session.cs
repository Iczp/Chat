using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.MessageSections.Counters;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.Scopeds;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionUnits;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.MessageSections.Messages;

public partial class Message
{
    [InverseProperty(nameof(OpenedRecorder.Message))]
    public virtual IList<OpenedRecorder> OpenedRecorderList { get; set; }

    [InverseProperty(nameof(ReadedRecorder.Message))]
    public virtual IList<ReadedRecorder> ReadedRecorderList { get; set; }  = new List<ReadedRecorder>();


    [InverseProperty(nameof(FavoritedRecorder.Message))]
    public virtual IList<FavoritedRecorder> FavoriteList { get; set; }

    [InverseProperty(nameof(Scoped.Message))]
    public virtual IList<Scoped> ScopedList { get; set; }

    [ForeignKey(nameof(SessionId))]
    public virtual Session Session { get; protected set; }

    /// <summary>
    /// sender session unit
    /// </summary>
    [ForeignKey(nameof(SenderSessionUnitId))]
    public virtual SessionUnit SenderSessionUnit { get; protected set; }

    [InverseProperty(nameof(SessionSections.Sessions.Session.LastMessage))]
    public virtual IList<Session> SessionList { get; set; }

    /// <summary>
    /// last message list
    /// </summary>
    [InverseProperty(nameof(SessionUnits.SessionUnit.LastMessage))]
    public virtual List<SessionUnit> LastMessageSessionUnitList { get; protected set; }

    /// <summary>
    /// last message list
    /// </summary>
    [InverseProperty(nameof(SessionUnitSetting.ReadedMessage))]
    public virtual List<SessionUnitSetting> ReadedMessageSessionUnitList { get; protected set; }

    public virtual ReadedCounter ReadedCounter { get; protected set; } = new ReadedCounter();
    public virtual OpenedCounter OpenedCounter { get; protected set; } = new OpenedCounter();
    public virtual FavoritedCounter FavoritedCounter { get; protected set; } = new FavoritedCounter();

    internal virtual void SetSessionUnitCount(int sessionUnitCount)
    {
        SessionUnitCount = sessionUnitCount;
    }
}
