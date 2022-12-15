using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.Favorites;
using IczpNet.Chat.SessionSections.MessageReminders;
using IczpNet.Chat.SessionSections.OpenedRecorders;
using IczpNet.Chat.SessionSections.Sessions;
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

    [InverseProperty(nameof(MessageReminder.Message))]
    public virtual IList<MessageReminder> MessageReminderList { get; set; }

    [InverseProperty(nameof(Session.Message))]
    public virtual IList<Session> SessionList { get; set; }
    


}
