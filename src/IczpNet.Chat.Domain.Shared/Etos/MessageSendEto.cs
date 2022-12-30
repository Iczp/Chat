using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Etos
{
    public class MessageSendEto
    {
        public List<Guid> TargetIdList { get; set; }

        public object Payload { get; set; }

    }
}
