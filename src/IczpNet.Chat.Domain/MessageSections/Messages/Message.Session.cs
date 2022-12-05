using IczpNet.Chat.SessionSections.OpenedRecorderMessages;
using IczpNet.Chat.SessionSections.OpenedRecorders;
using System.Collections.Generic;

namespace IczpNet.Chat.Messages
{
    public partial class Message
    {
        public virtual IList<OpenedRecorder> OpenedRecorderList { get; set; }
    }
}
