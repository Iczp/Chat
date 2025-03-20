using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using System;

namespace IczpNet.Chat.FavoritedRecorders;

public class FavoritedRecorder : BaseRecorder
{
    public virtual long Size { get; protected set; }

    public virtual MessageTypes MessageType { get; protected set; }

    protected FavoritedRecorder() { }

    public FavoritedRecorder(SessionUnit sessionUnit, Message message, string deviceId) : base(sessionUnit, message.Id, deviceId)
    {
        Size = message.Size;
        MessageType = message.MessageType;
    }

    public FavoritedRecorder(Guid sessionUnitId, long messageId) : base(sessionUnitId, messageId) { }
}
