using System.Collections.Generic;

namespace IczpNet.Chat.MessageSections.Counters;

public abstract class MessageCounterArgs
{
    public virtual List<long> MessageIdList { get; set;}
}
