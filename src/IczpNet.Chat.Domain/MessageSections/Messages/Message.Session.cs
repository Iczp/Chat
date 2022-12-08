using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.Favorites;
using IczpNet.Chat.SessionSections.OpenedRecorders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.MessageSections.Messages;

public partial class Message
{
    [InverseProperty(nameof(OpenedRecorder.Message))]
    public virtual IList<OpenedRecorder> OpenedRecorderList { get; set; }


    [InverseProperty(nameof(FavoriteMessage.Message))]
    public virtual IList<FavoriteMessage> FavoriteMessageList { get; set; }

    internal void SetMessageType(MessageTypes messageType)
    {
        MessageType = messageType;
    }
}
