using IczpNet.Chat.FavoriteMessages;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.ReadedRecorders;
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
    public virtual IList<ReadedRecorder> ReadedRecorderList { get; set; }


    [InverseProperty(nameof(FavoriteMessage.Message))]
    public virtual IList<FavoriteMessage> FavoriteMessageList { get; set; }

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

    internal virtual void SetSessionUnitCount(int sessionUnitCount)
    {
        SessionUnitCount = sessionUnitCount;
    }
}
