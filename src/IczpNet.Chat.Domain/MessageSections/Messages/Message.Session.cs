using IczpNet.Chat.SessionSections.OpenedRecorders;
using System.Collections.Generic;

namespace IczpNet.Chat.MessageSections.Messages;

public partial class Message
{
    public virtual IList<OpenedRecorder> OpenedRecorderList { get; set; }
}
