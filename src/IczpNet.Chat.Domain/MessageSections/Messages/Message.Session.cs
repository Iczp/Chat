using IczpNet.Chat.Favorites;
using IczpNet.Chat.MessageSections.Counters;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.Scopeds;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
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


    [InverseProperty(nameof(Favorite.Message))]
    public virtual IList<Favorite> FavoriteList { get; set; }

    [InverseProperty(nameof(Scoped.Message))]
    public virtual IList<Scoped> ScopedList { get; set; }

    [ForeignKey(nameof(SessionId))]
    public virtual Session Session { get; protected set; }

    /// <summary>
    /// sender session unit
    /// </summary>
    [ForeignKey(nameof(SessionUnitId))]
    public virtual SessionUnit SessionUnit { get; protected set; }

    [InverseProperty(nameof(SessionSections.Sessions.Session.LastMessage))]
    public virtual IList<Session> SessionList { get; set; }

    /// <summary>
    /// last message list
    /// </summary>
    [InverseProperty(nameof(SessionSections.SessionUnits.SessionUnit.LastMessage))]
    public virtual List<SessionUnit> LastMessageSessionUnitList { get; protected set; }

    /// <summary>
    /// last message list
    /// </summary>
    [InverseProperty(nameof(SessionSections.SessionUnits.SessionUnit.ReadedMessage))]
    public virtual List<SessionUnit> ReadedMessageSessionUnitList { get; protected set; }

    public virtual ReadedCounter ReadedCounter { get; protected set; } = new ReadedCounter();
    public virtual OpenedCounter OpenedCounter { get; protected set; } = new OpenedCounter();
    public virtual FavoritedCounter FavoritedCounter { get; protected set; } = new FavoritedCounter();

    internal virtual void SetSessionUnitCount(int sessionUnitCount)
    {
        SessionUnitCount = sessionUnitCount;
    }
}
