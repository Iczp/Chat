using IczpNet.Chat.Commands;
using System;

namespace IczpNet.Chat.CommandPayloads;

[Command(CommandConsts.IncrementCompleted)]
[Serializable]
public class IncrementCompletedCommandPayload
{
    public virtual long MessageId { get; set; }
}
